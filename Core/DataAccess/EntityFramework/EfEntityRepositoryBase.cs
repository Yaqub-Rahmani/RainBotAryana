using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.DataAccess.EntityFramework
{
    public class EfEntityRepositoryBase<TEntity, TContext> : IEntityRepository<TEntity>
  where TEntity : class, IEntity, new()
  where TContext : DbContext
    {

        protected readonly TContext _context;
        public EfEntityRepositoryBase(TContext context) { _context = context; }

        public void Add(TEntity entity) { _context.Set<TEntity>().Add(entity); _context.SaveChanges(); }
        public void Delete(TEntity entity) { _context.Set<TEntity>().Remove(entity); _context.SaveChanges(); }
        public TEntity Get(Expression<Func<TEntity, bool>> filter)
          => _context.Set<TEntity>().FirstOrDefault(filter);
        public IList<TEntity> GetList(Expression<Func<TEntity, bool>> filter = null)
          => filter == null ? _context.Set<TEntity>().ToList() : _context.Set<TEntity>().Where(filter).ToList();
        public void Update(TEntity entity) { _context.Set<TEntity>().Update(entity); _context.SaveChanges(); }

        public IQueryable<TEntity> Query() => _context.Set<TEntity>().AsQueryable();

        // İsteğe bağlı: NoTracking sorgular (liste/detay için performans)
        public IQueryable<TEntity> QueryNoTracking() => _context.Set<TEntity>().AsNoTracking();

    }

    //public class EfEntityRepositoryBase<TEntity, TContext> : Core.DataAccess.IEntityRepository<TEntity>
    //        where TEntity : class, IEntity, new()
    //        where TContext : DbContext, new()
    //{
    //    public void Add(TEntity entity)
    //    {
    //        using var context = new TContext();
    //        var entry = context.Entry(entity);
    //        entry.State = EntityState.Added;
    //        context.SaveChanges();
    //    }

    //    public void Delete(TEntity entity)
    //    {
    //        using var context = new TContext();
    //        var entry = context.Entry(entity);
    //        entry.State = EntityState.Deleted;
    //        context.SaveChanges();
    //    }

    //    public TEntity Get(Expression<Func<TEntity, bool>> filter)
    //    {
    //        using var context = new TContext();
    //        return context.Set<TEntity>().SingleOrDefault(filter);
    //    }

    //    public List<TEntity> GetAll(Expression<Func<TEntity, bool>> filter = null)
    //    {
    //        using var context = new TContext();
    //        return filter == null
    //            ? context.Set<TEntity>().ToList()
    //            : context.Set<TEntity>().Where(filter).ToList();
    //    }


    //    public void Update(TEntity entity)
    //    {
    //        using var context = new TContext();
    //        var entry = context.Entry(entity);
    //        entry.State = EntityState.Modified;
    //        context.SaveChanges();
    //    }
    //}
}
