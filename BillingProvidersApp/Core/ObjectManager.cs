using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Data.SqlClient;
using System.Configuration;

namespace BillingProvidersApp.Core
{
    /// <summary>
    /// Delegate for executing <see cref="SqlCommand"/> by the method <see cref="CommunicationMethod"/> and filling <see cref="DataTable"/> with results.
    /// </summary>
    public delegate DataTable ExecuteDataTableDelegate(SqlCommand command, CommunicationMethod method);

    /// <summary>
    /// Delegate for getting <see cref="SqlCommand"/>.
    /// </summary>
    public delegate SqlCommand GetSqlCommandDelegate();
    public delegate void ObjectOperationWarningHandler(BusinessObject businessObject, string message, out bool continueExecution);
    public interface IObjectManager
    {
        event ObjectOperationWarningHandler ObjectOperationWarning;
        bool EnableCaching { get; }

        /// <summary>
        /// Manager items type.
        /// </summary>
        Type ItemType { get; }

        /// <summary>
        /// Items primary key field name.
        /// </summary>
        string PrimaryKeyField { get; }

        /// <summary>
        /// Items primary key .NET type.
        /// </summary>
        Type PrimaryKeyType { get; }

        void CanContinueCheck(BusinessObject obj, string message, Exception exception);

        /// <summary>
        /// Clears all cached objects.
        /// </summary>
        void ClearObjectsCache();

        /// <summary>
        /// Creates new object.
        /// </summary>
        /// <returns>Newly created object.</returns>
        object CreateObject();

        /// <summary>
        /// Returns object by given ID from either cache or DB.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="primaryKeyValue">Object primary key value in database.</param>
        /// <returns>Object with given primary key value or null if nothing.</returns>
        object GetObject(CommunicationMethod method, object primaryKeyValue);

        /// <summary>
        /// Returns object by given ID from either cache or DB.
        /// </summary>
        /// <param name="primaryKeyValue">Object primary key value in database.</param>
        /// <returns>Object with given primary key value or null if nothing.</returns>
        object GetObject(object primaryKeyValue);

        /// <summary>
        /// Returns object by given member value from either cache or DB.
        /// </summary>
        /// <param name="memberName">Object member name.</param>
        /// <param name="value">Object member value.</param>
        /// <returns>Object with given member value or null if nothing.</returns>
        object GetObject(string memberName, object value);

        /// <summary>
        /// Returns object by given member value from either cache or DB.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="memberName">Object member name.</param>
        /// <param name="value">Object member value.</param>
        /// <returns>Object with given member value or null if nothing.</returns>
        object GetObject(CommunicationMethod method, string memberName, object value);

        /// <summary>
        /// Saves object to DB. Alters or creates new (depends on object primary key).
        /// </summary>
        /// <param name="obj">Object to save.</param>
        /// <returns>True if save operation succeeded.</returns>
        bool SaveObject(object obj);

        /// <summary>
        /// Saves object to DB. Alters or creates new (depends on object primary key).
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="obj">Object to save.</param>
        /// <returns>True if save operation succeeded.</returns>
        bool SaveObject(CommunicationMethod method, object obj);

        /// <summary>
        /// Deletes object from DB and local objects cache.
        /// </summary>
        /// <param name="obj">Object to delete.</param>
        /// <returns>True if delete operation succeeded.</returns>
        bool DeleteObject(object obj);

        /// <summary>
        /// Deletes object from DB and local objects cache.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="obj">Object to delete.</param>
        /// <returns>True if delete operation succeeded.</returns>
        bool DeleteObject(CommunicationMethod method, object obj);

        /// <summary>
        /// Loads objects from DB by given member values.
        /// </summary>
        /// <param name="memberItems">Member name-value pairs.</param>
        /// <returns>Loaded objects in DataTable.</returns>
        DataTable LoadObjects(params object[] memberItems);

        /// <summary>
        /// Loads objects from DB by given member values.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="memberItems">Member name-value pairs.</param>
        /// <returns>Loaded objects in DataTable.</returns>
        DataTable LoadObjects(CommunicationMethod method, params object[] memberItems);

        /// <summary>
        /// Loads objects from DB by given member values.
        /// </summary>
        /// <param name="excludeNulls">If <c>true</c>, parameters from <c>memberItems</c> value of which is null will not be included to query.</param>
        /// <param name="memberItems">Member name-value pairs.</param>
        /// <returns>Loaded objects in DataTable.</returns>
        DataTable LoadObjects(bool excludeNulls, params object[] memberItems);

        /// <summary>
        /// Loads objects from DB by given member values.
        /// </summary>
        /// <param name="excludeNulls">If <c>true</c>, parameters from <c>memberItems</c> value of which is null will not be included to query.</param>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="memberItems">Member name-value pairs.</param>
        /// <returns>Loaded objects in DataTable.</returns>
        DataTable LoadObjects(bool excludeNulls, CommunicationMethod method, params object[] memberItems);

        /// <summary>
        /// Loads objects from DB.
        /// </summary>
        /// <returns>Loaded objects in DataTable.</returns>
        DataTable LoadObjects();

        /// <summary>
        /// Loads objects from DB.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <returns>Loaded objects in DataTable.</returns>
        DataTable LoadObjects(CommunicationMethod method);

        /// <summary>
        /// Restores object from <paramref name="row" />.
        /// </summary>
        /// <param name="row">Data row with object properties.</param>
        /// <returns>Restored object.</returns>
        object RestoreObject(DataRow row);

        /// <summary>
        /// Restores objects from <paramref name="table" />.
        /// </summary>
        /// <param name="table">Data table with objects.</param>
        /// <returns>Restored object.</returns>
        ArrayList RestoreObjects(DataTable table);

        /// <summary>
        /// Returns object of type <paramref name="referencedName" /> referenced to given <paramref name="obj" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="referencedName">Referenced object type name.</param>
        /// <returns>Referenced object value.</returns>
        object GetReferencedObject(object obj, string referencedName);

        /// <summary>
        /// Returns object of type <paramref name="referencedType" /> referenced to given <paramref name="obj" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="referencedType">Referenced object type.</param>
        /// <returns>Referenced object value.</returns>
        object GetReferencedObject(object obj, Type referencedType);

        /// <summary>
        /// Returns object by using <paramref name="manager" /> referenced to given <paramref name="obj" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="manager">Referenced object manager.</param>
        /// <returns>Referenced object value.</returns>
        object GetReferencedObject(object obj, IObjectManager manager);

        object GetReferencedObject(object obj, IObjectManager manager, string referencePropertyName);
        object GetReferencedObject(object obj, IObjectManager manager, MemberInfo memberInfo);

        /// <summary>
        /// Returns property/field value of given <paramref name="obj" /> by given <paramref name="memberName" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="memberName">Member name (supports dots).</param>
        /// <returns>Object member value.</returns>
        object GetMemberValue(object obj, string memberName);

        /// <summary>
        /// Returns property/field value of given <paramref name="obj" /> by given <paramref name="memberInfo" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="memberInfo">Member info.</param>
        /// <returns>Object member value.</returns>
        object GetMemberValue(object obj, MemberInfo memberInfo);

        /// <summary>
        /// Sets property/field value of given <paramref name="obj" /> by given <paramref name="memberInfo" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="memberInfo">Member info.</param>
        /// <param name="newValue">New member value.</param>
        void SetMemberValue(object obj, MemberInfo memberInfo, object newValue);

        /// <summary>
        /// Returns property/field value of given <paramref name="obj" /> by given DB <paramref name="fieldName" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="fieldName">DB field name.</param>
        /// <returns>Object member value.</returns>
        object GetFieldValue(object obj, string fieldName);

        /// <summary>
        /// Sets property/field value of given <paramref name="obj" /> by given DB <paramref name="fieldName" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="fieldName">DB field name.</param>
        /// <param name="newValue">New member value.</param>
        void SetFieldValue(object obj, string fieldName, object newValue);

        /// <summary>
        /// Sets property/field value of given <paramref name="obj" /> by given <paramref name="memberName" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="memberName">Member name (supports dots).</param>
        /// <param name="newValue">New member value.</param>
        void SetMemberValue(object obj, string memberName, object newValue);

        /// <summary>
        /// Sets properties/fields values of given <paramref name="obj" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="memberData">Array of data formed like name-value-name-value-...</param>
        void SetMemberValues(object obj, params object[] memberData);

        /// <summary>
        /// Returns type of given member.
        /// </summary>
        /// <param name="memberInfo">Member info.</param>
        /// <returns>Member type.</returns>
        Type GetMemberType(MemberInfo memberInfo);

        /// <summary>
        /// Returns type of given member.
        /// </summary>
        /// <param name="memberName">Member name.</param>
        /// <returns>Member type.</returns>
        Type GetMemberType(string memberName);

        /// <summary>
        /// Returns member info.
        /// </summary>
        /// <param name="memberName">Member name.</param>
        /// <returns>Member info.</returns>
        MemberInfo GetMemberInfo(string memberName);

        /// <summary>
        /// Returns member info.
        /// </summary>
        /// <param name="fieldName">Field name.</param>
        /// <returns>Member info.</returns>
        MemberInfo GetMemberInfoByField(string fieldName);


        string[] ErrorMessages { get; }


        bool InternalSaveObject(object obj, CommunicationMethod method);

        bool InternalDeleteObject(object obj, CommunicationMethod method);

    }
    /// <summary>
	/// Base class for object managers. Implements basic caching/working with DB operations.
	/// </summary>
	abstract public class ObjectManager : IObjectManager
    {
        /// <summary>
        /// Type of items created by this manager.
        /// </summary>
        internal Type itemType;

        ConstructorInfo itemConstructorInfo;       // item constructor
        Hashtable mapAttributesHash;         // hash for retrieving PropertyInfo/FieldInfo by given DB field name
        Hashtable memberHash;                // hash of all MemberInfo by given member name
        Hashtable nameToFieldHash;           // hash for assigning member name with field name
        Hashtable referenceTypeHash;         // hash of referenced members by type
        Hashtable referenceNameHash;         // hash of referenced types by name
        Hashtable mapReferenceMemberInfos;
        string primaryKeyName = "ID";  // name of primary key field (needed to be present)
        Type primaryKeyType;            // primary key .NET type (used oftenly)
        string[] errorMessages;             // error messages array by ObjectAction.

        Hashtable itemsCache;  // cache of loaded items

        ExecuteDataTableDelegate executeDataTableCallback;
        GetSqlCommandDelegate getItemLoadCommandCallback;
        GetSqlCommandDelegate getItemSaveCommandCallback;
        GetSqlCommandDelegate getItemDeleteCommandCallback;
        GetSqlCommandDelegate getItemsLoadCommandCallback;
        GetSqlCommandDelegate getUnfilteredItemsLoadCommandCallback;

        Exception lastException;

        bool enableCaching;

        public event ObjectOperationWarningHandler ObjectOperationWarning;

        internal void WarningNotify(BusinessObject businessObject, string message, out bool canContinue)
        {
            canContinue = false;
            if (ObjectOperationWarning != null)
            {
                ObjectOperationWarning(businessObject, message, out canContinue);
            }
        }

        public void CanContinueCheck(BusinessObject obj, string message, Exception exception)
        {
            string warningMessage = String.Empty;

            if (message != null && message.Length > 0)
            {
                warningMessage += message;
            }
            if (exception != null)
            {
                if (warningMessage.Length > 0)
                {
                    warningMessage += ": ";
                }
                warningMessage += exception.Message;
            }

            bool canContinue;
            WarningNotify(obj, warningMessage, out canContinue);
            if (!canContinue)
            {
                if (exception != null)
                {
                    throw (exception);
                }
                else
                {
                    throw (new Exception(warningMessage));
                }
            }
        }


        /// <summary>
        /// Creates new object manager instance for items of given type.
        /// </summary>
        protected ObjectManager() : this(null) { }

        /// <summary>
        /// Creates new object manager instance for items of given type.
        /// </summary>
        /// <param name="itemType">Items type.</param>
        protected ObjectManager(Type itemType)
        {
            // Get item type attribute. If no was found, throw an exception
            if (itemType == null)
            {
                ObjectManagerAttribute objectManagerAttribute = (ObjectManagerAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(ObjectManagerAttribute));
                if (objectManagerAttribute == null)
                {
                    throw new Exception("Manager must have ObjectManagerAttribute applied.");
                }
                itemType = objectManagerAttribute.ItemType;
            }
            if (itemType == null)
            {
                throw new Exception("Manager must have itemType.");
            }

            this.itemType = itemType;
            itemsCache = Hashtable.Synchronized(new Hashtable());

            InitItemMetadata();

            ConfigureCaching();

            // Finally add this manager to cache
            ManagerCache.AddManager(this);
        }

        internal Exception GetLastException()
        {
            return lastException;
        }

        #region Caching Management

        public bool EnableCaching
        {
            get { return enableCaching; }
        }

        private void ConfigureCaching()
        {
            try
            {
                enableCaching =
                    Convert.ToBoolean(ConfigurationManager.AppSettings["EnableCaching"]);
            }
            catch
            {//Caching is disabled
            }
        }

        #endregion

        #region Properties for work with DB

        /// <summary>
        /// Callback function for filling DataTable from command.
        /// </summary>
        protected ExecuteDataTableDelegate ExecuteDataTableCallback
        {
            get { return executeDataTableCallback; }
            set { executeDataTableCallback = value; }
        }

        /// <summary>
        /// Callback function for getting SqlCommand for loading item from DB.
        /// </summary>
        protected GetSqlCommandDelegate GetItemLoadCommandCallback
        {
            get { return getItemLoadCommandCallback; }
            set { getItemLoadCommandCallback = value; }
        }

        /// <summary>
        /// Callback function for getting SqlCommand for saving item to DB.
        /// </summary>
        protected GetSqlCommandDelegate GetItemSaveCommandCallback
        {
            get { return getItemSaveCommandCallback; }
            set { getItemSaveCommandCallback = value; }
        }

        /// <summary>
        /// Callback function for getting SqlCommand for deleting item from DB.
        /// </summary>
        protected GetSqlCommandDelegate GetItemDeleteCommandCallback
        {
            get { return getItemDeleteCommandCallback; }
            set { getItemDeleteCommandCallback = value; }
        }

        /// <summary>
        /// Callback function for getting SqlCommand for loading items from DB.
        /// </summary>
        protected GetSqlCommandDelegate GetItemsLoadCommandCallback
        {
            get { return getItemsLoadCommandCallback; }
            set { getItemsLoadCommandCallback = value; }
        }

        /// <summary>
        /// Callback function for getting SqlCommand for loading items from DB without filtering.
        /// </summary>
        protected GetSqlCommandDelegate GetUnfilteredItemsLoadCommandCallback
        {
            get { return getUnfilteredItemsLoadCommandCallback; }
            set { getUnfilteredItemsLoadCommandCallback = value; }
        }

        #endregion


        #region Creating objects/retrieving objects from cache

        /// <summary>
        /// Clears all cached objects.
        /// </summary>
        virtual public void ClearObjectsCache()
        {
            itemsCache.Clear();
        }

        /// <summary>
        /// Creates new object.
        /// </summary>
        /// <returns>Newly created object.</returns>
        public object CreateObject()
        {
            object obj = itemConstructorInfo.Invoke(null);

            // Maybe set manager for this object (if it's a BusinessObject)
            BusinessObject businessObj = obj as BusinessObject;
            if (businessObj != null)
            {
                businessObj.manager = this;
            }

            return obj;
        }

        /// <summary>
        /// Returns object by given ID from either cache or DB.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="primaryKeyValue">Object primary key value in database.</param>
        /// <returns>Object with given primary key value or null if nothing.</returns>
        public object GetObject(CommunicationMethod method, object primaryKeyValue)
        {
            if (!enableCaching)
            {
                return LoadObject(method, primaryKeyValue);
            }

            object item = itemsCache[primaryKeyValue];
            if (item == null)
            {
                // Load object from DB, put it in cache
                item = LoadObject(method, primaryKeyValue);
            }
            return item;
        }

        /// <summary>
        /// Returns object by given ID from either cache or DB.
        /// </summary>
        /// <param name="primaryKeyValue">Object primary key value in database.</param>
        /// <returns>Object with given primary key value or null if nothing.</returns>
        public object GetObject(object primaryKeyValue)
        {
            return GetObject(CommunicationMethod.Default, primaryKeyValue);
        }

        /// <summary>
        /// Returns object by given member value from either cache or DB.
        /// </summary>
        /// <param name="memberName">Object member name.</param>
        /// <param name="value">Object member value.</param>
        /// <returns>Object with given member value or null if nothing.</returns>
        public object GetObject(string memberName, object value)
        {
            return GetObject(CommunicationMethod.Default, memberName, value);
        }

        /// <summary>
        /// Returns object by given member value from either cache or DB.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="memberName">Object member name.</param>
        /// <param name="value">Object member value.</param>
        /// <returns>Object with given member value or null if nothing.</returns>
        public object GetObject(CommunicationMethod method, string memberName, object value)
        {
            // Retrieves MemberInfo
            MemberInfo memberInfo = (MemberInfo)memberHash[memberName];

            if (enableCaching)
            {
                // Try to find this item in cache
                lock (itemsCache.SyncRoot)
                {
                    foreach (object cachedItem in itemsCache.Values)
                    {
                        if (GetMemberValue(cachedItem, memberInfo).Equals(value))
                        {
                            return cachedItem;
                        }
                    }
                }
            }

            // Finally load it from database
            return LoadObject(method, memberName, value);
        }

        #endregion

        #region Loading objects from DB/saving them to DB

        /// <summary>
        /// Loads object from database by given ID.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="primaryKeyValue">Object primary key value in database.</param>
        /// <returns>Loaded object or null.</returns>
        protected object LoadObject(CommunicationMethod method, object primaryKeyValue)
        {
            // Fill command parameters and load
            SqlCommand itemLoadCommand = GetItemLoadCommandCallback();
            SetCommandParameter(itemLoadCommand, primaryKeyName, primaryKeyValue);
            return LoadObject(method, itemLoadCommand);
        }

        /// <summary>
        /// Loads object from database by given member value.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="memberName">Object member name.</param>
        /// <param name="value">Object member value.</param>
        /// <returns>Loaded object or null.</returns>
        protected object LoadObject(CommunicationMethod method, string memberName, object value)
        {
            // Fill command parameters and load
            SqlCommand itemLoadCommand = GetItemLoadCommandCallback();
            string fieldName = (string)nameToFieldHash[memberName];
            SetCommandParameter(itemLoadCommand, fieldName, value);
            return LoadObject(method, itemLoadCommand);
        }

        /// <summary>
        /// Loads object from database.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="command">SqlCommand used to load object.</param>
        /// <returns>Loaded object or null.</returns>
        protected object LoadObject(CommunicationMethod method, SqlCommand command)
        {
            // Load object from DB
            using (command)
            {
                DataTable objectTable = ExecuteDataTable(command, ObjectAction.LoadObject, method);
                return objectTable.Rows.Count != 0 ? RestoreObject(objectTable.Rows[0]) : null;
            }
        }

        /// <summary>
        /// Saves object to DB. Alters or creates new (depends on object primary key).
        /// </summary>
        /// <param name="obj">Object to save.</param>
        /// <returns>True if save operation succeeded.</returns>
        public bool SaveObject(object obj)
        {
            return SaveObject(CommunicationMethod.Default, obj);
        }

        /// <summary>
        /// Saves object to DB. Alters or creates new (depends on object primary key).
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="obj">Object to save.</param>
        /// <returns>True if save operation succeeded.</returns>
        public bool SaveObject(CommunicationMethod method, object obj)
        {
            // For BusinessObject call appropriate method
            BusinessObject businessObj = obj as BusinessObject;
            if (businessObj != null)
            {
                try
                {
                    return businessObj.Save();
                }
                catch (Exception ex)
                {
                    //ex = businessObj.HandleException(ex);
                    HandleObjectException(itemType, ObjectAction.SaveObject, ex);
                    return false;
                }
            }
            else
            {
                return InternalSaveObject(obj, method);
            }
        }

        /// <summary>
        /// Deletes object from DB and local objects cache.
        /// </summary>
        /// <param name="obj">Object to delete.</param>
        /// <returns>True if delete operation succeeded.</returns>
        public bool DeleteObject(object obj)
        {
            return DeleteObject(CommunicationMethod.Default, obj);
        }

        /// <summary>
        /// Deletes object from DB and local objects cache.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="obj">Object to delete.</param>
        /// <returns>True if delete operation succeeded.</returns>
        public bool DeleteObject(CommunicationMethod method, object obj)
        {
            // For BusinessObject call appropriate method
            BusinessObject businessObj = obj as BusinessObject;
            if (businessObj != null)
            {
                try
                {
                    return businessObj.Delete();
                }
                catch (Exception ex)
                {
                    //TODO fix it
                    //ex = businessObj.HandleException(ex);
                    HandleObjectException(itemType, ObjectAction.DeleteObject, ex);
                    return false;
                }
            }
            else
            {
                return InternalDeleteObject(obj, method);
            }
        }

        /// <summary>
        /// Loads objects from DB by given member values.
        /// </summary>
        /// <param name="memberItems">Member name-value pairs.</param>
        /// <returns>Loaded objects in DataTable.</returns>
        public DataTable LoadObjects(params object[] memberItems)
        {
            return LoadObjects(CommunicationMethod.Default, memberItems);
        }

        /// <summary>
        /// Loads objects from DB by given member values.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="memberItems">Member name-value pairs.</param>
        /// <returns>Loaded objects in DataTable.</returns>
        public DataTable LoadObjects(CommunicationMethod method, params object[] memberItems)
        {
            return LoadObjects(false, method, memberItems);
        }

        /// <summary>
        /// Loads objects from DB by given member values.
        /// </summary>
        /// <param name="excludeNulls">If <c>true</c>, parameters from <c>memberItems</c> value of which is null will not be included to query.</param>
        /// <param name="memberItems">Member name-value pairs.</param>
        /// <returns>Loaded objects in DataTable.</returns>
        public DataTable LoadObjects(bool excludeNulls, params object[] memberItems)
        {
            return LoadObjects(excludeNulls, CommunicationMethod.Default, memberItems);
        }

        /// <summary>
        /// Loads objects from DB by given member values.
        /// </summary>
        /// <param name="excludeNulls">If <c>true</c>, parameters from <c>memberItems</c> value of which is null will not be included to query.</param>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <param name="memberItems">Member name-value pairs.</param>
        /// <returns>Loaded objects in DataTable.</returns>
        public DataTable LoadObjects(bool excludeNulls, CommunicationMethod method, params object[] memberItems)
        {
            SqlCommand sqlCommand = (memberItems == null || memberItems.Length == 0) ?
                GetUnfilteredItemsLoadCommandCallback() : GetItemsLoadCommandCallback();

            return LoadObjects(excludeNulls, memberItems, sqlCommand, method);
        }

        /// <summary>
        /// Loads objects from DB.
        /// </summary>
        /// <returns>Loaded objects in DataTable.</returns>
        public DataTable LoadObjects()
        {
            return LoadObjects(CommunicationMethod.Default);
        }

        /// <summary>
        /// Loads objects from DB.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <returns>Loaded objects in DataTable.</returns>
        public DataTable LoadObjects(CommunicationMethod method)
        {
            return LoadObjects(GetUnfilteredItemsLoadCommandCallback(), method);
        }

        /// <summary>
        /// Loads objects from DB using given SqlCommand by given member values.
        /// </summary>
        /// <param name="memberItems">Member name-value pairs.</param>
        /// <param name="command">SqlCommand to use for objects loading.</param>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <returns>Loaded objects in DataTable.</returns>
        protected DataTable LoadObjects(bool excludeNulls, object[] memberItems, SqlCommand command, CommunicationMethod method)
        {
            // Set parameters
            for (int i = 0; i < memberItems.Length - 1; i += 2)
            {
                if (excludeNulls && ObjectManager.IsNull(memberItems[i + 1]))
                {
                    continue;
                }
                string fieldName = (string)nameToFieldHash[(string)memberItems[i]];
                if (fieldName == null)
                {
                    // If no field was found, just ignore and add this
                    fieldName = (string)memberItems[i];
                }
                SetCommandParameter(command, fieldName, memberItems[i + 1]);
            }
            return LoadObjects(command, method);
        }
        /// <summary>
        /// Loads objects from DB using given SqlCommand by given member values.
        /// </summary>
        /// <param name="memberItems">Member name-value pairs.</param>
        /// <param name="command">SqlCommand to use for objects loading.</param>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <returns>Loaded objects in DataTable.</returns>
        protected DataTable LoadObjects(object[] memberItems, SqlCommand command, CommunicationMethod method)
        {
            return LoadObjects(false, memberItems, command, method);
        }

        /// <summary>
        /// Loads objects from DB using given SqlCommand.
        /// </summary>
        /// <param name="command">SqlCommand to use for objects loading.</param>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <returns>Loaded objects in DataTable.</returns>
        protected DataTable LoadObjects(SqlCommand command, CommunicationMethod method)
        {
            using (command)
            {
                return ExecuteDataTable(command, ObjectAction.LoadObjects, method);
            }
        }

        /// <summary>
        /// Restores object from <paramref name="row" />.
        /// </summary>
        /// <param name="row">Data row with object properties.</param>
        /// <returns>Restored object.</returns>
        public object RestoreObject(DataRow row)
        {
            // Create object
            object obj = CreateObject();

            // Fill object's properties
            object value;
            foreach (DataColumn column in row.Table.Columns)
            {
                // DBNull.Value fix
                value = row[column.ColumnName];
                if (value == DBNull.Value)
                {
                    value = null;
                }

                SetFieldValue(obj, column.ColumnName, value);
            }

            // Add object to cache (either overwrite existing with newer version or add new)
            if (enableCaching)
                itemsCache[row[primaryKeyName]] = obj;

            return obj;
        }

        /// <summary>
        /// Restores objects from <paramref name="table" />.
        /// </summary>
        /// <param name="table">Data table with objects.</param>
        /// <returns>Restored object.</returns>
        public ArrayList RestoreObjects(DataTable table)
        {
            // Fill objects list
            ArrayList objectsList = new ArrayList(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                objectsList.Add(RestoreObject(row));
            }

            return objectsList;
        }


        /// <summary>
        /// Internally saves object to DB. Alters or creates new (depends on object primary key).
        /// </summary>
        /// <param name="obj">Object to save.</param>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <returns>True if save operation succeeded.</returns>
        public bool InternalSaveObject(object obj, CommunicationMethod method)
        {
            // Fill command parameters
            SqlCommand itemSaveCommand = GetItemSaveCommandCallback();
            foreach (string fieldName in mapAttributesHash.Keys)
            {
                // string.Empty fix (they will be null)
                object value = GetFieldValue(obj, fieldName);
                if (IsNull(value))
                {
                    value = null;
                }

                // Get parameter for this field and set its value
                SetCommandParameter(itemSaveCommand, fieldName, value);
            }

            // Save object to DB
            DataTable resultTable = null;
            using (itemSaveCommand)
            {
                bool isSucceed;
                resultTable = ExecuteDataTable(itemSaveCommand, ObjectAction.SaveObject, out isSucceed, method);
                if (!isSucceed) return false;
            }

            // Maybe change primary key value for new object
            if (resultTable != null && resultTable.Rows.Count != 0)
            {
                object newPrimaryKeyValue = resultTable.Rows[0][primaryKeyName];
                if (newPrimaryKeyValue != null)
                {
                    SetFieldValue(obj, primaryKeyName, newPrimaryKeyValue);
                }
            }

            return true;
        }

        /// <summary>
        /// Internally deletes object from DB and local objects cache.
        /// </summary>
        /// <param name="obj">Object to delete.</param>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <returns>True if delete operation succeeded.</returns>
        public bool InternalDeleteObject(object obj, CommunicationMethod method)
        {
            // First delete it from DB
            SqlCommand itemDeleteCommand = GetItemDeleteCommandCallback();
            object primaryKeyValue = GetFieldValue(obj, primaryKeyName);
            SetCommandParameter(itemDeleteCommand, primaryKeyName, primaryKeyValue);
            using (itemDeleteCommand)
            {
                bool isSucceed;
                ExecuteDataTable(itemDeleteCommand, ObjectAction.DeleteObject, out isSucceed, method);
                if (!isSucceed) return false;
            }

            // Next remove it from cache
            if (enableCaching)
                itemsCache.Remove(primaryKeyValue);
            return true;
        }

        /// <summary>
        /// Sets parameter on given SqlCommand.
        /// </summary>
        /// <param name="command">Command to set parameter.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterValue">Parameter value.</param>
        static protected void SetCommandParameter(SqlCommand command, string parameterName, object parameterValue)
        {
            parameterName = "@" + parameterName;
            if (command.Parameters.Contains(parameterName))
            {
                // Set value for existing parameter
                command.Parameters[parameterName].Value = parameterValue;
            }
            else
            {
                // Create new parameter
                command.Parameters.AddWithValue(parameterName, parameterValue);
            }
        }

        /// <summary>
        /// Clears parameter on given SqlCommand.
        /// </summary>
        /// <param name="command">Command to clear parameter.</param>
        /// <param name="parameterName">Parameter name.</param>
        static protected void ClearCommandParameter(SqlCommand command, string parameterName)
        {
            parameterName = "@" + parameterName;
            if (command.Parameters.Contains(parameterName))
            {
                command.Parameters.RemoveAt(parameterName);
            }
        }

        #endregion

        #region Get fields/properties info and attributes thru reflection

        /// <summary>
        /// Manager items type.
        /// </summary>
        public Type ItemType
        {
            get { return itemType; }
        }

        /// <summary>
        /// Items primary key field name.
        /// </summary>
        public string PrimaryKeyField
        {
            get { return primaryKeyName; }
        }

        /// <summary>
        /// Items primary key .NET type.
        /// </summary>
        public Type PrimaryKeyType
        {
            get { return primaryKeyType; }
        }


        /// <summary>
        /// Fills map attributes hash if it's empty.
        /// </summary>
        private void InitItemMetadata()
        {
            if (mapAttributesHash == null)
            {
                // Get default constructor
                itemConstructorInfo = itemType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);

                // Get properties and fields reflection data for itemType
                PropertyInfo[] propertyInfos = itemType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                FieldInfo[] fieldInfos = itemType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                MemberInfo[] memberInfos = new MemberInfo[propertyInfos.Length + fieldInfos.Length];
                propertyInfos.CopyTo(memberInfos, 0);
                fieldInfos.CopyTo(memberInfos, propertyInfos.Length);

                mapAttributesHash = new Hashtable(propertyInfos.Length + fieldInfos.Length);
                memberHash = new Hashtable(propertyInfos.Length + fieldInfos.Length);
                nameToFieldHash = new Hashtable(propertyInfos.Length + fieldInfos.Length);
                referenceTypeHash = new Hashtable(propertyInfos.Length + fieldInfos.Length);
                referenceNameHash = new Hashtable(propertyInfos.Length + fieldInfos.Length);
                mapReferenceMemberInfos = new Hashtable();

                // Fill map attributes hash according to properties/fields info
                foreach (MemberInfo memberInfo in memberInfos)
                {
                    // Add to members hash
                    memberHash.Add(memberInfo.Name, memberInfo);

                    // Get MapFieldAttribute from member
                    MapFieldAttribute mapField = (MapFieldAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(MapFieldAttribute), true);
                    if (mapField != null)
                    {
                        // Get valid field name
                        string fieldName = mapField.FieldName.Length != 0 ? mapField.FieldName : memberInfo.Name;

                        // Add this to cache
                        mapAttributesHash[fieldName] = memberInfo;
                        nameToFieldHash[memberInfo.Name] = fieldName;

                        // Maybe set this field as primary key
                        if (mapField.IsPrimaryKey)
                        {
                            primaryKeyName = fieldName;
                            primaryKeyType = GetMemberType(memberInfo);
                        }
                    }

                    // Get MapReferenceAttribute from member
                    MapReferenceAttribute mapReference = (MapReferenceAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(MapReferenceAttribute), true);
                    if (mapReference != null)
                    {
                        // Add mapping by referenced type to cache
                        string referencePropertyName = mapReference.ReferencePropertyName.Length != 0 ?
                          mapReference.ReferencePropertyName :
                          mapReference.ReferencedType.Name;

                        referenceTypeHash[mapReference.ReferencedType] = memberInfo;
                        referenceNameHash[referencePropertyName] = mapReference.ReferencedType;
                        mapReferenceMemberInfos[referencePropertyName] = memberInfo;
                    }
                }

                // Get error messages from attribute
                errorMessages = GetTypeErrorMessages(itemType);
            }
        }

        /// <summary>
        /// Returns object of type <paramref name="referencedName" /> referenced to given <paramref name="obj" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="referencedName">Referenced object type name.</param>
        /// <returns>Referenced object value.</returns>
        public object GetReferencedObject(object obj, string referencedName)
        {
            return GetReferencedObject(obj, (Type)referenceNameHash[referencedName]);
        }

        /// <summary>
        /// Returns object of type <paramref name="referencedType" /> referenced to given <paramref name="obj" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="referencedType">Referenced object type.</param>
        /// <returns>Referenced object value.</returns>
        public object GetReferencedObject(object obj, Type referencedType)
        {
            return GetReferencedObject(obj, ManagerCache.GetManager(referencedType));
        }

        /// <summary>
        /// Returns object by using <paramref name="manager" /> referenced to given <paramref name="obj" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="manager">Referenced object manager.</param>
        /// <returns>Referenced object value.</returns>
        public object GetReferencedObject(object obj, IObjectManager manager)
        {
            return GetReferencedObject(obj, manager, (MemberInfo)referenceTypeHash[manager.ItemType]);
        }

        public object GetReferencedObject(object obj, IObjectManager manager, string referencePropertyName)
        {
            if (mapReferenceMemberInfos.ContainsKey(referencePropertyName))
            {
                return GetReferencedObject(obj, manager, (MemberInfo)mapReferenceMemberInfos[referencePropertyName]);
            }
            else
            {
                return GetReferencedObject(obj, manager);
            }
        }

        public object GetReferencedObject(object obj, IObjectManager manager, MemberInfo memberInfo)
        {
            return manager.GetObject(GetMemberValue(obj, memberInfo));
        }

        /// <summary>
        /// Returns property/field value of given <paramref name="obj" /> by given <paramref name="memberName" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="memberName">Member name (supports dots).</param>
        /// <returns>Object member value.</returns>
        public object GetMemberValue(object obj, string memberName)
        {
            IObjectManager manager = GetNextManager(ref obj, ref memberName);
            if (manager == this)
            {
                return GetMemberValue(obj, GetMemberInfo(memberName));
            }
            else
            {
                return manager.GetMemberValue(obj, memberName);
            }
        }

        /// <summary>
        /// Returns property/field value of given <paramref name="obj" /> by given <paramref name="memberInfo" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="memberInfo">Member info.</param>
        /// <returns>Object member value.</returns>
        public object GetMemberValue(object obj, MemberInfo memberInfo)
        {
            return obj != null && memberInfo != null ?
              memberInfo is PropertyInfo ?
                ((PropertyInfo)memberInfo).GetValue(obj, null) :
                ((FieldInfo)memberInfo).GetValue(obj) :
              null;
        }

        /// <summary>
        /// Sets property/field value of given <paramref name="obj" /> by given <paramref name="memberInfo" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="memberInfo">Member info.</param>
        /// <param name="newValue">New member value.</param>
        public void SetMemberValue(object obj, MemberInfo memberInfo, object newValue)
        {
            if (obj == null || memberInfo == null)
                return;

            if (newValue == null)
            {
                // Get "correct" null value
                newValue = GetCorrectNull(GetMemberType(memberInfo));
            }

            if (memberInfo is PropertyInfo)
            {
                ((PropertyInfo)memberInfo).SetValue(obj, newValue, null);
            }
            else
            {
                ((FieldInfo)memberInfo).SetValue(obj, newValue);
            }
        }

        /// <summary>
		/// Returns property/field value of given <paramref name="obj" /> by given DB <paramref name="fieldName" />.
		/// </summary>
		/// <param name="obj">Object to use.</param>
		/// <param name="fieldName">DB field name.</param>
		/// <returns>Object member value.</returns>
		public object GetFieldValue(object obj, string fieldName)
        {
            return GetMemberValue(obj, GetMemberInfoByField(fieldName));
        }

        /// <summary>
        /// Sets property/field value of given <paramref name="obj" /> by given DB <paramref name="fieldName" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="fieldName">DB field name.</param>
        /// <param name="newValue">New member value.</param>
        public void SetFieldValue(object obj, string fieldName, object newValue)
        {
            SetMemberValue(obj, GetMemberInfoByField(fieldName), newValue);
        }

        /// <summary>
        /// Sets property/field value of given <paramref name="obj" /> by given <paramref name="memberName" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="memberName">Member name (supports dots).</param>
        /// <param name="newValue">New member value.</param>
        public void SetMemberValue(object obj, string memberName, object newValue)
        {
            IObjectManager manager = GetNextManager(ref obj, ref memberName);
            if (manager == this)
            {
                SetMemberValue(obj, GetMemberInfo(memberName), newValue);
            }
            else
            {
                manager.SetMemberValue(obj, memberName, newValue);
            }
        }

        /// <summary>
        /// Sets properties/fields values of given <paramref name="obj" />.
        /// </summary>
        /// <param name="obj">Object to use.</param>
        /// <param name="memberData">Array of data formed like name-value-name-value-...</param>
        public void SetMemberValues(object obj, params object[] memberData)
        {
            for (int i = 0; i < memberData.Length - 1; i += 2)
            {
                string memberName = memberData[i] as string;
                if (memberName != null)
                {
                    SetMemberValue(obj, memberName, memberData[i + 1]);
                }
            }
        }

        /// <summary>
        /// Returns type of given member.
        /// </summary>
        /// <param name="memberInfo">Member info.</param>
        /// <returns>Member type.</returns>
        public Type GetMemberType(MemberInfo memberInfo)
        {
            return memberInfo != null ?
              memberInfo is PropertyInfo ?
                ((PropertyInfo)memberInfo).PropertyType :
                ((FieldInfo)memberInfo).FieldType :
              null;
        }

        /// <summary>
        /// Returns type of given member.
        /// </summary>
        /// <param name="memberName">Member name.</param>
        /// <returns>Member type.</returns>
        public Type GetMemberType(string memberName)
        {
            return GetMemberType(GetMemberInfo(memberName));
        }

        /// <summary>
        /// Returns member info.
        /// </summary>
        /// <param name="memberName">Member name.</param>
        /// <returns>Member info.</returns>
        public MemberInfo GetMemberInfo(string memberName)
        {
            return (MemberInfo)memberHash[memberName];
        }

        /// <summary>
        /// Returns member info.
        /// </summary>
        /// <param name="fieldName">Field name.</param>
        /// <returns>Member info.</returns>
        public MemberInfo GetMemberInfoByField(string fieldName)
        {
            return (MemberInfo)mapAttributesHash[fieldName];
        }

        public string[] ErrorMessages
        {
            get
            {
                return errorMessages;
            }
        }


        /// <summary>
		/// Gets next manager if memberName is dot-delimited.
		/// </summary>
		/// <param name="obj">Object to use.</param>
		/// <param name="memberName">Member name.</param>
		/// <returns>Object manager to use.</returns>
		private IObjectManager GetNextManager(ref object obj, ref string memberName)
        {
            int dotIndex = memberName.IndexOf('.');
            if (dotIndex < 0) return this;

            string memberSubname = memberName.Substring(0, dotIndex);
            MemberInfo memberInfo = GetMemberInfo(memberSubname);

            Type nextType = memberInfo != null ? GetMemberType(memberInfo) : (Type)referenceNameHash[memberSubname];

            if (nextType != null)
            {
                IObjectManager manager = ManagerCache.GetManager(nextType);
                obj = memberInfo != null ?
                  GetMemberValue(obj, memberInfo) :
                  GetReferencedObject(obj, manager, memberSubname);
                memberName = memberName.Substring(dotIndex + 1);
                return manager;
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Returns error messages defined with <see cref="ErrorMessageAttribute" /> for given <paramref name="type" />.
        /// </summary>
        /// <param name="type">Type with error messages attached.</param>
        /// <returns>String array with error messages for this type.</returns>
        public static string[] GetTypeErrorMessages(Type type)
        {
            string[] errorMessages = new string[Enum.GetValues(typeof(ObjectAction)).Length];

            // System.Object can't have ErrorMessageAttribute anyway
            if (type == typeof(object)) return errorMessages;

            // Get error messages from attribute
            Attribute[] errorMessageAttributes = Attribute.GetCustomAttributes(type, typeof(ErrorMessageAttribute));
            int seriesSum = 0;
            foreach (ErrorMessageAttribute attribute in errorMessageAttributes)
            {
                int index = (int)attribute.Action;
                if (errorMessages[index] == null)
                {
                    errorMessages[index] = attribute.ErrorMessage;
                    seriesSum += index + 1;
                }
            }

            // Maybe add missing messages from parent class. Check series sum
            if (seriesSum < (errorMessages.Length + 1) * errorMessages.Length / 2)
            {
                string[] baseErrorMessages = GetTypeErrorMessages(type.BaseType);
                for (int i = 0; i < errorMessages.Length; ++i)
                {
                    if (errorMessages[i] == null)
                    {
                        errorMessages[i] = baseErrorMessages[i] != null ? baseErrorMessages[i] : string.Empty;
                    }
                }
            }

            return errorMessages;
        }

        #endregion

        #region Working with non-nullable objects

        /// <summary>
        /// Returns true if <paramref name="obj" /> is null.
        /// </summary>
        /// <param name="obj">Not-nullable object.</param>
        /// <returns>True or false.</returns>
        static public bool IsNull(object obj)
        {
            return
              obj == null ||
              obj is string && ((string)obj).Length == 0 ||
              obj == DBNull.Value ||
              obj is int && (int)obj == Int32.MinValue ||
              obj is DateTime && (DateTime)obj == DateTime.MinValue ||
              obj is Guid && (Guid)obj == Guid.Empty;
        }

        /// <summary>
        /// Returns "correct" value of null for certain struct types.
        /// </summary>
        /// <param name="objType">Struct type.</param>
        /// <returns>Correct null value for this type of items.</returns>
        static public object GetCorrectNull(Type objType)
        {
            if (objType == typeof(int))
            {
                return int.MinValue;
            }
            else if (objType == typeof(DateTime))
            {
                return DateTime.MinValue;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the NULL value for integers.
        /// </summary>
        /// <returns>Return value for integers.</returns>
        public static int GetNullInt()
        {
            return (int)GetCorrectNull(typeof(int));
        }

        #endregion

        #region Exceptions handling

        /// <summary>
        /// Executes <see cref="ExecuteDataTableCallback" />.
        /// Fires <see cref="ManagerCache.ObjectException" /> event on throwed exception.
        /// </summary>
        /// <param name="command"><see cref="SqlCommand" /> to execute.</param>
        /// <param name="action">Action performed on business object.</param>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <returns>Query results as <see cref="DataTable" />.</returns>
        protected DataTable ExecuteDataTable(SqlCommand command, ObjectAction action, CommunicationMethod method)
        {
            bool isSucceed;
            return ExecuteDataTable(command, itemType, action, out isSucceed, method);
        }

        /// <summary>
        /// Executes <see cref="ExecuteDataTableCallback" />.
        /// Fires <see cref="ManagerCache.ObjectException" /> event on throwed exception.
        /// </summary>
        /// <param name="command"><see cref="SqlCommand" /> to execute.</param>
        /// <param name="action">Action performed on business object.</param>
        /// <param name="isSucceed">Execute succeeded.</param>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <returns>Query results as <see cref="DataTable" />.</returns>
        protected DataTable ExecuteDataTable(SqlCommand command, ObjectAction action, out bool isSucceed, CommunicationMethod method)
        {
            return ExecuteDataTable(command, itemType, action, out isSucceed, method);
        }

        /// <summary>
        /// Executes <see cref="ExecuteDataTableCallback" />.
        /// Fires <see cref="ManagerCache.ObjectException" /> event on throwed exception.
        /// </summary>
        /// <param name="command"><see cref="SqlCommand" /> to execute.</param>
        /// <param name="itemType">Type of business object which caused an exception.</param>
        /// <param name="action">Action performed on business object.</param>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <returns>Query results as <see cref="DataTable" />.</returns>
        protected DataTable ExecuteDataTable(SqlCommand command, Type itemType, ObjectAction action, CommunicationMethod method)
        {
            bool isSucceed;
            return ExecuteDataTable(command, itemType, action, out isSucceed, method);
        }

        /// <summary>
        /// Executes <see cref="ExecuteDataTableCallback" />.
        /// Fires <see cref="ManagerCache.ObjectException" /> event on throwed exception.
        /// </summary>
        /// <param name="command"><see cref="SqlCommand" /> to execute.</param>
        /// <param name="itemType">Type of business object which caused an exception.</param>
        /// <param name="action">Action performed on business object.</param>
        /// <param name="isSucceed">Execute succeeded.</param>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <returns>Query results as <see cref="DataTable" />.</returns>
        protected DataTable ExecuteDataTable(SqlCommand command, Type itemType, ObjectAction action, out bool isSucceed, CommunicationMethod method)
        {
            try
            {
                isSucceed = true;
                return ExecuteDataTableCallback(command, method);
            }
            catch (Exception ex)
            {
                isSucceed = false;
                throw ex;
            }
        }

        /// <summary>
        /// Handles business object exception.
        /// </summary>
        /// <param name="itemType">Type of business object which caused an exception.</param>
        /// <param name="action">Action performed on business object.</param>
        /// <param name="ex">Exception throwed.</param>
        private void HandleObjectException(Type itemType, ObjectAction action, Exception ex)
        {
            lastException = ex;

            IObjectManager itemManager = itemType == this.itemType ? this : ManagerCache.GetManager(itemType);
            string message;
            if (ex is BusinessObjectException || ex is SqlException)
            {
                message = ex.Message;
                //				if (ex.InnerException != null) {
                //					ex = ex.InnerException;
                //				}
            }
            else
            {
                message = itemManager.ErrorMessages[(int)action];
            }
            ManagerCache.HandleObjectException(itemType, action, message, ex);
        }

        #endregion
    }
}
