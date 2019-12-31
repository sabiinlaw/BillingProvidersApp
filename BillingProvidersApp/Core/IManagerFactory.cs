using System;

namespace BillingProvidersApp.Core
{
    /// <summary>
    /// Interface of object managers factory.
    /// </summary>
    public interface IManagerFactory
    {
        /// <summary>
        /// Returns new NocObjectManager for specified type of items.
        /// </summary>
        /// <param name="itemType">Type of items processed by manager.</param>
        /// <returns>Generated manager or null if can't create manager.</returns>
        IObjectManager GetManager(Type itemType);
    }
}
