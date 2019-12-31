using System;

namespace BillingProvidersApp.Core
{
    /// <summary>
    /// Base class for all application's business objects.
    /// </summary>
    [ErrorMessage(ObjectAction.LoadObject, Strings.BusinessObject.LoadObjectError)]
    [ErrorMessage(ObjectAction.LoadObjects, Strings.BusinessObject.LoadObjectsError)]
    [ErrorMessage(ObjectAction.SaveObject, Strings.BusinessObject.SaveObjectError)]
    [ErrorMessage(ObjectAction.DeleteObject, Strings.BusinessObject.DeleteObjectError)]
    abstract public class BusinessObject
    {
        /// <summary>
		/// Manager to use while interoperating with DB.
		/// </summary>
		protected internal IObjectManager manager;


        /// <summary>
        /// Saves this business object into database.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <returns>True if save operation succeeded.</returns>
        virtual public bool Save(CommunicationMethod method)
        {
            return manager.InternalSaveObject(this, method);
        }

        /// <summary>
        /// Saves this business object into database.
        /// </summary>
        /// <returns>True if save operation succeeded.</returns>
        virtual public bool Save()
        {
            return manager.InternalSaveObject(this, CommunicationMethod.Default);
        }

        /// <summary>
        /// Deletes this business object from database.
        /// </summary>
        /// <returns>True if delete operation succeeded.</returns>
        virtual public bool Delete()
        {
            return manager.InternalDeleteObject(this, CommunicationMethod.Default);
        }

        /// <summary>
        /// Deletes this business object from database.
        /// </summary>
        /// <param name="method">Method of loading of the data from a storage</param>
        /// <returns>True if delete operation succeeded.</returns>
        virtual public bool Delete(CommunicationMethod method)
        {
            return manager.InternalDeleteObject(this, method);
        }

        /// <summary>
        /// Handle Business Exception.
        /// </summary>
        virtual public Exception HandleException(Exception ex)
        {
            return ex;
        }
    }
}
