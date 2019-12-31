using System;

namespace BillingProvidersApp.Core
{
    /// <summary>
    /// <see cref="BusinessObject" /> specific exception.
    /// It must be treated specially in handlers.
    /// </summary>
    public class BusinessObjectException : Exception
    {
    }
}