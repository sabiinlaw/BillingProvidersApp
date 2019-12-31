using BillingProvidersApp.Core;
using BillingProvidersApp.Helper;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace BillingProvidersApp.Tools
{
    /// <summary>
    /// Helping methods for work with NOC database.
    /// </summary>
    sealed public class SqlHelper
    {
        const string DatabaseRegexPattern = @"Database\s*=";
        const string InitialCatalogRegexPattern = @"Initial Catalog\s*=";
        const string ConnectionStringSeparator = ";";

        public const string AspStateDatabaseName = "ASPState";
        public const string AspStateScriptFileName = "InstallSqlState.sql";


        static string nocConnectionString = string.Empty;
        static string NocApplicationName = string.Format("NOC {0}", typeof(SqlHelper).Assembly.GetName().Version);
        static bool isMsDtcEnabled;

        // This class is static.
        private SqlHelper() { }

        /// <summary>
        /// Static constructor
        /// </summary>
        static SqlHelper()
        {
            isMsDtcEnabled = false;
            try
            {
                string enableMsDtcString = ConfigurationManager.AppSettings["EnableMsDtc"];
                if (enableMsDtcString != null && enableMsDtcString.Trim().Length > 0)
                    isMsDtcEnabled = Convert.ToBoolean(enableMsDtcString);
            }
            catch { }
        }

        /// <summary>
        /// Indicates if MsDtc transactions enabled
        /// </summary>
        static public bool IsMsDtcEnabled
        {
            get { return isMsDtcEnabled; }
        }

        /// <summary>
        /// Connection string for NOC DB.
        /// </summary>
        static public string NocConnectionString
        {
            get { return SQLHelper.ConnectionString; }
        }

        /// <summary>
        /// Returns filled DataTable by given SqlCommand.
        /// Uses <see cref="HttpContext.Current" /> for thread-safe work.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <returns>Resulting DataTable.</returns>
        static public DataTable ExecuteDataTable(SqlCommand command, CommunicationMethod method)
        {
            bool useMSDTC = (method == CommunicationMethod.UseMSDTC) || ((method == CommunicationMethod.Default) && isMsDtcEnabled);

            return useMSDTC ? null : ExecuteDataTableWithoutMsDtc(command);
        }

        /// <summary>
        /// Returns filled DataTable by given SqlCommand.
        /// Uses <see cref="HttpContext.Current" /> for thread-safe work.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <returns>Resulting DataTable.</returns>
        static public DataTable ExecuteDataTable(SqlCommand command)
        {
            return ExecuteDataTable(command, CommunicationMethod.Default);
        }

        /// <summary>
        /// Extracts database name from database connection string
        /// </summary>
        /// <param name="connectionString">connectin string</param>
        /// <returns></returns>
        static public string GetDatabaseFromConnectionString(string connectionString)
        {
            string lowerCaseConnStr = connectionString.ToLower();

            Match match = new Regex(DatabaseRegexPattern, RegexOptions.IgnoreCase).Match(connectionString);

            return match.Success ?
                GetStringPartWithRegexMatch(
                    connectionString,
                    match,
                    ConnectionStringSeparator) :
                GetStringPartWithRegexMatch(
                    connectionString,
                    new Regex(InitialCatalogRegexPattern, RegexOptions.IgnoreCase).Match(connectionString),
                    ConnectionStringSeparator);
        }

        #region Methods for creating commands

        /// <summary>
        /// Creates SQL command for given stored procedure execution.
        /// </summary>
        /// <param name="storedProcedureName">Stored procedure name.</param>
        /// <returns>Instance of an SQL command of type stored procedure.</returns>
        static public SqlCommand GetStoredProcedureCommand(string storedProcedureName)
        {
            SqlCommand cmd = new SqlCommand(storedProcedureName);
            cmd.CommandType = CommandType.StoredProcedure;
            return cmd;
        }

        /// <summary>
        /// Generates SqlCommand for business object.
        /// </summary>
        /// <param name="itemTypeName">Business object type name.</param>
        /// <param name="action">DB action. Can be: 'item', 'items', 'save'.</param>
        /// <returns></returns>
        public static SqlCommand GetBusinessObjectCommand(string itemTypeName, string action)
        {
            return GetBusinessObjectCommand(itemTypeName, action, "spnocBO{0}");
        }

        public static SqlCommand GetBusinessObjectCommand(string itemTypeName, string action, string commandTextTemplate)
        {
            SqlCommand command = GetStoredProcedureCommand(string.Format(commandTextTemplate, itemTypeName));
            command.Parameters.AddWithValue("@Action", action);
            return command;
        }

        #endregion

        static private int ExecuteNonQueryWithoutMsDtc(SqlCommand command, int commandTimeout)
        {
            using (SqlConnection connection = new SqlConnection(NocConnectionString))
            {
                connection.Open();
                command.Connection = connection;
                command.CommandTimeout = commandTimeout;

                return command.ExecuteNonQuery();
            }
        }

        private static DataTable ExecuteDataTableWithoutMsDtc(SqlCommand command, int commandTimeout)
        {
            using (SqlConnection connection = new SqlConnection(NocConnectionString))
            {
                connection.Open();
                command.Connection = connection;

                if (-1 != commandTimeout)
                    command.CommandTimeout = commandTimeout;

                DataTable result = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(result);
                return result;
            }
        }

        private static DataTable ExecuteDataTableWithoutMsDtc(SqlCommand command)
        {
            return ExecuteDataTableWithoutMsDtc(command, -1);
        }

        private static string NormalizeConnectionString(string connectionString)
        {
            if (connectionString == null)
            {
                return connectionString;
            }
            string normalServer = String.Format("Server={0}", Environment.MachineName);
            return connectionString.Replace("Server=(local)", normalServer)
                .Replace("Server=localhost", normalServer)
                .Replace("Server=127.0.0.1", normalServer)
                .Replace("Server=.", normalServer);
        }

        private static string GetStringPartWithRegexMatch(string targetString, Match match, string endSymbol)
        {
            if (!match.Success)
                return null;

            int index = targetString.IndexOf(endSymbol, match.Index + match.Length);

            return (index > -1) ?
                targetString.Substring(match.Index + match.Length, index - match.Index - match.Length) :
                targetString.Substring(match.Index + match.Length).Trim();
        }
    }
}
