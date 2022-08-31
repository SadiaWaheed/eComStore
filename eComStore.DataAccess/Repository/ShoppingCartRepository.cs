﻿using eComStore.Model;
using eComStore.DataAccess.Repository.IRepository;
using System.Linq.Expressions;
using eComStore.DataAccess.Data;

namespace eComStore.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }
    }
}
