using System;

namespace BillingProvidersApp.Core
{
    #region enum ObjectAction

    /// <summary>
    /// Action performed on business object.
    /// </summary>
    public enum ObjectAction
    {
        /// <summary>
        /// Unknown action.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Load object.
        /// </summary>
        LoadObject = 1,
        /// <summary>
        /// Load objects.
        /// </summary>
        LoadObjects = 2,
        /// <summary>
        /// Save object.
        /// </summary>
        SaveObject = 3,
        /// <summary>
        /// Delete object.
        /// </summary>
        DeleteObject = 4
    }

    #endregion enum ObjectAction
    public class ExceptionInfo
    {
        /// <summary>
        /// Exception actually throwed.
        /// </summary>
        public Exception Exception { get; protected set; }

        public ExceptionInfo(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Exception = exception;
        }

        /// <summary>
        /// Returns exception message
        /// </summary>
        public string Message { get { return Exception.Message; } }

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
}
