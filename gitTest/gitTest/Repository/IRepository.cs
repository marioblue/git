using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace gitTest.Repository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        bool AddEntity(TEntity entity);
        bool UpdateEntity(TEntity entity);
        bool UpdateEntity(IEnumerable<TEntity> entities);
        bool DeleteEntity(int ID);
        bool DeleteEntity(TEntity entity);
        bool DeleteEntity(Expression<Func<TEntity, bool>> predicate);
        bool DeleteEntity(IEnumerable<TEntity> entities);
        IList<TEntity> LoadEntities(Func<TEntity, bool> whereLambda);
        IList<TEntity> LoadEntities(int pageIndex = 1, int pageSize = 30, Func<TEntity, bool> whereLambda = null);
        TEntity FindByID(int ID);

        IQueryable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy, int index, int size, out int total, string includeProperties = "");
    }
}
