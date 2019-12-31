using BillingProvidersApp.DAL.Base;
using BillingProvidersApp.Helper.DependencyResolver;

namespace BillingProvidersApp.DAL
{
    public static class EntitiesSingleton
    {
        private static object _lock = new object();

        public static IEntities GetEntities()
        {
            //TODO implement HttpContext.Current
            //if (HttpContext.Current != null)
            //{
            //    IEntities entities = HttpContext.Current.Items["_EntityContext"] as IEntities;
            //    if (entities == null)
            //    {
            //        lock (_lock)
            //        {
            //            entities = HttpContext.Current.Items["_EntityContext"] as IEntities;
            //            if (entities == null)
            //            {
            //                entities = DependencyResolver.Get<IEntities>();
            //                HttpContext.Current.Items["_EntityContext"] = entities;
            //            }
            //        }
            //    }

            //    return entities;
            //}
            //else
            //{
                return DependencyResolver.Get<IEntities>();
            //}
        }
    }
}
