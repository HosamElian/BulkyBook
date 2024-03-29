﻿using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }
        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? indcludeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null) { query = query.Where(filter); }
            if(indcludeProperties != null)
            {
                foreach (var includeProp in indcludeProperties.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                     query = query.Include(includeProp);
                }
            }
            return query.ToList();
        }

        public T GetFirstOrDefualt(Expression<Func<T, bool>> filter, string? indcludeProperties = null, bool tracked = true)
        {
            IQueryable<T> query;
            if (tracked)
            {
                query = dbSet;
            }
            else
            {
                query = dbSet.AsNoTracking();

            }
            query = query.Where(filter);
            if (indcludeProperties != null)
            {
                foreach (var includeProp in indcludeProperties.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }
    }
}
