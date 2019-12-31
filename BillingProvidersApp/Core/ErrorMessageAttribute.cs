using System;

namespace BillingProvidersApp.Core
{
    /// <summary>
    /// Error message which will be displayed in case of exception on business object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class ErrorMessageAttribute: Attribute
    {
        ObjectAction action;
        string errorMessage;


        /// <summary>
        /// Adds new <see cref="ErrorMessageAttribute" /> to the business object.
        /// </summary>
        /// <param name="action">Object action which causes exception.</param>
        /// <param name="errorMessage">Error message to display on exception.</param>
        /// <exception cref="ArgumentNullException"><paramref name="errorMessage" /> is a null reference.</exception>
        public ErrorMessageAttribute(ObjectAction action, string errorMessage)
        {
            // Exceptions
            if (errorMessage == null)
            {
                throw new ArgumentNullException("errorMessage");
            }

            this.action = action;
            this.errorMessage = errorMessage;
        }


        /// <summary>
        /// Object action which causes exception.
        /// </summary>
        public ObjectAction Action
        {
            get { return action; }
        }

        /// <summary>
        /// Error message to display on exception.
        /// </summary>
        public string ErrorMessage
        {
            get { return errorMessage; }
        }
    }
}
