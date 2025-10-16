using System.Threading.Tasks;

namespace ECommerce.Repositories
{
    public class ProductRepository : Repository<Product> 
    {
        private ApplicationDbContext _context = new();

        public async Task AddRangeAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
        {
            await _context.AddRangeAsync(products, cancellationToken);
        }
    }
}
