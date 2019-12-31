using BillingProvidersApp.BO.Tools;
using BillingProvidersApp.Core;
using BillingProvidersApp.DAL.BusinessObjects;
using BillingProvidersApp.Diagnostics;
using BillingProvidersApp.Helper;
using BillingProvidersApp.Managers;
using SuiteTalk;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Trace = BillingProvidersApp.Diagnostics.Trace;

namespace BillingProvidersApp
{
    class Program
    {
        //TODO implement provider events (OnInvoiced etc.)
        //TODO move to appropriate place
        private const string GetBillableProvidersForSpecificiProvidersCommandText =
            @"SELECT m.ID, m.DisplayAs, m.InstanceID, ISNULL(a.nsCustomerID, -1) as nsCustomerID, m.BillForSmartZones, m.BillingCountry, m.PreferableLanguageCultureID,
								cast ((case when (select count(1) from ypLnkPrincipalMember where PrincipalMemberID = m.ID) > 0 then 'true' else 'false' end) as bit) as IsPrincipalMember,
								(select top 1 PrincipalMemberID from ypLnkPrincipalMember where MemberID = m.ID) as PrincipalMemberID
			FROM ypMember m
	            INNER JOIN nocAccount a on a.ID = m.AccountID
                INNER JOIN ypNOC n ON m.NocId = n.ID
            WHERE m.TypeID = 1 and m.Billable = 1 and a.nsCustomerID > 0 and m.BillingCountry is not null and m.BillingCountry <> ''
			and n.WebServiceUrl = @WebServiceUrl 
			and m.Created <= @EndDate
			ORDER BY DisplayAs";

        private const string SQLRequestorAlives =
            @"select t.MemberID, Sum(Alive) as Alives, Sum(notAlive) as NotAlives FROM
            (
                select l.MemberID, 
	                case when DATEDIFF(day, mreq.BillableFrom, GETUTCDATE()) < 0 then 1 else 0 end as notAlive,
	                case when DATEDIFF(day, mreq.BillableFrom, GETUTCDATE()) < 0 then 0 else 1 end as Alive
                FROM ypLocation l
	                INNER join ypLnkMember lnk on lnk.ProviderLocationID = l.ID
	                INNER join ypMember mreq on mreq.ID=lnk.RequestorMemberID
                WHERE l.MemberID in( {0} )
            ) t
            group by t.MemberID";

        private const int InvalidNetSuiteCustomerID = -1;
        private static Mutex _blockingMutex = new Mutex(false, typeof(Program).FullName);
        private static List<string> emailMessages = new List<string>();
        private static Trace trace = new Trace();
        private static List<ProviderItem> providers = new List<ProviderItem>()
        {
            new ProviderItem
            {
                ProviderName = "[Corrigo-GL] USA 191217.062742",
                SubscriptionType = 2,
                ToBeEMailed = true,
            },

            new ProviderItem
            {
                ProviderName = "[Corrigo-GL] Canada 191217.062949",
                SubscriptionType = 2,
                ToBeEMailed = true,
            }
        };
        private static int wonSubscriptionType;
        private static readonly DateTime billingMonth;

        static void Main(string[] args)
        {
            StartBillingProviders();
        }
        private static void StartBillingProviders()
        {
            var result = DoTask(providers);
        }
        private static object DoTask(List<ProviderItem> providers)
        {
            try
            {
                trace.WriteLine("Billing Providers.");

                DateTime startDate = new DateTime(2019, 12, 1); //TODO get real data
                DateTime endDate = new DateTime(2019, 12, DateTime.DaysInMonth(2019, 12)); //TODO get real data

                ChargeProviders(providers, startDate, endDate);

            }
            catch (Exception e)
            {
                //TODO implement exception
            }

            return null;
        }

        //TODO move to appropriate place
        #region Charge Providers
        private static void ChargeProviders(List<ProviderItem> providers, DateTime startDate, DateTime endDate)
        {
            bool ownMutex = false;
            string eMailNotificationBody = String.Empty;

            try
            {
                if (!_blockingMutex.WaitOne(0, true))
                {
                    LogDetailRecord(AuditLogType.Error, "Cannot bill providers due to another active billing process. Please try again later.");
                    return;
                }

                ownMutex = true;

                PriceLevel defaultPriceLevel = new PriceLevel() { name = "Base Price", internalId = "1" };  //load from mock (execute on prepare step) Netsuite !!!! fuckkkk

                int failedToChargeCounter = 0;
                try
                {
                    int successfullyChargedCounter = 0, skippedCounter = 0;

                    //TODO implement correct logic when provider is billed
                    //    if (provider.IsBilled())
                    //    {
                    //        LogDetailRecord(AuditLogType.Warning, String.Format("Provider '{0}' (MemberID: {1}) had been billed already. Skipped.", provider.ProviderName, provider.ProviderId));
                    //        Interlocked.Increment(ref skippedCounter);
                    //    }
                    //    else
                    //    {
                              AuditLogType auditLogType = ChargeProviders(providers, defaultPriceLevel, startDate, endDate);
                    //        switch (auditLogType)
                    //        {
                    //            case AuditLogType.Success:
                    //                Interlocked.Increment(ref successfullyChargedCounter);
                    //                break;
                    //            case AuditLogType.Warning:
                    //                Interlocked.Increment(ref skippedCounter);
                    //                break;
                    //            default:
                    //                Interlocked.Increment(ref failedToChargeCounter);
                    //                break;
                    //        }

                    //        LogDetailRecord(AuditLogType.Information,
                    //            string.Format("Finished charging the provider '{0}'. Subscription type {1}",
                    //                provider.ProviderName, subscriptionRepository.Get(provider.SubscriptionType).Name));
                    //    }


                    eMailNotificationBody = string.Format("Billing statistics:\n\nSuccessfully charged: {0}.\nErrors: {1}.\nSkipped: {2}\n\nPlease see the audit log for details. Thanks.",
                        successfullyChargedCounter, failedToChargeCounter, skippedCounter);
                    LogDetailRecord(AuditLogType.Information, eMailNotificationBody);
                }
                catch (Exception ex)
                {
                    eMailNotificationBody = string.Format("The process of charging is stopped! The unexpected error occurred! Exception information: {0}.", ex);
                    LogDetailRecord(AuditLogType.Error, eMailNotificationBody);
                    failedToChargeCounter++;
                }

                if (failedToChargeCounter > 0)
                    LogDetailRecord(AuditLogType.Error, "The process of charging has finished with errors.");
                else
                    LogDetailRecord(AuditLogType.Success, "The process of charging successfully finished.");
            }
            finally
            {
                SendAccountingEmail(eMailNotificationBody, startDate);
                SendLogedErrors();


                if (ownMutex)
                    _blockingMutex.ReleaseMutex();
            }
        }
        private static AuditLogType ChargeProviders(List<ProviderItem> providers, PriceLevel priceLevel, DateTime startDate, DateTime endDate)
        {
            try
            {
                LogAuditRecord(AuditLogType.Information, string.Format("Preparing providers with billing type '{0}' for billing for {1}.", wonSubscriptionType.ToString(), billingMonth.ToString("yyyy MMMM")));

                IEnumerable<BillableProviderInfo> billableProvidersPortionsWithErrors = LoadBillableProviders((int)wonSubscriptionType);

                //TODO adjust FindInvoicesAndPaymentsForBillingMonth method
                FindInvoicesAndPaymentsForBillingMonth(billableProvidersPortionsWithErrors, billingMonth);
                List<Customer> customers = null;

                LogDetailRecord(AuditLogType.Information, "Loading customers from NetSuite");
                var loadCustomersTime = BillingProvidersApp.Core.Helper.GetTime(() =>
                {
                    customers = ChecktNsCustomer(providers);
                });

                List<string> invoiceInternalIds = new List<string>();
                AuditLogType createResult = CreateInvoiceForProviders(providers, customers, priceLevel, startDate, endDate, out invoiceInternalIds);
                if (invoiceInternalIds.Count == 0 || createResult != AuditLogType.Success)
                {
                    return createResult;
                }

                //provider.OnInvoiced(invoiceInternalId);
                else
                {
                    LogDetailRecord(AuditLogType.Information, String.Format("Skip creating invoice. Using existing invoice (nsInvoiceId: {0}) for provider '{1}'.", null, providers[0].ProviderName));
                }

                AuditLogType payResult = AuditLogType.Error;

                payResult = ApplyPaymentForProviders(providers, customers, new List<CustomerCreditCardsList>(),  invoiceInternalIds);

                if (payResult != AuditLogType.Success)
                    return payResult;

                //provider.OnBilled();
            }
            catch (Exception ex)
            {
                var message = string.Format("Error billing provider '{0}'. Error details: {1}.", providers[0].ProviderName, ex);
                //provider.OnBillError(message, AuditLogType.Error);

                LogDetailRecord(AuditLogType.Error, message);
                return AuditLogType.Error;
            }

            return AuditLogType.Success;
        }
        private static IEnumerable<BillableProviderInfo> LoadBillableProviders(int wonSubscriptionType)
        {
            LogAuditRecord(AuditLogType.Information, String.Format("Start loading all providers from NOC"));

            IList<BillableProviderInfo> nocProviders = null;
            var getFromNocTime = BillingProvidersApp.Core.Helper.GetTime(() =>
            {
                nocProviders = GetNocBillableProviders().ToList();
            });

            nocProviders = FilterByBillableFromRequestors(nocProviders).ToList();

            LogAuditRecord(AuditLogType.Information, String.Format("Providers loaded from NOC in {0}", getFromNocTime));
            LogAuditRecord(AuditLogType.Information, String.Format("Start loading all providers from NetSuite"));


                int[] customersIds = nocProviders
                    .Select(p => p.NsCustomerId)
                    .ToArray();

                List<NsProviderInfo> nsProviderInfos = new List<NsProviderInfo>();
                nsProviderInfos = GetNetSuiteProviders(wonSubscriptionType, customersIds)
                    .ToList();

                // merge NOC and NetSuite results
                var matchedBillableProviders = nocProviders.Join(
                        nsProviderInfos,
                        matchedBillableProvider => matchedBillableProvider.NsCustomerId,
                        nsProviderInfo => nsProviderInfo.InternalId,
                        (matchedBillableProvider, nsProviderInfo) =>
                        {
                            matchedBillableProvider.SubscriptionType = nsProviderInfo.SubscriptionType;

                            //TODO get mbTier, implement missed logic 
                            return matchedBillableProvider;
                        })
                    .ToList();

                foreach (var p in matchedBillableProviders)
                {
                    yield return p;
                }
        }
        private static IEnumerable<BillableProviderInfo> GetNocBillableProviders()
        {
            List<BillableProviderInfo> billableProviders = new List<BillableProviderInfo>();

            string webServiceUrl = SettingsBag.GlobalSettings["yellowpages-webservice-url"];

            using (SqlConnection sqlConnection = new SqlConnection(SQLHelper.ConnectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = new SqlCommand(GetBillableProvidersForSpecificiProvidersCommandText, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@WebServiceUrl", webServiceUrl);
                    sqlCommand.Parameters.AddWithValue("@EndDate", BillableProviderInfo.EndDate);

                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            var providerId = (int)sqlDataReader["ID"];
                            var providerName = sqlDataReader["DisplayAs"] as string;
                            var instanceId = (int)sqlDataReader["InstanceID"];
                            var nsCustomerId = (int)sqlDataReader["nsCustomerID"];
                            var billingCountry = sqlDataReader["BillingCountry"] as string;
                            var langLocale = (int)sqlDataReader["PreferableLanguageCultureID"];
                            var isPrincipalMember = (bool)sqlDataReader["IsPrincipalMember"];
                            var principalMemberID = sqlDataReader["PrincipalMemberID"] == DBNull.Value
                                ? null
                                : (int?)sqlDataReader["PrincipalMemberID"];

                            var billForSmartZones = (bool)sqlDataReader["BillForSmartZones"];

                            BillableProviderInfo result = new BillableProviderInfo(
                                providerId: providerId,
                                instanceId: instanceId,
                                nsCustomerId: nsCustomerId,
                                startDate: BillableProviderInfo.StartDate,
                                endDate: BillableProviderInfo.EndDate,
                                providerName: providerName,
                                billForSmartZones: billForSmartZones,
                                billingCountry: billingCountry,
                                langLocale: langLocale,
                                isPrincipalMember: isPrincipalMember,
                                principalMemberID: principalMemberID);

                            billableProviders.Add(result);
                        }
                    }
                }
            }

            return billableProviders;
        }
        private static IEnumerable<BillableProviderInfo> FilterByBillableFromRequestors(IList<BillableProviderInfo> nocProviders)
        {

                var providerIds = nocProviders.Select(pi => pi.ProviderId).ToArray();

                var requestorAlives = GetAlivesForProviders(providerIds);

                foreach (var nocProvider in nocProviders)
                {
                    var requestorAlive = requestorAlives.FirstOrDefault(a => a.MemberId == nocProvider.ProviderId);
                    if (requestorAlive == null || requestorAlive.Alives > 0 || (requestorAlive.Alives == 0 && requestorAlive.NotAlives == 0))
                    {
                        yield return nocProvider;
                    }
                    else
                    {
                        LogAuditRecord(AuditLogType.Information, String.Format("\"{0}\" has been excluded from billing as they are connected only to non-live requestor(s)", nocProvider.ProviderName));
                    }
                };
        }
        private static List<AlivesForProvider> GetAlivesForProviders(int[] ids)
        {
            List<AlivesForProvider> result = new List<AlivesForProvider>();

            var sqlRequestorAlives = string.Format(SQLRequestorAlives,
                ids.Select(id => id.ToString())
                    .DefaultIfEmpty(string.Empty)
                    .Aggregate((f1, f2) => string.Format("{0},{1}", f1, f2)));

            using (SqlConnection sqlConnection = new SqlConnection(SQLHelper.ConnectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = new SqlCommand(sqlRequestorAlives, sqlConnection))
                {
                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            if (null == sqlDataReader)
                                throw new ArgumentNullException("sqlDataReader");

                            var memberId = (int)sqlDataReader["MemberId"];
                            var alives = (int)sqlDataReader["Alives"];
                            var notAlives = (int)sqlDataReader["NotAlives"];

                            AlivesForProvider item = new AlivesForProvider()
                            {
                                MemberId = memberId,
                                Alives = alives,
                                NotAlives = notAlives
                            };

                            result.Add(item);
                        }
                    }
                }
            }
            return result;
        }
        private static IEnumerable<NsProviderInfo> GetNetSuiteProviders(int wonSubscriptionType, int[] customersIds)
        {
            CustomerSearchAdvanced customerSearch = new CustomerSearchAdvanced()
            {
                columns = new CustomerSearchRow()
                {
                    basic = new CustomerSearchRowBasic()
                    {
                        internalId = new SearchColumnSelectField[] { new SearchColumnSelectField() },
                        customFieldList = new SearchColumnCustomField[]
                        {
                            new SearchColumnSelectCustomField()
                            {
                                scriptId = "" //TODO get WonSubscriptionTypeFieldId
                            }
                        }
                    }
                },

                criteria = new CustomerSearch()
                {
                    basic = new CustomerSearchBasic()
                    {
                        internalId = new SearchMultiSelectField()
                        {
                            @operator = SearchMultiSelectFieldOperator.anyOf,
                            operatorSpecified = true
                        },

                        customFieldList =
                        new SearchCustomField[1]
                        {
                            new SearchMultiSelectCustomField()
                            {
                                scriptId = "", //TODO get WonSubscriptionTypeFieldId,
                                @operator = SearchMultiSelectFieldOperator.anyOf,
                                operatorSpecified = true,
                                searchValue = new ListOrRecordRef[1]
                                {
                                    new ListOrRecordRef()
                                    {
                                        internalId = wonSubscriptionType.ToString()
                                    }
                                }
                            }
                        },

                        custStatus = new SearchMultiSelectField()
                        {
                            searchValue = new RecordRef[1]
                            {
                                new RecordRef()
                                {
                                    internalId = ""//TODO get BillingCustomerStatusName
                                }
                            },
                            @operator = SearchMultiSelectFieldOperator.anyOf,
                            operatorSpecified = true
                        }
                    }
                }
            };


            customerSearch.criteria.basic.internalId.searchValue = customersIds.Select(id =>
                    new RecordRef()
                    {
                        internalId = id.ToString()
                    }
                ).ToArray();

            Task<SearchResult> searchResultTask = null;
            SearchResult searchResult = null;

            NetSuiteHelper.TryRun(() =>
            {
                using (var proxy = NetSuiteHelper.GetProxy())
                {
                    searchResultTask = proxy.searchAsync(customerSearch);
                    return searchResultTask.Result.status.isSuccessSpecified && searchResult.status.isSuccess;
                }
            },
            (message) =>
            {
                LogAuditRecord(AuditLogType.Warning, message);
            });

            CheckResult(searchResult);

            foreach (var res in ProcessNetSuiteResult(searchResult))
                yield return res;

            // read all pages from NetSuite
            while (searchResult.totalPagesSpecified && searchResult.totalPages > 1 &&
                   searchResult.pageIndexSpecified && searchResult.pageIndex < searchResult.totalPages)
            {
                NetSuiteHelper.TryRun(() =>
                {
                    searchResultTask = NetSuiteHelper.GetProxy().searchMoreWithIdAsync(searchResult.searchId, searchResult.pageIndex + 1);
                    return searchResultTask.Result.status.isSuccessSpecified && searchResult.status.isSuccess;
                },
                (message) =>
                {
                    LogAuditRecord(AuditLogType.Warning, message);
                });
                CheckResult(searchResult);

                foreach (var res in ProcessNetSuiteResult(searchResult))
                    yield return res;
            }
        }

        private static void FindInvoicesAndPaymentsForBillingMonth(IEnumerable<BillableProviderInfo> billableProviders,
            DateTime billingMonth)
        {
            if (null == billableProviders)
                throw new ArgumentNullException("billableProviders");

            Dictionary<Tuple<int, string>, TransactionData> foundInvoicesAndPayments = new Dictionary<Tuple<int, string>, TransactionData>();
            var elapsed = BillingProvidersApp.Core.Helper.GetTime(() =>
            {
                DateTime startDate = new DateTime(billingMonth.Year, billingMonth.Month, 1);
                DateTime endDate = new DateTime(
                    billingMonth.Year,
                    billingMonth.Month,
                    DateTime.DaysInMonth(billingMonth.Year, billingMonth.Month),
                    23, 59, 59);
                endDate = endDate.AddHours(1);

                InvoiceManager invoiceManager = new InvoiceManager((message) =>
                {
                    LogDetailRecord(AuditLogType.Warning, message);
                });
                foundInvoicesAndPayments = SearchInvoicesAndPaymentsWithMaxDate<TransactionData>(
                    startDate,
                    endDate,
                    billableProviders
                        .GroupBy(p => p.NsCustomerId)
                        .Select(group => new RecordRef()
                        {
                            internalId = group.Key.ToString()
                        }).ToArray(),
                    (prevValue, currentValue) =>
                    {
                        var nsCustomerId = int.Parse(currentValue.entity[0].searchValue.internalId);
                        var providerDisplayName = currentValue.customFieldList == null ? "" : ((SearchColumnStringCustomField)currentValue.customFieldList.First()).searchValue;
                        providerDisplayName = ""; //TODO get HttpUtility.HtmlDecode(providerDisplayName);

                        var invoiceSums = billableProviders
                            .Where(p => p.NsCustomerId == nsCustomerId && p.Sum > 0 &&
                                (string.IsNullOrEmpty(providerDisplayName) || (providerDisplayName.StartsWith(p.ProviderName + '\n') || providerDisplayName == p.ProviderName)))
                            .Select(p => p.Sum)
                            .ToArray();

                        decimal currentSum = (decimal)currentValue.fxAmount[0].searchValue;

                        if (prevValue != null)
                        {
                            DateTime? previousTranDate = prevValue.LastInvoiced;
                            DateTime currentTranDate = currentValue.tranDate[0].searchValue;

                            if (currentTranDate <= previousTranDate)
                                return null;
                        }

                        if (invoiceSums.Contains(currentSum))
                        {
                            string invoiceId = currentValue.internalId[0].searchValue.internalId;
                            DateTime? lastBilled = null;
                            DateTime? lastInvoiced = currentValue.tranDate[0].searchValue;

                            if ("paidInFull" == currentValue.status[0].searchValue)
                            {
                                lastBilled = lastInvoiced;
                            }

                            return new TransactionData()
                            {
                                InvoiceId = invoiceId,
                                LastInvoiced = lastInvoiced,
                                LastBilled = lastBilled,
                                CurrentSum = currentSum,
                                ProviderDisplayName = providerDisplayName
                            };
                        }
                        return null;
                    }
                );

                foreach (KeyValuePair<Tuple<int, string>, TransactionData> tempInvoicePaymentPair in foundInvoicesAndPayments)
                {
                    billableProviders
                        .Where(p => p.NsCustomerId == tempInvoicePaymentPair.Key.Item1 &&
                            (string.IsNullOrEmpty(tempInvoicePaymentPair.Value.ProviderDisplayName) ||
                            (p.ProviderName == tempInvoicePaymentPair.Value.ProviderDisplayName || tempInvoicePaymentPair.Value.ProviderDisplayName.StartsWith(p.ProviderName + '\n'))))
                        .ToList()
                        .ForEach(tempBillableProviderInfo =>
                        {
                            if (tempBillableProviderInfo.Sum > 0)
                            {
                                tempBillableProviderInfo.TransactionSearchRowBasic = tempInvoicePaymentPair.Value;
                            }
                        });
                }
            });

            LogAuditRecord(AuditLogType.Information, string.Format("Found {0} invoices for billable provides in {1}. Processing time: {2}.",
                foundInvoicesAndPayments.Count, billingMonth.ToString("MMM"), elapsed));
        }

        /// <summary>
        /// Performs the search of invoices in the period between startDate and endDate for the customers
        /// </summary>
        /// <param name="startDate">Invoice search start date</param>
        /// <param name="endDate">Invoice search end date</param>
        /// <param name="customersRef">Object array of the <see cref="Invoice"/>type</param>
        /// <param name="customersArray"></param>
        /// <param name="acceptItem"></param>
        /// <returns>Objects array of the <see cref="TransactionSearchRowBasic"/>type</returns>
        private static Dictionary<Tuple<int, string>, TTransactionData> SearchInvoicesAndPaymentsWithMaxDate<TTransactionData>(DateTime startDate, DateTime endDate, RecordRef[] customersArray,
            Func<TTransactionData/*prev value*/, TransactionSearchRowBasic/*current value*/, TTransactionData/*accept*/> acceptItem)
            where TTransactionData : class
        {
            Dictionary<Tuple<int, string>, TTransactionData> foundData = new Dictionary<Tuple<int, string>, TTransactionData>();
            if ((null == customersArray) || (0 == customersArray.Length))
                return foundData;

            var maxSearchListRecordsCount = 50; //TODO NetSuiteHelper.MaxSearchListRecordsCount;
            for (int index = 0; index * maxSearchListRecordsCount < customersArray.Length; index++)
            {
                int startIndex = index * maxSearchListRecordsCount;
                int elementsCount = (startIndex + maxSearchListRecordsCount) > customersArray.Length ?
                    (customersArray.Length - startIndex) : maxSearchListRecordsCount;

                RecordRef[] tempCustomersArray = new RecordRef[elementsCount];
                Array.Copy(customersArray, startIndex, tempCustomersArray, 0, elementsCount);

                TransactionSearchAdvanced transactionSearchAdvanced = new TransactionSearchAdvanced()
                {
                    columns = new TransactionSearchRow()
                    {
                        basic = new TransactionSearchRowBasic()
                        {
                            entity = new SearchColumnSelectField[] { new SearchColumnSelectField() },
                            tranDate = new SearchColumnDateField[] { new SearchColumnDateField() },
                            amount = new SearchColumnDoubleField[] { new SearchColumnDoubleField() },
                            fxAmount = new SearchColumnDoubleField[] { new SearchColumnDoubleField() },
                            status = new SearchColumnEnumSelectField[] { new SearchColumnEnumSelectField() },
                            internalId = new SearchColumnSelectField[] { new SearchColumnSelectField() },
                            customFieldList = new SearchColumnCustomField[]
                            {
                                new SearchColumnStringCustomField()
                                {
                                    scriptId = "" //TODO get Corrigo.Web.Noc.NetSuiteClient.Helper.GetWonInvoiceInstanceFieldId()
                                }
                            }
                        }
                    },
                    criteria = new TransactionSearch()
                    {
                        basic = new TransactionSearchBasic()
                        {
                            type = new SearchEnumMultiSelectField()
                            {
                                @operator = SearchEnumMultiSelectFieldOperator.anyOf,
                                operatorSpecified = true,
                                searchValue = new string[] { "_invoice" }
                            },
                            mainLine = new SearchBooleanField()
                            {
                                searchValue = true,
                                searchValueSpecified = true
                            },
                            endDate = new SearchDateField()
                            {
                                @operator = SearchDateFieldOperator.within,
                                operatorSpecified = true,
                                searchValue = startDate,
                                searchValueSpecified = true,
                                searchValue2 = endDate,
                                searchValue2Specified = true
                            }
                        },
                        customerJoin = new CustomerSearchBasic()
                        {
                            internalId = new SearchMultiSelectField()
                            {
                                @operator = SearchMultiSelectFieldOperator.anyOf,
                                operatorSpecified = true,
                                searchValue = tempCustomersArray
                            }
                        }
                    }
                };

                SearchResult searchResult = null;
                NetSuiteHelper.TryRun(() =>
                {
                    using (var proxy = NetSuiteHelper.GetProxy())
                    {
                        //TODO implement proper logic
                        //searchResult = proxy.searchAsync(transactionSearchAdvanced);
                        //return searchResult.status.isSuccessSpecified && searchResult.status.isSuccess;
                    }
                },
                (message) =>
                {
                    //TODO LogMessage?.Invoke(message);
                });

                if (!(searchResult.status.isSuccessSpecified && searchResult.status.isSuccess))
                    //TODO implement proper logic
                    //throw new Exceptions.NetSuiteException(Utils.ParseStatus(NetSuiteOperationType.Search,
                    //    searchResult.status, RecordType.invoice));


                    while (searchResult.totalRecords > (searchResult.pageIndex * searchResult.pageSize))
                {
                    NetSuiteHelper.TryRun(() =>
                    {
                        using (var proxy = NetSuiteHelper.GetProxy())
                        {
                            //TODO implement proper logic
                            //searchResult = proxy.searchMoreAsync(searchResult.pageIndex + 1);
                            //return searchResult.status.isSuccessSpecified && searchResult.status.isSuccess;
                        }
                    },
                    (message) =>
                    {
                        //TODO LogMessage?.Invoke(message);
                    });

                        if (searchResult.status.isSuccessSpecified && searchResult.status.isSuccess) { }
                        //TODO implement proper logic
                        else { }
                       //TODO implement proper logic
                       //throw new Exceptions.NetSuiteException(Utils.ParseStatus(NetSuiteOperationType.Search,
                       //     searchResult.status, RecordType.invoice));
                }
            }

            return foundData;
        }

        private static AuditLogType CreateInvoiceForProviders(List<ProviderItem> providers, List<Customer> customers, PriceLevel priceLevel, DateTime startDate, DateTime endDate, out List<string> invoiceInternalIds)
        {
            DateTime transactionDate = endDate;
            DateTime currentDate = DateTime.Now;
            invoiceInternalIds = new List<string>();
            List<Record> invoices_l = new List<Record>();

            for (int i = 0; i < 1; i++)
            {
                invoices_l.AddRange(new List<Record>() {
                    InvoiceManager.CreateInvoice(
                        customers[0].internalId,
                        RecordType.customer,
                        startDate,
                        endDate,
                        transactionDate,
                        priceLevel,
                        providers[0].ToBeEMailed,
                        providers[0].FormId,
                        providers[0].ProviderName,
                        providers[0].PriceListItems
                        ),

                    InvoiceManager.CreateInvoice(
                        customers[1].internalId,
                        RecordType.customer,
                        startDate,
                        endDate,
                        transactionDate,
                        priceLevel,
                        providers[1].ToBeEMailed,
                        providers[1].FormId,
                        providers[1].ProviderName,
                        providers[1].PriceListItems
                        ),
                    });
            }

            Record[] invoices = invoices_l.ToArray();

            InvoiceManager invoiceManager = new InvoiceManager();
            WriteResponseList addInvoiceResults = invoiceManager.AsyncAddList(invoices);

            foreach (var item in addInvoiceResults.writeResponse)
            {
                invoiceInternalIds.Add(item.baseRef.GetInternalId());
            }



            if (!addInvoiceResults.status.isSuccess)
            {
                var message = string.Format("Invoice cannot be created in NetSuite for the provider '{0}'. Details: {1}.",
                        providers[0].ProviderName);// addInvoiceResults[0].OperationLog);

                //providers[0].OnBillError(message, AuditLogType.Error);

                string providerName = providers[0].ProviderName;
                LogErrorAndEmail(message, providerName);
                return AuditLogType.Error;
            }

            return AuditLogType.Success;
        }
        private static AuditLogType ApplyPaymentForProviders(List<ProviderItem> providers, List<Customer> customers, List<CustomerCreditCardsList> customerCreditCardsList, List<string> invoiceInternalIds)
        {
            string providerName = providers[0].ProviderName;
            int subscriptionType = providers[0].SubscriptionType;
            var paymentType = providers[0].PaymentType;

            var payByCreditCard = (PaymentType)paymentType == PaymentType.EFT;

            if (payByCreditCard)
            {
                int count = 0;
                foreach (var customer in customers)
                {

                    if (customerCreditCardsList == null || customerCreditCardsList[count].creditCards == null //here is the problem we can't get this info from database -- it comes from netsuite ... not a problem anymaore
                        || customerCreditCardsList[count].creditCards.Length == 0)
                    {
                        var message = string.Format(
                                "The payment can't be accepted automatically from provider '{3}' (NetSuite Customer Internal ID {0}). Reason: there are no credit cards linked to the NetSuite customer.\n NOTE: Invoice has been created in NetSuite: Subscription type: {1}, NetSuite Invoice ID: {2}.",
                                customer.internalId,
                                subscriptionType,
                                invoiceInternalIds[count],
                                providerName);
                        //provider.OnBillError(message, AuditLogType.Error);
                        LogErrorAndEmail(message, providerName);
                        return AuditLogType.Error;
                    }

                    CustomerCreditCards nonExpiredCreditCard = customer.creditCardsList.creditCards.FirstOrDefault(
                        creditCard => !IsCredirCardExpired(creditCard)
                        );

                    if (nonExpiredCreditCard == null)
                    {
                        var message = string.Format(
                                "The payment can't be accepted automatically from provider '{3}' ( NetSuite Customer Internal ID {0}). Reason: all customer credit cards are expired or invalid in NetSuite.\n NOTE: Invoice has been created in NetSuite: Subscription type: {1}, NetSuite Invoice ID: {2}.",
                                customer.internalId,
                                subscriptionType,
                                invoiceInternalIds[count],
                                providerName);
                        //provider.OnBillError(message, AuditLogType.Error);
                        LogErrorAndEmail(message, providerName);
                        return AuditLogType.Error;
                    }

                    count++;
                }
            }

            CustomerPaymentManager customerPaymentManager = new CustomerPaymentManager((message) =>
            {
                //provider.OnBillError(message, AuditLogType.Warning);
            });
            

            List<NetSuiteReadWriteResult> addCustomerPaymentResults = customerPaymentManager.ApplyPayments(customers, invoiceInternalIds, payByCreditCard);

            if (!addCustomerPaymentResults[0].IsSuccess)
            {
                var message = string.Format(
                        "Error billing Provider '{2}' (NetSuite Internal ID {0}). Details: {1}",
                        customers[0].internalId,
                        addCustomerPaymentResults[0].OperationLog,
                        providerName);

                //provider.OnBillError(message, AuditLogType.Error);
                LogErrorAndEmail(message, providerName);
                return AuditLogType.Error;
            }


            LogDetailRecord(AuditLogType.Success,
                string.Format(
                    "Success billing provider '{0}'. Subscription type: {1}; NetSuite Invoice ID: {2}; Customer Payment ID: {3} ",
                    providerName, subscriptionType, invoiceInternalIds[0],
                    ((RecordRef)addCustomerPaymentResults[0].BaseRef).internalId));
            return AuditLogType.Success;
        }
        #endregion

        //TODO move to appropriate place
        #region Helper Methods
        private static void CheckResult(SearchResult searchResult)
        {
            if (!(searchResult.status.isSuccessSpecified && searchResult.status.isSuccess))
                throw new Exception();
        }
        private static IEnumerable<NsProviderInfo> ProcessNetSuiteResult(SearchResult searchResult)
        {
            var res = searchResult.searchRowList.Select(row =>
            {
                int subscriptionTypeId = ((CustomerSearchRow)row).basic.customFieldList == null
                    ? 0
                    : Int32.Parse(((SearchColumnSelectCustomField)((CustomerSearchRow)row).basic.customFieldList.First()).searchValue.internalId);

                return new NsProviderInfo()
                {
                    InternalId = Int32.Parse(((CustomerSearchRow)row).basic.internalId.First().searchValue.internalId),
                    SubscriptionType = subscriptionTypeId
                };
            })
            .ToArray();

            return res;
        }
        private static void LogAuditRecord(AuditLogType auditLogType, string msg)
        {
            lock (trace)
            {
                trace.WriteLine(msg, (MessageCategory)auditLogType);
            }
        }
        private static void LogErrorAndEmail(string message, string providerName)
        {
            LogDetailRecord(AuditLogType.Error, message);

            emailMessages.Add(message);
        }
        static string SalesEmail
        {
            get { return "sales-email"; }
        }
        private static void SendLogedErrors()
        {

            if (emailMessages.Any())
            {
                string errorMessage = emailMessages
                    .DefaultIfEmpty("")
                    .Aggregate((current, next) => current + "\r\n\r\n " + next);

                EmailHelper.SendMail(errorMessage, "Error charging the providers", SalesEmail, MailPriority.High, false);
            }
        }
        private static string AccountingEmail
        {
            get { return "accounting-email"; }
        }
        private static void SendAccountingEmail(string eMailNotificationBody, DateTime startDate)
        {
            // Send e-mail notification to accounting
            if (!string.IsNullOrEmpty(AccountingEmail))
            {
                EmailHelper.SendMail(eMailNotificationBody,
                    string.Format("Finished billing providers. Billing month: {0}", startDate.ToString("MMMM yyyy")),
                    AccountingEmail, MailPriority.Normal, false);
            }
        }
        private static List<Customer> ChecktNsCustomer(List<ProviderItem> providers)
        {
            var providerIds = providers.Select(provider => provider.ProviderId).ToArray();

            var entities = DAL.EntitiesSingleton.GetEntities();
            var accountMembers = entities.ypMembers
                .Where(m => providerIds.Contains(m.ID))
                .Join(entities.nocAccounts, member => member.AccountID, account => account.ID,
                    (member, account) => new
                    {
                        member,
                        account
                    })
                .ToList();

            List<int> nsCustomerIds = new List<int>();
            List<ProviderItem> providersWithNsIds = new List<ProviderItem>();

            providers.ToList().ForEach(provider =>
            {
                var accountMember = accountMembers.FirstOrDefault(am => am.member.ID == provider.ProviderId);

                if (accountMember == null || accountMember.account == null)
                {
                    var message = string.Format("Unable to find Account by ProviderID {0} for {1}.", provider.ProviderId, provider.ProviderName);

                    //provider.OnBillError(message, AuditLogType.Error);

                    LogErrorAndEmail(message, provider.ProviderName);
                    return;
                }

                if (!accountMember.account.nsCustomerID.HasValue)
                {
                    var message = string.Format("The account '{0}' associated with the provider '{1}' is not linked to NetSuite.",
                            accountMember.account.Name, provider.ProviderName);
                    //provider.OnBillError(message, AuditLogType.Error);
                    LogErrorAndEmail(message, provider.ProviderName);
                    return;
                }

                int nsCustomerId = accountMember.account.nsCustomerID.Value;
                

                if (InvalidNetSuiteCustomerID == nsCustomerId)
                {
                    var message = string.Format("The account '{0}' associated with the provider '{1}' had broken link to NetSuite.",
                            accountMember.account.Name, provider.ProviderName);
                    //provider.OnBillError(message, AuditLogType.Error);
                    LogErrorAndEmail(message, provider.ProviderName);

                    return;
                }

                nsCustomerIds.Add(nsCustomerId);
                providersWithNsIds.Add(provider);
            });

            CustomerManager customerManager = new CustomerManager((message) =>
            {
                LogDetailRecord(AuditLogType.Warning, message);
            });
            NetSuiteReadWriteResult[] getCustomerResults = null;
            NetSuiteHelper.TryRun(() =>
            {
                getCustomerResults = customerManager.GetList(
                    nsCustomerIds.Select(nsCustomerId =>
                                new RecordRef()
                                {
                                    internalId = nsCustomerId.ToString(),
                                    type = customerManager.RecordType,
                                    typeSpecified = true
                                }).ToArray()

                );
            },
            (message) =>
            {
                LogDetailRecord(AuditLogType.Warning, message);
            });

            List<Customer> result = new List<Customer>();

            for (int i = 0; i < getCustomerResults.Length; i++)
            {
                var getCustomerResult = getCustomerResults[i];
                var nsCustomerId = nsCustomerIds[i];
                var provider = providersWithNsIds[i];

                if (!getCustomerResult.IsSuccess)
                {
                    var message = string.Format(
                            "Can't obtain the customer with internal ID = {0} from the NetSuite. Details: {1}.",
                            nsCustomerId,
                            getCustomerResult.OperationLog);
                    //provider.OnBillError(message, AuditLogType.Error);
                    LogErrorAndEmail(message, provider.ProviderName);
                    {
                        continue;
                    }
                }

                var customer = (Customer)getCustomerResult.Record;
                if (customer.isInactive)
                {
                    var message = string.Format(
                            "Invoice can't be created automatically for the Provider {1} (NetSuite Customer Internal ID {0}). Reason: NetSuite Customer is inactive!",
                            customer.internalId,
                            provider.ProviderName);
                    //provider.OnBillError(message, AuditLogType.Error);
                    LogErrorAndEmail(message, provider.ProviderName);
                    {
                        {
                            continue;
                        }
                    }
                }

                result.Add(customer);
            }
            return result;
        }
        private static void LogDetailRecord(AuditLogType auditLogType, string msg)
        {
            trace.WriteLine(msg, (MessageCategory)auditLogType);
        }
        private enum AuditLogType
        {
            Error = 1,
            Success = 2,
            Warning = 3,
            Information = 4
        }
        private static bool IsCredirCardExpired(CustomerCreditCards creditCard)
        {
            if (null == creditCard)
                throw new ArgumentNullException("creditCard");

            DateTime today = DateTime.Today;

            return !(creditCard.ccExpireDateSpecified &&
                ((creditCard.ccExpireDate.Year > today.Year) ||
                ((creditCard.ccExpireDate.Year == today.Year) && (creditCard.ccExpireDate.Month >= today.Month))));
        }
        #endregion

    }

    //TODO move to appropriate place
    #region Helper Classes
    public class CustomerPaymentManager:  NetSuiteEntityManager
    {
        private static string _eftPaymentMethodInternalId = null;
        private static AuditLog _auditLog = AuditLog.AddLog("CustomerPaymentManager trace");
        public CustomerPaymentManager(Action<string> logMessage) : base(RecordType.customerPayment, logMessage)
        {
        }
         /// <summary>
        /// Pays the invoice in NetSuite
        /// </summary>
        /// <param name="customer">Instance of the <see cref="Customer"/> type</param>
        /// <param name="invoiceInternalId">NetSuite Invoice Internal ID</param>
        /// <param name="payByCreditCard">Specified that the credit card payment should be done.</param>
        /// <returns></returns>
        public List<NetSuiteReadWriteResult> ApplyPayments(List<Customer> customers, List<string> invoiceInternalIds, bool payByCreditCard = true)
        {
            InvoiceManager invoiceManager = new InvoiceManager((message) =>
            {
                LogMessage?.Invoke(message);
            });
            IEnumerable<NetSuiteReadWriteResult> getInvoiceResultsEnumeration = invoiceManager.Get(invoiceInternalIds); // here is we need to be very carefully coz we can fail on first attempt
            var getInvoiceResults = getInvoiceResultsEnumeration.ToList();

            List<Invoice> invoices = new List<Invoice>();
            foreach (var item in getInvoiceResults)
            {
                invoices.Add((Invoice)item.Record);
            }

            if (!getInvoiceResults[0].IsSuccess)
                return getInvoiceResults;

            return ApplyPayments(customers, invoices, payByCreditCard).ToList();
        }
        public IEnumerable<NetSuiteReadWriteResult> ApplyPayments(List<Customer> customers, List<Invoice> invoices, bool payByCreditCard = true)
        {
            List<Record> payments = new List<Record>();
            int count = 0;
            foreach (var customer in customers)
            {
                CustomerPayment payment = new CustomerPayment();

                if (payByCreditCard)
                {
                    payment.undepFunds = true;
                    payment.undepFundsSpecified = true;
                }

                payment.ccAvsStreetMatch = AvsMatchCode._n;
                payment.ccAvsStreetMatchSpecified = true;

                payment.ccAvsZipMatch = AvsMatchCode._n;
                payment.ccAvsZipMatchSpecified = true;

                payment.ignoreAvs = true;
                payment.ignoreAvsSpecified = true;

                payment.customer = new RecordRef();
                payment.customer.internalId = customer.internalId;

                CustomerPaymentApply invoiceApply = new CustomerPaymentApply();
                invoiceApply.total = invoices[count].total;
                invoiceApply.totalSpecified = true;
                invoiceApply.refNum = invoices[count].tranId;
                invoiceApply.apply = true;
                invoiceApply.doc = Convert.ToInt64(invoices[count].internalId);
                invoiceApply.docSpecified = true;
                invoiceApply.applySpecified = true;
                invoiceApply.type = "Invoice";

                payment.applyList = new CustomerPaymentApplyList();
                payment.applyList.apply = new CustomerPaymentApply[] { invoiceApply };

                payment.autoApply = false;
                payment.autoApplySpecified = true;

                payment.payment = invoices[count].total;
                payment.paymentSpecified = true;

                payment.memo = customer.entityId;

                string paymentMethodInternalId = null;

                if (payByCreditCard)
                {
                    CustomerCreditCards creditCard = BillingProvidersApp.Managers.Helper.GetDefaultOrFirstCustomerCreditCards(customer.creditCardsList);
                    if (creditCard != null)
                    {
                        payment.creditCard = new RecordRef();
                        payment.creditCard.internalId = creditCard.internalId;

                        if (creditCard.paymentMethod != null)
                            paymentMethodInternalId = creditCard.paymentMethod.internalId;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(_eftPaymentMethodInternalId))
                        paymentMethodInternalId = _eftPaymentMethodInternalId;
                }

                if (!string.IsNullOrEmpty(paymentMethodInternalId))
                {
                    payment.paymentMethod = new RecordRef();
                    payment.paymentMethod.internalId = paymentMethodInternalId;
                }

                payments.Add(payment);
            }

            Task<WriteResponseList> saveResponseTask = null;
            NetSuiteHelper.TryRun(() =>
            {
                var proxy = NetSuiteHelper.GetProxy();

                Stopwatch sw = new Stopwatch();
                DateTime timestamp = DateTime.UtcNow;
                sw.Start();
                saveResponseTask = proxy.addListAsync(payments.ToArray());
                sw.Stop();

                _auditLog.AddLogDetails(AuditLogType.Information, $"Apply payment takes {sw.ElapsedMilliseconds}ms. Started at {timestamp}. Request token: {proxy.tokenPassport.token}");
            });


            var saveResponses = saveResponseTask.Result;
            count = 0;
            foreach (var saveResponse in saveResponses.writeResponse)
            {
                if (saveResponse.status.isSuccess)
                {
                    ((CustomerPayment)payments[count]).internalId = ((RecordRef)saveResponse.baseRef).internalId;
                    ((CustomerPayment)payments[count]).externalId = ((RecordRef)saveResponse.baseRef).externalId;
                }

                yield return new NetSuiteReadWriteResult(saveResponse.status, RecordType,
                    NetSuiteOperationType.Add, payments[count], saveResponse.baseRef);

                count++;
            }

        }
    }
    public class CustomerManager : NetSuiteEntityManager
    {
        #region Construction

        /// <summary>
        /// Type constructor.
        /// </summary>
        public CustomerManager(Action<string> logMessage = null) : base(RecordType.customer, logMessage) { }

        public CustomerManager() : base(RecordType.customer, null) { }

        #endregion
    }
    internal class ProviderItem
    {

        public ProviderItem(string providerName, int providerid, int subscriptionType, int workOrders, int? billableWorkOrders,
                            string countryCode, int regionsCount, int? billableRegionsCount, bool billForSmartZones, int langLocale)
        {
            ProviderName = providerName;
            ProviderId = providerid;
            SubscriptionType = subscriptionType;
        }
        public ProviderItem() { }
        public int FormId { get; set; }
        public IEnumerable<PriceListItem> PriceListItems { get; set; }
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
        public int SubscriptionType { get; set; }
        public int PaymentType { get; set; }
        public bool ToBeEMailed { get; set; }
        public DateTime TransactionDate { get; set; }
    }
    internal class BillableProviderInfo : ProviderItem
    {
        internal BillableProviderInfo(
            int providerId,
            int instanceId,
            int nsCustomerId,
            string billingCountry,
            string providerName,
            bool billForSmartZones,
            bool isPrincipalMember,
            int? principalMemberID, DateTime startDate, DateTime endDate, int langLocale           
        )
            : base(providerName, providerId, 0, 0, null, billingCountry, 0, null, billForSmartZones, langLocale)
        {
            ProviderId = providerId;
            InstanceId = instanceId;
            NsCustomerId = nsCustomerId;
            StartDate = startDate;
            EndDate = endDate;
            IsPrincipalMember = isPrincipalMember;
            PrincipalMemberID = principalMemberID;
        }

        public int? PrincipalMemberID { get; set; }

        public bool IsPrincipalMember { get; set; }

        public static DateTime StartDate
        {
            get;
            set;
        }

        public static DateTime EndDate
        {
            get;
            set;
        }

        public int ProviderId
        {
            get;
            set;
        }

        public int InstanceId
        {
            get;
            set;
        }

        public int NsCustomerId
        {
            get;
            set;
        }

        public decimal Sum
        {
            get;
            set;
        }

        public TransactionData TransactionSearchRowBasic
        {
            get;
            set;
        }
    }
    public class TransactionData
    {
        public DateTime? LastInvoiced { get; set; }

        public DateTime? LastBilled { get; set; }

        public decimal CurrentSum { get; set; }

        public string InvoiceId { get; set; }

        public string ProviderDisplayName { get; set; }
    }
    public class AlivesForProvider
    {
        public int MemberId { get; set; }
        public int Alives { get; set; }
        public int NotAlives { get; set; }
    }
    public class NsProviderInfo
    {
        public int InternalId { get; internal set; }
        public int SubscriptionType { get; internal set; }
    }
    #endregion
}
