using SuiteTalk;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BillingProvidersApp.Managers
{
    public abstract class NetSuiteEntityManager
    {
        #region Private

        private RecordType _recordType;
        private Random _random = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Type constructor.
        /// </summary>
        /// <param name="recordType">NetSuite entity type</param>
        public NetSuiteEntityManager(RecordType recordType, Action<string> logMessage = null)
        {
            _recordType = recordType;
            LogMessage = logMessage;
        }


        /// <summary>
		/// Adds object to the NetSuite.
		/// </summary>
		/// <param name="entity">Object to add</param>		
		public virtual NetSuiteReadWriteResult Add(Record entity)
        {
            Task<WriteResponse> resultTask = null;
            NetSuiteHelper.TryRun(() =>
            {
                using (var proxy = NetSuiteHelper.GetProxy())
                {
                    resultTask = proxy.addAsync(entity);
                    var result = resultTask.Result;
                    return result.status.isSuccessSpecified && result.status.isSuccess;
                }
            },
            (message) =>
            {
                LogMessage?.Invoke(message);
            });

            return new NetSuiteReadWriteResult(result.status, RecordType,
                NetSuiteOperationType.Add, entity, result.writeResponse[0].baseRef);
        }

        /// <summary>
        /// Invokes NetSuite's asyncAddList(..) method.
        /// </summary>
        /// <param name="records">The records to be added.</param>
        /// <returns>The filter criteria for the retrieved data.</returns>
        public virtual WriteResponseList AsyncAddList(Record[] recordsList)
        {
            WriteResponseList result = null;
            Task<WriteResponseList> resultTask = null;
            NetSuiteHelper.TryRun(() =>
            {
                using (var proxy = NetSuiteHelper.GetProxy())
                {
                    resultTask = proxy.addListAsync(recordsList);
                    result = resultTask.Result;
                    return result.status.isSuccessSpecified && result.status.isSuccess;
                }
            },
            (message) =>
            {
                LogMessage?.Invoke(message);
            });

            return result;
        }

        /// <summary>
        /// Gets the object from the NetSuite by the given internal ID
        /// </summary>
        /// <param name="internalId">Object internal ID in NetSuite</param>
        /// <returns>The instance of the <see cref="NetSuiteReadWriteResult"/> object</returns>
        public NetSuiteReadWriteResult Get(string internalId)
        {
            RecordRef recordRef = new RecordRef();
            recordRef.internalId = internalId;
            recordRef.type = RecordType;
            recordRef.typeSpecified = true;

            return Get(recordRef);
        }

        public IEnumerable<NetSuiteReadWriteResult> Get(List<string> internalIds)
        {
            List<RecordRef> recordRefs = new List<RecordRef>();
            foreach (var item in internalIds)
            {
                RecordRef recordRef = new RecordRef();
                recordRef.internalId = item;
                recordRef.type = RecordType;
                recordRef.typeSpecified = true;
            }

            return Get(recordRefs.ToArray());
        }

        public IEnumerable<NetSuiteReadWriteResult> Get(RecordRef[] recordRefs)
        {
            foreach (var recordRef in recordRefs)
            {
                recordRef.type = RecordType;
                recordRef.typeSpecified = true;
            }
            Task<ReadResponseList> getAsyncResults = null;


            NetSuiteHelper.TryRun(() =>
            {
                using (var proxy = NetSuiteHelper.GetProxy())
                {
                    getAsyncResults = proxy.getListAsync(recordRefs);
                    var result = getAsyncResults.Result;
                    return result.status.isSuccessSpecified && result.status.isSuccess;
                }
            },
            (message) =>
            {
                LogMessage?.Invoke(message);
            });

            //TODO record
            yield return new NetSuiteReadWriteResult(result.status, RecordType,
                NetSuiteOperationType.Get, null, null);
        }

        public NetSuiteReadWriteResult Get(RecordRef recordRef)
        {
            Task<ReadResponse> getAsyncResult = null;
            recordRef.type = RecordType;
            recordRef.typeSpecified = true;

            NetSuiteHelper.TryRun(() =>
            {
                using (var proxy = NetSuiteHelper.GetProxy())
                {
                    getAsyncResult = proxy.getAsync(recordRef);
                    var result = getAsyncResult.Result;
                    return result.status.isSuccessSpecified && result.status.isSuccess;
                }
            },
            (message) =>
            {
                LogMessage?.Invoke(message);
            });

            //TODO record
            return new NetSuiteReadWriteResult(result.status, RecordType,
                NetSuiteOperationType.Get, null, null);
        }

        /// <summary>
        /// Gets a list of records for one or more NetSuite WEB service call
        /// </summary>
        /// <param name="recordsList">Array of <see cref="RecordRef"/> objects</param>
        /// <returns>Array of the <see cref="NetSuiteReadWriteResult"/> objects</returns>
        public virtual NetSuiteReadWriteResult[] GetList(RecordRef[] recordsList)
        {
            if (recordsList == null)
                return null;

            if (recordsList.Length == 0)
                return new NetSuiteReadWriteResult[0];

            NetSuiteReadWriteResult[] resultArray = new NetSuiteReadWriteResult[recordsList.Length];

            var maxGetListRecordsCount = NetSuiteHelper.MaxGetListRecordsCount;

            for (int index = 0; index * maxGetListRecordsCount < recordsList.Length; index++)
            {
                int startIndex = index * maxGetListRecordsCount;
                int elementsCount = (startIndex + maxGetListRecordsCount) > recordsList.Length ?
                    (recordsList.Length - startIndex) : maxGetListRecordsCount;

                RecordRef[] getList = new RecordRef[elementsCount];
                Array.Copy(recordsList, startIndex, getList, 0, elementsCount);
                Task<ReadResponseList> responseArrayTask = NetSuiteHelper.GetProxy().getListAsync(getList);

                for (int jIndex = 0; jIndex < responseArrayTask.Result.readResponse.Length; jIndex++)
                    resultArray[startIndex + jIndex] = new NetSuiteReadWriteResult(
                        responseArrayTask.Result.readResponse[jIndex].status, getList[jIndex].type, NetSuiteOperationType.Get,
                        responseArrayTask.Result.readResponse[jIndex].record, null);
            }

            return resultArray;
        }

        #endregion

        #region Properties

        public Action<string> LogMessage { get; set; }

        /// <summary>
        /// Gets the NetSuite entity type.
        /// </summary>
        public RecordType RecordType
        {
            get { return _recordType; }
        }

        #endregion

        WriteResponseList result = null;
        public bool requestFinished;
        public delegate void AddListCompletedEventhandler(object sender, addListCompletedEventArgs e);
        object userState = new object();


        private void NetSuiteEntityManager_addListCompleted(object sender, addListCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                result = e.Result;
            }
            requestFinished = true;
        }

    }
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class addListCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal addListCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public WriteResponseList Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((WriteResponseList)(this.results[0]));
            }
        }
    }

}
