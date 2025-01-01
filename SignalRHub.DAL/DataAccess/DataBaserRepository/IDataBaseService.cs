using System.Data;

namespace SihnalRHub.DAL.DataAccess.DataBaserRepository
{
    public interface IDataBaseService
    {
        object Users { get; }
        //Task<object> GetUsersAsync();
    }
}
