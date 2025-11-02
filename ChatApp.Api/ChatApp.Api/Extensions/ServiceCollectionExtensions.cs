using System.Reflection;

namespace ChatApp.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServicesByConvention(this IServiceCollection services, Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service"));

            foreach (var type in types)
            {
                var interfaceType = type.GetInterfaces()
                    .FirstOrDefault(i => i.Name == $"I{type.Name}");

                if (interfaceType == null)
                    continue;

                // Attribute varsa al, yoksa Scoped olarak varsay
                var lifetimeAttr = type.GetCustomAttribute<ServiceLifetimeAttribute>();
                var lifetime = lifetimeAttr?.Lifetime ?? ServiceLifetime.Scoped;

                switch (lifetime)
                {
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(interfaceType, type);
                        break;
                    case ServiceLifetime.Transient:
                        services.AddTransient(interfaceType, type);
                        break;
                    default:
                        services.AddScoped(interfaceType, type);
                        break;
                }
            }

            return services;
        }
    }
}
