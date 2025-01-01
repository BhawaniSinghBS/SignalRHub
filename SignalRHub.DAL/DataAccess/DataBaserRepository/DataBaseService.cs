using System.Data;

namespace SihnalRHub.DAL.DataAccess.DataBaserRepository
{
    public class DataBaseService : IDataBaseService
    {
        private readonly IDictionary<string, IDbConnection> _connection;

        public object Users => new ();

        public DataBaseService( IDictionary<string, IDbConnection> connection)
        {
            _connection = connection;
        }
    }
}
