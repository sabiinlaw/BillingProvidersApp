
namespace BillingProvidersApp.Core
{
    /// <summary>
    /// Base class for all NOC business objects.
    /// </summary>
    public class NocBusinessObject : BusinessObject
    {
        int id = -1;


        /// <summary>
        /// Object identifier.
        /// </summary>
        [MapField("ID", true)]
        virtual public int Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// True if this is object not saved in database.
        /// </summary>
        virtual public bool IsNewObject
        {
            get { return Id <= 0; }
        }
    }
}
