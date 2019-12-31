using BillingProvidersApp.Core;
using SuiteTalk;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Threading;

namespace BillingProvidersApp.Managers
{

    public static class NetSuiteHelper
    {
        private static string _netSuiteUrlTemplate;
        private static bool _isNsEnabled;
        private static AuditLog _auditLog = AuditLog.AddLog("NetSuiteHelper trace");


        internal const int DefaultMaxUpdateRecordsCount = 40;
        internal const int DefaultMaxAddRecordsCount = 20;
        internal const int DefaultMaxDeleteRecordsCount = 50;
        internal const int DefaultMaxGetListRecordsCount = 200;
        internal const int DefaultMaxSearchListRecordsCount = 50;
        internal const int DefaultMaxNumberOfAttempts = 5;

        public const string ConnectedPartyNotProperlyRespond = "connected party did not properly respond";
        public const string ConnectionWasClosedByRemoteHost = "An existing connection was forcibly closed by the remote host";
        public const string ConcurrentRequestLimit = "concurrent request limit exceeded";
        public const string UnexpectedError = "An unexpected error has occurred";
        public const string ContentTypeError = "Client found response content type";
        public const string InvalidXML = "The XML declaration must be the first node in the document";
        public const string BadGateway = "The request failed with HTTP status 502: Bad Gateway";

        private static string _netSuiteUrl;
        private static object _locker = new Object();

        public static NetSuitePortTypeClient GetProxy()
        {
            try
            {
                NetSuitePortTypeClient proxy = new NetSuitePortTypeClient();

                proxy.preferences = new Preferences();
                proxy.preferences.ignoreReadOnlyFields = true;
                proxy.preferences.ignoreReadOnlyFieldsSpecified = true;

                proxy.tokenPassport = CreateTokenPassport();

                return proxy;
            }
            catch (Exception e)
            {
                int logId = AuditLog.AddLog("NetSuiteHelper GetProxy() error").Id;

                AuditLog.AddLogDetails(logId, AuditLogType.Error, e.Message);

                if (e.InnerException != null)
                    AuditLog.AddLogDetails(logId, AuditLogType.Error, e.InnerException.Message);

                throw;
            }
        }

        private static TokenPassport CreateTokenPassport()
        {
            Stopwatch sw = new Stopwatch();

            string account = "606500_SB1";
            string consumerKey = "e0a74c549bfa07f950eff91b41292ab3bc4cf9e62718cea85fc5fe87950272e3";
            string consumerSecret = "cfeb3ac9ac84ad933c08e1de083660204cbf3844231d75341603f1d532f1d4eb";
            string tokenId = "9bcab909033a99936c7e569b6432be961f432c4336131f88779883f4b9d743c2";
            string tokenSecret = "f027714435a5b30aa7af437a5a93b129b2c71c41db4a1d5dd6202abda00a3016";

            string nonce = ComputeNonce();
            long timestamp = ComputeTimestamp();
            sw.Start();
            TokenPassportSignature signature = ComputeSignature(account, consumerKey, consumerSecret, tokenId, tokenSecret, nonce, timestamp);

            TokenPassport tokenPassport = new TokenPassport();
            tokenPassport.account = account;
            tokenPassport.consumerKey = consumerKey;
            tokenPassport.token = tokenId;
            tokenPassport.nonce = nonce;
            tokenPassport.timestamp = timestamp;
            tokenPassport.signature = signature;
            sw.Stop();

            //_auditLog.AddLogDetails(AuditLogType.Information, $"{DateTime.UtcNow} Timestamp: {timestamp}, Token: {tokenId}, signature generation, ms: {sw.ElapsedMilliseconds}");

            return tokenPassport;
        }

        private static string ComputeNonce()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] data = new byte[20];
            rng.GetBytes(data);
            int value = Math.Abs(BitConverter.ToInt32(data, 0));
            return value.ToString();
        }

        private static long ComputeTimestamp()
        {
            return ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }

        private static TokenPassportSignature ComputeSignature(string compId, string consumerKey, string consumerSecret,
                                        string tokenId, string tokenSecret, string nonce, long timestamp)
        {
            string baseString = compId + "&" + consumerKey + "&" + tokenId + "&" + nonce + "&" + timestamp;
            string key = consumerSecret + "&" + tokenSecret;
            string signature = "";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyBytes = encoding.GetBytes(key);
            byte[] baseStringBytes = encoding.GetBytes(baseString);
            using (var hmacSha1 = new HMACSHA1(keyBytes))
            {
                byte[] hashBaseString = hmacSha1.ComputeHash(baseStringBytes);
                signature = Convert.ToBase64String(hashBaseString);
            }
            TokenPassportSignature sign = new TokenPassportSignature();
            sign.algorithm = "HMAC-SHA1";
            sign.Value = signature;
            return sign;
        }


        public static int MaxGetListRecordsCount
        {
            get
            {
                var nsMaxGetListRecordsCount = ConfigurationManager.AppSettings["NsMaxGetListRecordsCount"];
                int result;
                if (string.IsNullOrEmpty(nsMaxGetListRecordsCount) || !int.TryParse(nsMaxGetListRecordsCount, out result))
                    return DefaultMaxGetListRecordsCount;
                return result;
            }
        }



        public static void TryRun(Action action, Action<string> log = null, string[] messageText = null)
        {
            TryRun(() =>
            {
                action();
                return true;
            }, log, messageText);
        }

        public static void TryRun(Func<bool> action, Action<string> log, string[] messageText = null)
        {
            if (messageText == null || messageText.Length == 0)
            {
                messageText = new string[] {
                ConnectedPartyNotProperlyRespond,
                ConnectionWasClosedByRemoteHost,
                ConcurrentRequestLimit,
                UnexpectedError,
                InvalidXML,
                ContentTypeError,
                BadGateway
                };
            }

            int tryCount = 10;
            var random = new Random();

            for (int i = 1; i <= tryCount; i++)
            {
                try
                {
                    action();

                    if (i > 1)
                        log?.Invoke("Processed NS request successfully");
                    break;
                }
                catch (WebException ex)
                {
                   log?.Invoke($"Atempt number {i} of {tryCount} to call NetSuite method has finished with error. Trying again...");
                }
                catch (InvalidOperationException ex)
                {
                    log?.Invoke($"Atempt number {i} of {tryCount} to call NetSuite method has finished with error. Trying again...");
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Atempt number {i} of {tryCount} to call NetSuite method has finished with error. Trying again...");
                }
                Thread.Sleep(random.Next(3, 10) * 1000);
            }
        }

        private static bool ExceptionHasMessage(Exception ex, string message)
        {
            if (ex.Message != null && ex.Message.Contains(message))
                return true;

            if (ex.InnerException == null)
                return false;

            return ExceptionHasMessage(ex.InnerException, message);
        }

    }
}
