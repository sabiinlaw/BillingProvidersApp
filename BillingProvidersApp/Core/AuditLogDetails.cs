namespace BillingProvidersApp.Core
{
    /// <summary>
    /// Audit Log Details entity.
    /// </summary>
    public class AuditLogDetails : NocBusinessObject
    {
        int auditLogId;
        int auditTypeId;
        string description;

        /// <summary>
        /// Audit Log entity parent ID.
        /// </summary>
        [MapField("AuditLogID"), MapReference(typeof(AuditLog))]
        virtual public int AuditLogId
        {
            get { return auditLogId; }
            set { auditLogId = value; }
        }

        /// <summary>
        /// Audit Log entity type.
        /// </summary>
        [MapField("AuditTypeID")]
        virtual public int AuditTypeId
        {
            get { return auditTypeId; }
            protected set { auditTypeId = value; }
        }

        virtual public AuditLogType AuditLogType
        {
            get { return (AuditLogType)AuditTypeId; }
            set { AuditTypeId = (int)value; }
        }

        /// <summary>
        /// Audit Log Details entity description.
        /// </summary>
        [MapField]
        virtual public string Description
        {
            get { return description; }
            set { description = value; }
        }
    }
    /// <summary>
    /// nocAuditLogDetails record type.
    /// </summary>
    public enum AuditLogType
    {
        /// <summary>
        /// Error log record.
        /// </summary>
        Error = 1,
        /// <summary>
        /// Information log record.
        /// </summary>
        Information = 4,
        /// <summary>
        /// Success log record.
        /// </summary>
        Success = 2,
        /// <summary>
        /// Warning log record.
        /// </summary>
        Warning = 3
    }
}
