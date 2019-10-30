using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace SuperRocket.ConsoleCore
{
    [DependsOn(
        typeof(AbpAutofacModule)
        )]
    public class AppModule : AbpModule
    {

    }
}
