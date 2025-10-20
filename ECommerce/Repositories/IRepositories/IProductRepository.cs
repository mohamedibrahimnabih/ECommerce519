﻿using Microsoft.EntityFrameworkCore;

namespace ECommerce.Repositories.IRepositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task AddRangeAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default);
    }
}
