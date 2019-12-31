using System;

namespace BillingProvidersApp.Core
{

    /// <summary>
	/// Provides information about exception to global exception handlers.
	/// </summary>
	sealed public class ObjectExceptionInfo : ExceptionInfo
    {
        readonly Type itemType;
        readonly ObjectAction action;
        readonly string errorMessage;


        /// <summary>
        /// Creates new object exception info.
        /// </summary>
        /// <param name="itemType">Type of business object which caused an exception.</param>
        /// <param name="action">Action performed on business object.</param>
        /// <param name="errorMessage">Error message to display (may vary between different objects).</param>
        /// <param name="exception">Exception actually throwed.</param>
        /// <exception cref="ArgumentNullException"><paremref name="itemType" />, <paremref name="errorMessage" /> or <paremref name="innerException" /> is a null reference.</exception>
        internal ObjectExceptionInfo(Type itemType, ObjectAction action, string errorMessage, Exception exception) : base(exception)
        {
            if (errorMessage == null)
            {
                throw new ArgumentNullException("errorMessage");
            }

            this.itemType = itemType;
            this.action = action;
            this.errorMessage = errorMessage;
        }

        /// <summary>
        /// Type of business object which caused an exception.
        /// </summary>
        public Type ItemType
        {
            get { return itemType; }
        }

        /// <summary>
        /// Action performed on business object.
        /// </summary>
        public ObjectAction Action
        {
            get { return action; }
        }

        /// <summary>
        /// Error message to display (may vary between different objects).
        /// </summary>
        public string ErrorMessage
        {
            get { return errorMessage; }
        }
    }
}
