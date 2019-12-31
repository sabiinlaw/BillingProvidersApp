namespace BillingProvidersApp.Core
{
    public enum CommunicationMethod
    {
        /// <summary>
        /// Using configuration properties
        /// </summary>
        Default,

        /// <summary>
        /// Loading of the data using MS DTC
        /// </summary>
        UseMSDTC,

        /// <summary>
        /// Loading of the data without using MS DTC
        /// </summary>
        NotUseMSDTC
    }
}
