using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Needlework.Net.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSingletonsFromAssemblies<T>(this ServiceCollection services)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsAbstract && typeof(T).IsAssignableFrom(p));

            foreach (var type in types) services.AddSingleton(typeof(T), type);

            return services;
        }
    }
}