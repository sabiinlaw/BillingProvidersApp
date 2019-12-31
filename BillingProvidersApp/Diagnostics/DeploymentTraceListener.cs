using System;

namespace BillingProvidersApp.Diagnostics
{
    public abstract class DeploymentTraceListener
    {
        public abstract void Write(string message);

        public virtual void Write(string message, MessageCategory category)
        {
            Write(FormatMessage(message, category));
        }

        public abstract void WriteLine(string message);

        public virtual void WriteLine(string message, MessageCategory category)
        {
            WriteLine(FormatMessage(message, category));
        }

        protected string FormatMessage(string message, MessageCategory category)
        {
            return String.Format("{0}: {1}", category, message);
        }
    }
}
