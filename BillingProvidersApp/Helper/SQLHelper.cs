using System;
using System.Configuration;

namespace BillingProvidersApp.Helper
{
    public static class SQLHelper
    {
        private static string _connectionString;
        private static object _lock = new object();

        public static String ConnectionString
        {
            get
            {
                if (String.IsNullOrEmpty(_connectionString))
                {
                    lock (_lock)
                    {
                        if (String.IsNullOrEmpty(_connectionString))
                        {
                            ConnectionStringSettings nocConfigConnectionStringSetting =
                                ConfigurationManager.ConnectionStrings["NocDatabase"];
                            if ((null != nocConfigConnectionStringSetting) &&
                                !string.IsNullOrEmpty(nocConfigConnectionStringSetting.ConnectionString))
                                _connectionString = nocConfigConnectionStringSetting.ConnectionString;
                            else
                                throw new InvalidOperationException("Connection string can't be null or empty");
                        }
                    }
                }

                return _connectionString;
            }
        }
    }
}
