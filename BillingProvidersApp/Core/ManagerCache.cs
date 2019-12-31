using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Reflection;

namespace BillingProvidersApp.Core
{
    /// <summary>
    /// Handles exceptions throwed in <see cref="ObjectManager" /> objects.
    /// </summary>
    /// <param name="exceptionInfo">Information about throwed exception.</param>
    public delegate void ObjectExceptionHandler(ObjectExceptionInfo exceptionInfo);


    /// <summary>
    /// Static cache of all existing BO managers.
    /// </summary>
    sealed public class ManagerCache
    {
        static Hashtable constructorCache;  // cache of all object constructors with given ManagerAttribute(itemType)
        static Hashtable managerCache;      // managers cache
        static ArrayList managerFactories;  // external factories of managers


        /// <summary>
        /// This class is non-instantiatable.
        /// </summary>
        private ManagerCache(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Creates ManagerCache class.
        /// </summary>
        static ManagerCache()
        {
            managerFactories = ArrayList.Synchronized(new ArrayList());
            constructorCache = Hashtable.Synchronized(new Hashtable());
            RegisterAssemblyTypes(Assembly.GetExecutingAssembly());
            managerCache = Hashtable.Synchronized(new Hashtable(constructorCache.Count));

            objectExceptionHandlers = Hashtable.Synchronized(new Hashtable());
        }


        #region Handling exceptions globally (uses HttpContext.Current)

        static Hashtable objectExceptionHandlers;  // ObjectExceptionHandler objects for different HttpContext

        public static IHttpContextAccessor _httpContextAccessor { get; private set; }


        /// <summary>
        /// Fired if exception is throwed in any <see cref="ObjectManager" />.
        /// </summary>
        static public event ObjectExceptionHandler ObjectException
        {
            add
            {
                if (_httpContextAccessor != null)
                {
                    objectExceptionHandlers[_httpContextAccessor] = value;
                }
            }
            remove
            {
                if (_httpContextAccessor != null)
                {
                    objectExceptionHandlers.Remove(_httpContextAccessor);
                }
            }
        }

        /// <summary>
        /// Gives <see cref="ObjectExceptionInfo" /> object to handlers.
        /// </summary>
        /// <param name="itemType">Type of business object which caused an exception.</param>
        /// <param name="action">Action performed on business object.</param>
        /// <param name="errorMessage">Error message to display (may vary between different objects).</param>
        /// <param name="innerException">Exception actually throwed.</param>
        public static void HandleObjectException(Type itemType, ObjectAction action, string errorMessage, Exception innerException)
        {
            if (_httpContextAccessor == null)
            {
                return;
            }
            ObjectExceptionHandler handler = (ObjectExceptionHandler)objectExceptionHandlers[_httpContextAccessor];
            if (handler != null)
            {
                handler(new ObjectExceptionInfo(itemType, action, errorMessage, innerException));
            }
        }

        #endregion Handling exceptions globally (uses HttpContext.Current)

        /// <summary>
        /// Registers manager metadata in cache.
        /// </summary>
        /// <param name="managerType">Manager type to register.</param>
        static public void RegisterManagerType(Type managerType)
        {
            if (managerType.IsSubclassOf(typeof(ObjectManager)))
            {
                ObjectManagerAttribute objectManagerAttribute = (ObjectManagerAttribute)Attribute.GetCustomAttribute(managerType, typeof(ObjectManagerAttribute));
                if (objectManagerAttribute != null)
                {
                    // Add to cache
                    constructorCache[objectManagerAttribute.ItemType] = managerType.GetConstructor(
                      BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                      null,
                      Type.EmptyTypes,
                      null);
                }
            }
        }

        /// <summary>
        /// Registers managers metadata from assembly in cache.
        /// </summary>
        /// <param name="assembly">Assembly with manager objects.</param>
        static public void RegisterAssemblyTypes(Assembly assembly)
        {
            // Go thru all types in assembly and find all of them inherited from ObjectManager with ManagerAttribute
            Type[] assemblyTypes = assembly.GetTypes();
            foreach (Type type in assemblyTypes)
            {
                // Find type which is ObjectManager child and have ManagerAttribute attribute
                RegisterManagerType(type);

                // Also maybe register manager factory
                if (type.GetInterface(typeof(IManagerFactory).FullName) != null)
                {
                    ConstructorInfo managerFactoryConstructor = type.GetConstructor(
                      BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                      null,
                      Type.EmptyTypes,
                      null);
                    IManagerFactory managerFactory = (IManagerFactory)managerFactoryConstructor.Invoke(null);
                    AddManagerFactory(managerFactory);
                }
            }
        }

        /// <summary>
        /// Adds manager factory for manager.
        /// </summary>
        /// <param name="managerFactory">External factory of managers.</param>
        static public void AddManagerFactory(IManagerFactory managerFactory)
        {
            if (!managerFactories.Contains(managerFactory))
            {
                managerFactories.Add(managerFactory);
            }
        }

        /// <summary>
        /// Deletes manager factory for manager.
        /// </summary>
        /// <param name="managerFactory">External factory of managers.</param>
        static public void RemoveManagerFactory(IManagerFactory managerFactory)
        {
            managerFactories.Remove(managerFactory);
        }

        /// <summary>
        /// Adds new manager into managers cache (for use in ObjectManager constructor).
        /// </summary>
        /// <param name="manager">Manager to add.</param>
        static internal void AddManager(IObjectManager manager)
        {
            managerCache[manager.ItemType] = manager;
        }

        /// <summary>
        /// Returns manager for managing given type of items.
        /// </summary>
        /// <param name="itemType">Type of managed items.</param>
        /// <returns>Manager or null if nothing was found.</returns>
        static public IObjectManager GetManager(Type itemType)
        {
            if (itemType == null)
                throw new ArgumentNullException("itemType");

            IObjectManager manager = (IObjectManager)managerCache[itemType];
            if (manager == null)
            {
                // Try to find this manager constructor
                ConstructorInfo managerConstructor = (ConstructorInfo)constructorCache[itemType];
                if (managerConstructor != null)
                {
                    manager = (IObjectManager)managerConstructor.Invoke(null);
                }
                else
                {
                    // Try to create manager with factory
                    lock (managerFactories.SyncRoot)
                    {
                        foreach (IManagerFactory managerFactory in managerFactories)
                        {
                            if (managerFactory != null)
                            {
                                manager = managerFactory.GetManager(itemType);
                                if (manager != null) break;
                            }
                        }
                    }
                }
                if (manager != null)
                {
                    AddManager(manager);
                }
            }
            return manager;
        }

        /// <summary>
        /// Clears all cached objects on all existing managers.
        /// </summary>
        static public void ClearObjectsCache()
        {
            lock (managerCache.SyncRoot)
            {
                foreach (ObjectManager manager in managerCache.Values)
                {
                    manager.ClearObjectsCache();
                }
            }
        }
    }
}
