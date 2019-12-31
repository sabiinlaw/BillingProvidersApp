using Microsoft.AspNetCore.Http;
using System;

namespace BillingProvidersApp.Core
{
    /// <summary>
    /// Audit Log entity.
    /// </summary>
    public class AuditLog : NocBusinessObject
    {
        public const int ServiceUserId = 0;

        public AuditLog(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }


        /// <summary>
        /// Business object can't be created without object manager.
        /// </summary>
        internal AuditLog() { }

        /// <summary>
        /// Audit Log entity date/time added.
        /// </summary>
        [MapField]
        virtual public DateTime DateTime
        {
            get;
            set;
        }

        /// <summary>
        /// Audit Log entity description.
        /// </summary>
        [MapField]
        virtual public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Audit Log entity description.
        /// </summary>
        [MapField]
        virtual public string Keywords
        {
            get;
            set;
        }

        /// <summary>
        /// Audit Log entity DNN user ID.
        /// </summary>
        [MapField("UserID")]
        virtual public int UserId
        {
            get;
            set;
        }

        #region Static properties

        /// <summary>
        /// <see cref="AuditLog" /> objects manager.
        /// </summary>
        static private IObjectManager AuditLogManager
        {
            get { return ManagerCache.GetManager(typeof(AuditLog)); }
        }

        /// <summary>
        /// <see cref="AuditLogDetails" /> objects manager.
        /// </summary>
        static private IObjectManager AuditLogDetailsManager
        {
            get { return ManagerCache.GetManager(typeof(AuditLogDetails)); }
        }

        public static IHttpContextAccessor _httpContextAccessor { get; private set; }

        #endregion Static properties


        #region Extended functionality methods

        /// <summary>
        /// Creates new <see cref="AuditLog" /> object and fills it with provided data.
        /// </summary>
        /// <param name="description">Audit Log entry description.</param>
        /// <param name="dateTime">Audit Log entry created date/time.</param>
        /// <param name="userId">Audit Log entry DNN user ID.</param>
        /// <returns>Newly created <see cref="AuditLog" /> object or null if object creation failed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="description" /> is a null reference.</exception>
        static public AuditLog AddLog(string description, DateTime dateTime, string keywords, int userId)
        {
            // Exceptions
            if (description == null)
            {
                throw new ArgumentNullException("description");
            }

            // Create and fill new AuditLog object
            AuditLog newAuditLog = (AuditLog)AuditLogManager.CreateObject();
            newAuditLog.Description = description;
            newAuditLog.DateTime = dateTime;
            newAuditLog.UserId = userId;
            newAuditLog.Keywords = keywords;

            // Return newly created object if it was saved in the DB
            return newAuditLog.Save() ? newAuditLog : null;
        }

        /// <summary>
        /// Creates new <see cref="AuditLog" /> object and fills it with provided data.
        /// <see cref="AuditLog.UserId" /> is filled automatically.
        /// </summary>
        /// <param name="description">Audit Log entry description.</param>
        /// <param name="dateTime">Audit Log entry created date/time.</param>
        /// <returns>Newly created <see cref="AuditLog" /> object or null if object creation failed.</returns>
        static public AuditLog AddLog(string description, DateTime dateTime, string keywords = null)
        {
            int userId = ServiceUserId;

            if (_httpContextAccessor.HttpContext != null && !String.IsNullOrEmpty(_httpContextAccessor.HttpContext.User.Identity.Name))
            {
                try
                {
                    MembershipUser membershipUser = new MembershipUser();
                    if (membershipUser != null)
                    {
                        //TODO get User
                        //User user = new UsersRepository().GetByEmail(membershipUser.Email);

                        //if (user != null)
                        //    userId = user.ID;
                    }
                }
                catch (Exception)
                {
                    // Ignore user getting error
                }
            }

            return AddLog(description, dateTime, keywords, userId);
        }

        /// <summary>
        /// Creates new <see cref="AuditLog" /> object and fills it with provided data.
        /// <see cref="AuditLog.UserId" /> is filled automatically.
        /// <see cref="AuditLog.DateTime" /> is filled with <c>DateTime.Now</c>.
        /// </summary>
        /// <param name="description">Audit Log entry description.</param>
        /// <returns>Newly created <see cref="AuditLog" /> object or null if object creation failed.</returns>
        static public AuditLog AddLog(string description, string keywords = null)
        {
            return AddLog(description, DateTime.Now, keywords);
        }

        /// <summary>
        /// Creates new <see cref="AuditLog" /> object and fills it with provided data (which can be formatted).
        /// <see cref="AuditLog.UserId" /> is filled automatically.
        /// <see cref="AuditLog.DateTime" /> is filled with <c>DateTime.Now</c>.
        /// </summary>
        /// <param name="description">Audit Log entry description template.</param>
        /// <param name="args"><paramref name="description" /> formatting arguments.</param>
        /// <returns>Newly created <see cref="AuditLog" /> object or null if object creation failed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="description" /> is a null reference.</exception>
        /// <example>
        /// <c>
        /// // Create new Audit Log entry<br />
        /// AuditLog logEntry = AuditLog.AddLogFormat("Moving object {0} to another instance.", "Main");<br />
        /// <br />
        /// // Operation #1 succeeded<br />
        /// logEntry.AddLogDetailsFormat(AuditType.Success, "Operation #{0} succeeded!", 1);<br />
        /// <br />
        /// // Operation #2 failed<br />
        /// logEntry.AddLogDetailsFormat(AuditType.Error, "Operation #{0} failed...", 2);<br />
        /// </c>
        /// </example>
        static public AuditLog AddLogFormat(string description, params object[] args)
        {
            if (string.IsNullOrEmpty(description))
                throw new ArgumentNullException("description");

            return AddLog(string.Format(description, args), DateTime.Now);
        }

        public static AuditLog AddLogFormatUnderServiceUser(string description, params object[] args)
        {
            if (string.IsNullOrEmpty(description))
                throw new ArgumentNullException("description");

            return AddLog(string.Format(description, args), DateTime.Now, null, ServiceUserId);
        }

        /// <summary>
        /// Creates new <see cref="AuditLogDetails" /> object and fills it with provided data (which can be formatted).
        /// </summary>
        /// <param name="auditLogId">Parent Audit Log entry ID.</param>
        /// <param name="auditType">Audit Type (<see cref="AuditLogType" /> constants can be used).</param>
        /// <param name="description">Audit Log Details description.</param>
        /// <param name="args"><paramref name="description" /> formatting arguments.</param>
        /// <returns>Newly created <see cref="AuditLogDetails" /> object or null if object creation failed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="description" /> is a null reference.</exception>
        static public AuditLogDetails AddLogDetailsFormat(int auditLogId, AuditLogType auditType, string description, params object[] args)
        {
            // Exceptions
            if (description == null)
            {
                throw new ArgumentNullException("description");
            }
            return AddLogDetails(auditLogId, auditType, string.Format(description, args));
        }

        /// <summary>
        /// Creates new <see cref="AuditLogDetails" /> object and fills it with provided data.
        /// </summary>
        /// <param name="auditLogId">Parent Audit Log entry ID.</param>
        /// <param name="auditType">Audit Type (<see cref="AuditLogType" /> constants can be used).</param>
        /// <param name="description">Audit Log Details description.</param>
        /// <returns>Newly created <see cref="AuditLogDetails" /> object or null if object creation failed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="description" /> is a null reference.</exception>
        static public AuditLogDetails AddLogDetails(int auditLogId, AuditLogType auditType, string description)
        {
            // Exceptions
            if (description == null)
            {
                throw new ArgumentNullException("description");
            }

            // Create and fill new AuditLog object
            AuditLogDetails newAuditLogDetails = (AuditLogDetails)AuditLogDetailsManager.CreateObject();
            newAuditLogDetails.AuditLogId = auditLogId;
            newAuditLogDetails.AuditLogType = auditType;
            newAuditLogDetails.Description = description;

            // Return newly created object if it was saved in the DB
            return newAuditLogDetails.Save() ? newAuditLogDetails : null;
        }

        /// <summary>
        /// Creates new <see cref="AuditLogDetails" /> object for this <see cref="AuditLog" /> object
        /// and fills it with provided data (which can be formatted).
        /// </summary>
        /// <param name="auditType">Audit Type (<see cref="AuditLogType" /> constants can be used).</param>
        /// <param name="description">Audit Log Details description.</param>
        /// <param name="args"><paramref name="description" /> formatting arguments.</param>
        /// <returns>Newly created <see cref="AuditLogDetails" /> object or null if object creation failed.</returns>
        virtual public AuditLogDetails AddLogDetailsFormat(AuditLogType auditType, string description, params object[] args)
        {
            if (IsNewObject) return null;
            return AddLogDetails(auditType, String.Format(description, args));
        }

        /// <summary>
        /// Creates new <see cref="AuditLogDetails" /> object for this <see cref="AuditLog" /> object and fills it with provided data.
        /// </summary>
        /// <param name="auditType">Audit Type (<see cref="AuditLogType" /> constants can be used).</param>
        /// <param name="description">Audit Log Details description.</param>
        /// <returns>Newly created <see cref="AuditLogDetails" /> object or null if object creation failed.</returns>
        virtual public AuditLogDetails AddLogDetails(AuditLogType auditType, string description)
        {
            if (IsNewObject) return null;
            return AddLogDetails(Id, auditType, description);
        }

        #endregion Extended functionality methods
    }

    internal class MembershipUser
    {
        public string Email { get; internal set; }
    }
}
