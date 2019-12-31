namespace BillingProvidersApp.Diagnostics
{
    public class Trace
    {
        public const MessageCategory DefaultCategory = MessageCategory.Information;

        DeploymentTraceListenersCollection listeners;
        public DeploymentTraceListenersCollection Listeners
        {
            get { return listeners; }
        }
        public Trace()
        {
            this.listeners = new DeploymentTraceListenersCollection();
        }
        public void Write(string message)
        {
            foreach (DeploymentTraceListener listener in Listeners)
            {
                listener.Write(message);
            }
        }
        public void Write(string message, MessageCategory category)
        {
            foreach (DeploymentTraceListener listener in Listeners)
            {
                listener.Write(message, category);
            }
        }
        public void WriteLine(string message)
        {
            foreach (DeploymentTraceListener listener in Listeners)
            {
                listener.WriteLine(message);
            }
        }
        public void WriteLine(string message, MessageCategory category)
        {
            foreach (DeploymentTraceListener listener in Listeners)
            {
                listener.WriteLine(message, category);
            }
        }
    }
}
