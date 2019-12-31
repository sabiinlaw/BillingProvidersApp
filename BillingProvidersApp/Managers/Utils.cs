using SuiteTalk;
using System;
using System.Text;

namespace BillingProvidersApp.Managers
{
    public static class Utils
    {
        #region constants

        const string StatusMessageTemplate = "NetSuite status code: {0}. Status detail type: {1}. Status message: '{2}'.\n";

        #endregion

        /// <summary>
        /// Combines array of the StatusDetail messages into a single string
        /// </summary>
        /// <param name="details">Array of StatusDetail objects</param>
        public static string ParseStatusDetails(StatusDetail[] details)
        {
            if (null == details)
                return null;

            StringBuilder errorMessage = new StringBuilder();

            if (details.Length > 0)
            {
                foreach (StatusDetail statusDetail in details)
                    errorMessage.AppendFormat(StatusMessageTemplate, statusDetail.code,
                        statusDetail.type, statusDetail.message);
            }

            return errorMessage.ToString();
        }

        /// <summary>
        /// Parses NetSuite response status and returns the detailed string message
        /// </summary>
        /// <param name="operationType">NetSuite operation type</param>
        /// <param name="status">NetSuite response status</param>
        /// <param name="recordType">NetSuite entity type</param>
        /// <returns></returns>
        public static string ParseStatus(NetSuiteOperationType operationType,
            Status status,
            RecordType recordType)
        {
            if (null == status)
                return null;

            string details = ParseStatusDetails(status.statusDetail);

            string result = !status.isSuccess ? GetFailedOperationMessage(operationType, recordType) :
                GetSuccessOperationMessage(operationType, recordType);

            if (!string.IsNullOrEmpty(details))
                result = string.Format("{0} Details: {1}", result, details);

            return result;
        }

        /// <summary>
        /// Returns string message for successful operation with NetSuite.
        /// </summary>
        /// <param name="operationType">NetSuite operation type</param>
        /// <param name="recordType">NetSuite entity type</param>		
        public static string GetSuccessOperationMessage(
            NetSuiteOperationType operationType,
            RecordType recordType)
        {
            return string.Format("{0} {1}(s) was successful.", operationType, recordType);
        }

        /// <summary>
        /// Returns string message for failed operation with NetSuite.
        /// </summary>
        /// <param name="operationType">NetSuite operation type</param>
        /// <param name="recordType">NetSuite entity type</param>
        public static string GetFailedOperationMessage(
            NetSuiteOperationType operationType,
            RecordType recordType)
        {
            return string.Format("{0} {1}(s) was done with errors.", operationType, recordType);
        }

        public static bool IsBrokenNetSuiteCustomerReference(this Status status, string customerReferenceField)
        {
            if (null == status)
                throw new ArgumentNullException("status");

            if (string.IsNullOrEmpty(customerReferenceField))
                throw new ArgumentNullException("customerReferenceField");

            bool brokenCustomerReference = false;

            if (null != status.statusDetail)
            {
                foreach (StatusDetail statusDetail in status.statusDetail)
                {
                    if ((StatusDetailType.ERROR == statusDetail.type) &&
                        (StatusDetailCodeType.INVALID_KEY_OR_REF == statusDetail.code) &&
                        -1 !=
                        statusDetail.message.IndexOf(customerReferenceField, StringComparison.CurrentCultureIgnoreCase))
                    {
                        brokenCustomerReference = true;

                        break;
                    }
                }
            }

            return brokenCustomerReference;
        }
    }
}
