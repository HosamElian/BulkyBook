﻿using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private ApplicationDbContext _db;

        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public int DecrementCount(ShoppingCart shoppingCart, int Count)
        {
            shoppingCart.Count -= Count;
            return shoppingCart.Count;
        }

        public int IncrementCount(ShoppingCart shoppingCart, int Count)
        {
            shoppingCart.Count += Count;
            return shoppingCart.Count;
        }
    }
}
