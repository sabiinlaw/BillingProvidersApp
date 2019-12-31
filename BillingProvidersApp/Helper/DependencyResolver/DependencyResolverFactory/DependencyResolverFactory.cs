namespace BillingProvidersApp.Helper.DependencyResolver.DependencyResolverFactory
{
    public interface IDependencyResolver
    {
        TAbstaction Get<TAbstaction>() where TAbstaction : class;
    }
}
