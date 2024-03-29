﻿using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            var productFromDb = _db?.Products?.FirstOrDefault(u=> u.Id == product.Id);
            if(productFromDb != null)
            {
                productFromDb.Title = product.Title;
                productFromDb.ISBN = product.ISBN;
                productFromDb.Description = product.Description;
                productFromDb.ListPrice = product.ListPrice;
                productFromDb.Price = product.Price;
                productFromDb.Price50 = product.Price50;
                productFromDb.Price100 = product.Price100;
                productFromDb.CategoryId = product.CategoryId; 
                productFromDb.CoverTypeId = product.CoverTypeId; 
                productFromDb.Author = product.Author;
                if (product.ImageUrl != null)
                {
                    productFromDb.ImageUrl = product.ImageUrl; 
                }
            }
        }
    }
}
