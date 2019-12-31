using SuiteTalk;

namespace BillingProvidersApp.Managers
{
    public enum NetSuiteOperationType
    {
        /// <summary>
        /// Add entity to NetSuite
        /// </summary>
        Add,

        /// <summary>
        /// Attach	
        /// </summary>
        Attach,

        /// <summary>
        /// Get entity from NetSuite
        /// </summary>
        Get,

        /// <summary>
        /// Update
        /// </summary>
        Update,

        /// <summary>
        /// Delete
        /// </summary>
        Delete,

        /// <summary>
        /// Search
        /// </summary>
        Search
    }
    public class NetSuiteOperationResult
    {
        #region private

        private NetSuiteOperationType _operationType;
        private RecordType _recordType;
        private Status _status;

        #endregion

        #region Constructor

        internal NetSuiteOperationResult(Status status, RecordType recordType, NetSuiteOperationType operationType)
        {
            _status = status;
            _recordType = recordType;
            _operationType = operationType;
        }

        #endregion

        #region Public

        /// <summary>
        /// Gets the log of the NetSuite operation.
        /// </summary>
        public string OperationLog
        {
            get
            {
                return Utils.ParseStatus(OperationType, Status, RecordType);
            }
        }

        public NetSuiteOperationType OperationType
        {
            get
            {
                return _operationType;
            }
        }

        public RecordType RecordType
        {
            get
            {
                return _recordType;
            }
        }

        /// <summary>
        /// Indicates whether the NetSuite operation is successful.
        /// </summary>
        public virtual bool IsSuccess
        {
            get
            {
                return _status.isSuccessSpecified ? _status.isSuccess : true;
            }
        }

        public Status Status
        {
            get
            {
                return _status;
            }
        }

        public StatusDetail[] StatusDetail
        {
            get
            {
                return _status.statusDetail;
            }
        }

        #endregion
    }
}
