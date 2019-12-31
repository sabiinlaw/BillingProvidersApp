using BillingProvidersApp.Helper.DependencyResolver.DependencyResolverFactory;
using System;

namespace BillingProvidersApp.Helper.DependencyResolver
{
    /// <summary>
    /// Provide DI relation
    /// </summary>
    public static class DependencyResolver
    {
        private static volatile IDependencyResolver _resolver = null;
        private static volatile Boolean _isInited = false;


        /// <summary>
        /// Get dependecy 
        /// </summary>
        /// <typeparam name="TAbstaction">Abstraction type</typeparam>
        /// <returns>Binded implementation for abstaction</returns>
        /// <exception cref="InvalidOperationException">DependencyResolver if not configured</exception>
        public static TAbstaction Get<TAbstaction>() where TAbstaction : class
        {
            if (_isInited == false)
            {
                throw new InvalidOperationException("DependencyResolver if not configured");
            }

            return _resolver.Get<TAbstaction>();
        }
    }
}