using OnlineShop.Domain.Entities;
namespace OnlineShop.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
        Task<bool> ExistsBySlugAsync(string slug);
        Task<UserModel?> GetUserWithDetailsAsync(int userId);
 
    }
}
