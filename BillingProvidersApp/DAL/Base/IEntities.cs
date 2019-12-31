using System.Data.Entity;

namespace BillingProvidersApp.DAL.Base
{
    public interface IEntities
    {
        IDbSet<nocAccount> nocAccounts { get; set; }
        IDbSet<ypMember> ypMembers { get; set; }
        int SaveChanges();
    }

    //TODO implement ypMember, nocAccount, mbTier
    public class ypMember
    {
        public int ID { get; set; }
        public int AccountID { get; set; }
    }


    public class nocAccount
    {

        public int ID { get; set; }
        public int? nsCustomerID { get; internal set; }
        public string Name { get; set; }
    }

   

    public class mbTier
    {
        public int NotBillableSmartZonesCount { get; internal set; }
    }
}