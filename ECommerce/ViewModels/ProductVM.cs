﻿namespace ECommerce.ViewModels
{
    public class ProductVM
    {
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Brand> Brands { get; set; }
        public Product? Product { get; set; }
    }
}
