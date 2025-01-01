using SihnalRHub.DAL.DataAccess.DataBaserRepository;

namespace SihnalRHub.DAL.Repositories
{
    public interface IRepository
    {
        Task<List<object>> GetAllDBRules();
    }
    public class Repository : IRepository
    {
      

        private readonly IDataBaseService _repository;
        public Repository(IDataBaseService dataBaseService)
        {
            _repository = dataBaseService;
        }

        public async Task<object> GetAllData()
        {
            var result =  _repository.Users ;
            return result ;
        }

        public Task<List<object>> GetAllDBRules()
        {
            return Task.FromResult(new List<object>());
        }
    }
}
