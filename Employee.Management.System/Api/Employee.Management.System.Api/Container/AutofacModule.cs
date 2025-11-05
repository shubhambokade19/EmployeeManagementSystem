using Autofac;
using Employee.Management.System.Common.Data;
using Microsoft.Extensions.Caching.Memory;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Employee.Management.System.Api.Container
{
    public class AutofacModule : Autofac.Module
    {
        private readonly IConfiguration config;
        private readonly IMemoryCache memoryCache;

        public AutofacModule(IConfiguration configuration)
        {
            config = configuration;
            memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(config).As<IConfiguration>();
            builder.RegisterType<ConnectionStrings>().SingleInstance().UsingConstructor(typeof(IConfiguration));
            builder.RegisterType<MemoryCache>().As<IMemoryCache>().SingleInstance();
            builder.RegisterType<SwaggerGenerator>().As<ISwaggerProvider>().SingleInstance();

            // Registering controllers for ASP.NET Core
            builder.RegisterAssemblyTypes(
                Assembly.GetExecutingAssembly(),
                Assembly.Load("Employee.Management.System.Common.Api"),
                Assembly.Load("Employee.Management.System.Common.Core"),
                Assembly.Load("Employee.Management.System.Common.Data"),
                Assembly.Load("Employee.Management.System.Common.Email"),
                Assembly.Load("Employee.Management.System.Common.Logging"),
                Assembly.Load("Employee.Management.System.Domain.Core"),
                Assembly.Load("Employee.Management.System.Domain.Infrastructure")
            ).Where(t => t.IsClass)
            .AsImplementedInterfaces();
        }
    }
}
