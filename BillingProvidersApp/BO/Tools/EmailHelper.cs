using BillingProvidersApp.Core;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Net.Mail;
using AuditLogType = BillingProvidersApp.Core.AuditLogType;

namespace BillingProvidersApp.BO.Tools
{
    public static class EmailHelper
    {
        public static bool SendMail(MailMessage message, MailPriority priority, int auditLogId = 0)
        {
            try
            {
                SmtpClient client = new SmtpClient();
                string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];

                if (!String.IsNullOrEmpty(smtpHost))
                    client.Host = smtpHost;
                else
                    client.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis;

                message.Priority = priority;

                if (message.From == null)
                    message.From = new MailAddress(ConfigurationManager.AppSettings["FromEmailAddress"]);

                if (message.From == null)
                    throw new InvalidOperationException("Mail can't be without sender");

                client.Send(message);
            }
            catch (Exception ex)
            {
                try
                {
                    EventLog myLog = new EventLog();
                    string traceSource = "logging-event-source-name";
                    if (String.IsNullOrEmpty(traceSource))
                        traceSource = "NOC";

                    myLog.Source = traceSource;
                    myLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
                }
                catch
                {
                }

                if (auditLogId <= 0)
                    auditLogId = AuditLog.AddLog("SendMail exception").Id;

                IObjectManager manager = (ManagerCache.GetManager(typeof(AuditLog)));
                AuditLog log = (AuditLog)manager.GetObject(auditLogId);
                string messageLog = "";
                messageLog += "Expection: " + ex.ToString();
                if (ex.InnerException != null)
                {
                    messageLog += "\r\n<br/>Inner Exception: " + ex.InnerException.ToString();
                }
                messageLog += "\r\n<br/>Mail To: " + message.To.ToString();
                messageLog += "\r\n<br/>Mail Body: " + message.Body;
                log.AddLogDetails(AuditLogType.Error, messageLog);
            }

            return true;
        }
        public static bool SendMail(string body, string subject, string toAddress, MailPriority priority, bool isPublicEmail, int auditLogId = 0)
        {
            MailMessage message = new MailMessage();

            message.Body = body;
            message.IsBodyHtml = false;
            message.Subject = subject;
            if (!isPublicEmail)
            {
                string nocLabel = BillingProvidersApp.Core.Helper.GetNocLabel();
                if (!String.IsNullOrEmpty(nocLabel))
                {
                    message.Subject = $"{message.Subject} [{nocLabel}]";
                }
            }

            MailAddress toMailAddress = new MailAddress(toAddress);
            message.To.Add(toMailAddress);

            MailAddress fromMailAddress = new MailAddress("NOC@corrigo.com", "NOC Application");
            message.From = fromMailAddress;

            return SendMail(message, priority, auditLogId);
        }
    }
}
