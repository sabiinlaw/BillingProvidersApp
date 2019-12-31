using BillingProvidersApp.Tools;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Caching;

namespace BillingProvidersApp.Core
{
    #region enum SettingsBagType

    /// <summary>
    /// Possible type of <see cref="SettingsBag"/>.
    /// Used in <see cref="SettingsBag"/> constructor.
    /// </summary>
    public enum SettingsBagType
    {
        /// <summary>
        /// Global NOC settings.
        /// </summary>
        Global = 0,
        /// <summary>
        /// Default settings for certain NOC version.
        /// </summary>
        VersionDefault = 1,
        /// <summary>
        /// Farm-specific NOC settings.
        /// </summary>
        Farm = 2,
        /// <summary>
        /// NetSuite related NOC settings.
        /// </summary>
        NetSuite = 3,
        /// <summary>
        /// Elasticsearch related NOC settings.
        /// </summary>
        Elasticsearch = 4
    }

    #endregion enum SettingsBagType


    /// <summary>
    /// Class used for get/set NOC settings.
    /// </summary>
    [ErrorMessage(ObjectAction.LoadObject, Strings.SettingsBag.RestoreFromDbError)]
    [ErrorMessage(ObjectAction.LoadObjects, Strings.SettingsBag.RestoreFromDbError)]
    [ErrorMessage(ObjectAction.SaveObject, Strings.SettingsBag.StoreToDbError)]
    [ErrorMessage(ObjectAction.DeleteObject, Strings.SettingsBag.StoreToDbError)]
    public class SettingsBag
    {
        protected SettingsBagType type;      // settings type
        protected int objectId;  // ID of an object which hostes settings

        protected static string[] errorMessages;  // Error messages for ManagerCache.ObjectException
        protected static SettingsBag globalSettings;

        /// <summary>
        /// Creates new <see cref="SettingsBag"/> object (for Global settings only).
        /// </summary>
        /// <param name="type"><see cref="SettingsBag"/> type.</param>
        /// <exception cref="ArgumentException"><paramref name="type" /> not equals to <see cref="SettingsBagType"/>.Global.</exception>
        public SettingsBag(SettingsBagType type)
            : this(type, 0)
        {
            // Exceptions
            if (type != SettingsBagType.Global && type != SettingsBagType.NetSuite && type != SettingsBagType.Elasticsearch)
            {
                throw new ArgumentException("Only SettingsBagType.Global or SettingsBagType.NetSuite or SettingsBagType.Elasticsearch type can be used without providing objectId.", "type");
            }
        }

        /// <summary>
        /// Creates new <see cref="SettingsBag"/> object.
        /// </summary>
        /// <param name="type"><see cref="SettingsBag"/> type.</param>
        /// <param name="objectId">ID of an object which hostes settings.</param>
        public SettingsBag(SettingsBagType type, int objectId)
        {
            this.type = type;
            this.objectId = objectId;
        }

        /// <summary>
        /// Global NOC settings.
        /// </summary>
        static public SettingsBag GlobalSettings
        {
            get
            {
                if (globalSettings == null)
                {
                    globalSettings = new SettingsBag(SettingsBagType.Global);
                }
                return globalSettings;
            }
        }

        ObjectCache cache = MemoryCache.Default;

        /// <summary>
		/// Gets/sets setting with given <paramref name="name" /> from NOC database.
		/// Returns empty string if no setting was found.
		/// </summary>
		/// <param name="name">Setting name.</param>
		/// <exception cref="ArgumentNullException"><paramref name="name" /> is a null reference.</exception>
		public virtual string this[string name]
        {
            get
            {
                // Exceptions
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }

                if (cache.Contains(name))
                    return (string)cache[name];

                string action = (type == SettingsBagType.NetSuite)
                    ? "getns"
                    : (type == SettingsBagType.Elasticsearch) ? "getel" : "get";

                // Get setting from DB
                DataTable settingsTable = ExecuteDataTable(action, name, null, ObjectAction.LoadObject);
                string value = settingsTable.Rows.Count > 0 ? (string)settingsTable.Rows[0]["Value"] : string.Empty;

                cache.Add(name, value, DateTimeOffset.Now.AddDays(1));
                return value;
            }
            set
            {
                // Exceptions
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }

                string action = (type == SettingsBagType.NetSuite) ? "setns" : (type == SettingsBagType.Elasticsearch) ? "setel" : "set";

                // Set setting in DB
                ExecuteDataTable(action, name, value, value != null ? ObjectAction.SaveObject : ObjectAction.DeleteObject);

                cache.Remove(name);
            }
        }

        #region Protected methods

        /// <summary>
        /// Returns <see cref="SqlCommand" /> for work with settings.
        /// </summary>
        /// <param name="action">Setting action (get, set, all).</param>
        /// <param name="name">Setting name.</param>
        /// <param name="value">Setting value.</param>
        /// <returns>New <see cref="SqlCommand" />.</returns>
        protected SqlCommand GetSettingsCommand(string action, string name, string value)
        {
            SqlCommand settingsCommand = SqlHelper.GetStoredProcedureCommand("spnocSettings");
            if (type != SettingsBagType.Global)
            {
                settingsCommand.Parameters.AddWithValue(
                  type == SettingsBagType.VersionDefault ? "@VersionID" : "@FarmID",
                  objectId);
            }
            settingsCommand.Parameters.AddWithValue("@Action", action);
            settingsCommand.Parameters.AddWithValue("@Name", name);
            settingsCommand.Parameters.AddWithValue("@Value", value);
            return settingsCommand;
        }

        /// <summary>
        /// Executes <see cref="SqlHelper.ExecuteDataTable" />.
        /// Fires <see cref="ManagerCache.ObjectException" /> event on throwed exception.
        /// </summary>
        /// <param name="command"><see cref="SqlCommand" /> to execute.</param>
        /// <param name="action">Action performed on business object.</param>
        /// <returns>Query results as <see cref="DataTable" />.</returns>
        protected DataTable ExecuteDataTable(SqlCommand command, ObjectAction action)
        {
            try
            {
                return SqlHelper.ExecuteDataTable(command);
            }
            catch (Exception ex)
            {
                var errorMessage = (errorMessages == null || errorMessages.Length <= (int)action) ? ex.Message : errorMessages[(int)action];
                ManagerCache.HandleObjectException(typeof(SettingsBag), action, errorMessage, ex);
                return new DataTable();
            }
        }

        /// <summary>
        /// Executes <see cref="SqlHelper.ExecuteDataTable" />.
        /// Fires <see cref="ManagerCache.ObjectException" /> event on throwed exception.
        /// </summary>
        /// <param name="action">Setting action (get, set, all).</param>
        /// <param name="name">Setting name.</param>
        /// <param name="value">Setting value.</param>
        /// <param name="objectAction">Action performed on business object.</param>
        /// <returns>Query results as <see cref="DataTable" />.</returns>
        protected DataTable ExecuteDataTable(string action, string name, string value, ObjectAction objectAction)
        {
            using (SqlCommand command = GetSettingsCommand(action, name, value))
            {
                return ExecuteDataTable(command, objectAction);
            }
        }

        #endregion
    }
}
