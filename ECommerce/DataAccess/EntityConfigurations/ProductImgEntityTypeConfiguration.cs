﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.DataAccess.EntityConfigurations
{
    public class ProductImgEntityTypeConfiguration : IEntityTypeConfiguration<ProductSubImage>
    {
        public void Configure(EntityTypeBuilder<ProductSubImage> builder)
        {
            builder.HasKey(e => new { e.ProductId, e.Img });
        }
    }
}
