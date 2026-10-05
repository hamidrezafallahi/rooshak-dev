using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Repositories
{
    public class ContactRequestRepository : Repository<ContactRequest>, IContactRequestRepository
    {
        public ContactRequestRepository(AppDbContext context) : base(context)
        {
        }
    }
}
