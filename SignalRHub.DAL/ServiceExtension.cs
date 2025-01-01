using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SihnalRHub.DAL.DataAccess.DataBaserRepository;
using SihnalRHub.DAL.Repositories;
using System.Data;
using System.Data.SqlClient;

namespace SihnalRHub.DAL
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddDALServices(this IServiceCollection services, Dictionary<string, int> dbConnectionStringNames, IConfiguration configuration)
        {
            try
            {
                services.AddScoped<IDataBaseService>(provider =>
                  {
                      var specificDbConnections = new Dictionary<string, IDbConnection>();
                      foreach (var kvp in dbConnectionStringNames)
                      {
                          var specificDbConnection = new SqlConnection(configuration.GetConnectionString(kvp.Key));
                          specificDbConnections.Add(kvp.Key, specificDbConnection);
                      }
                      return new DataBaseService(specificDbConnections);
                  });

                services.AddScoped<IRepository, Repository>();
            }
            catch (Exception ex)
            {
                throw;
            }
            return services;
        }


        //public static TService GetRequiredService<TService>(this IServiceProvider serviceProvider)
        //{
        //    if (serviceProvider == null)
        //    {
        //        return default;
        //    }

        //    return serviceProvider.GetRequiredService<TService>();
        //}
    }
}
