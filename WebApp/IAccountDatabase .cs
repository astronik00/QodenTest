using System.Threading.Tasks;

namespace WebApp
{
    public interface IAccountDatabase
    {
        Task<Account> GetOrCreateAccountAsync(string id);

        Task<Account> GetOrCreateAccountAsync(long id);

        Task<Account> FindByUserNameAsync(string userName);

        void UpdateCounter(string id);

        void UpdateCounter(long id);

        Task ResetAsync();
    }
}