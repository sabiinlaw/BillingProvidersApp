using SuiteTalk;

namespace BillingProvidersApp.Managers
{
    public class NetSuiteReadWriteResult : NetSuiteOperationResult
    {
        #region private

        private Record _record;
        private BaseRef _baseRef;

        #endregion

        #region Constructor

        internal NetSuiteReadWriteResult(Status status, RecordType recordType, NetSuiteOperationType operationType, Record record, BaseRef baseRef)
            : base(status, recordType, operationType)
        {
            _record = record;
            _baseRef = baseRef;
        }

        #endregion

        #region Public

        /// <summary>
        /// Gets the target record.
        /// </summary>
        public Record Record
        {
            get
            {
                return _record;
            }
        }

        /// <summary>
        /// Gets the base reference.
        /// </summary>
        public BaseRef BaseRef
        {
            get
            {
                return _baseRef;
            }
        }

        #endregion
    }
}
