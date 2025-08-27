using VideoModels;

namespace VideoBackend.Interface
{
    public interface IUserOperation
    {
        Task<User> GetCurrentAccountAsync(string accessToken);
    }
}
