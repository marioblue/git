using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace gitTest.Repository
{
    /// <summary>
    /// EF5.0的写法 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EFRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        #region 单例模式
        public static EFRepository<TEntity> Instance = new EFRepository<TEntity>();
        private EFRepository()
        {
            Create();
        }
        #endregion

        /// <summary>
        /// 获取 当前使用的数据访问上下文对象
        /// </summary>
        public DbContext Context
        {
            get
            {
                //return your own dbcontext
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool AddEntity(TEntity entity)
        {
            EntityState state = Context.Entry(entity).State;
            if (state == EntityState.Detached)
            {
                Context.Entry(entity).State = EntityState.Added;
            }
            Context.SaveChanges();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool UpdateEntity(TEntity entity)
        {
            Context.Set<TEntity>().Attach(entity);
            Context.Entry<TEntity>(entity).State = EntityState.Modified;
            return Context.SaveChanges() > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public bool UpdateEntity(IEnumerable<TEntity> entities)
        {
            try
            {
                Context.Configuration.AutoDetectChangesEnabled = false;
                foreach (TEntity entity in entities)
                {
                    UpdateEntity(entity);
                }
                return true;
            }
            finally
            {
                Context.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool DeleteEntity(int ID)
        {
            TEntity entity = FindByID(ID);
            return DeleteEntity(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool DeleteEntity(TEntity entity)
        {
            Context.Set<TEntity>().Attach(entity);
            Context.Entry<TEntity>(entity).State = EntityState.Deleted;
            return Context.SaveChanges() > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public bool DeleteEntity(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            List<TEntity> entities = Set().Where(predicate).ToList();
            return Context.SaveChanges() > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public bool DeleteEntity(IEnumerable<TEntity> entities)
        {
            try
            {
                Context.Configuration.AutoDetectChangesEnabled = false;
                foreach (TEntity entity in entities)
                {
                    DeleteEntity(entity);
                }
                return true;
            }
            finally
            {
                Context.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public IList<TEntity> LoadEntities(Func<TEntity, bool> whereLambda)
        {
            if (whereLambda != null)
                return Context.Set<TEntity>().Where<TEntity>(whereLambda).AsQueryable().ToList();
            else
                return Context.Set<TEntity>().AsQueryable().ToList();
        }

        /// <summary>
        /// 不带排序的分页
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public IList<TEntity> LoadEntities(int pageIndex = 1, int pageSize = 30, Func<TEntity, bool> whereLambda = null)
        {
            int skinCount = (pageIndex - 1) * pageSize;
            if (whereLambda != null)
                return Set()
                    .Where<TEntity>(whereLambda)
                    .Skip(skinCount)
                    .Take(pageSize)
                    .ToList();
            else
                return Set()
                .Skip(skinCount)
                .Take(pageSize)
                .ToList();
        }

        /// <summary>
        /// 测试带排序的分页
        /// 这个方法貌似是把数据一次全部抓取出来再筛选排序
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="whereLambda"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public IList<TEntity> LoadEntities(int pageIndex = 1, int pageSize = 30, Func<TEntity, bool> whereLambda = null, Func<TEntity, int> sort = null)
        {
            int skinCount = (pageIndex - 1) * pageSize;
            if (whereLambda != null)
                return Set()
                    .Where<TEntity>(whereLambda)
                    .OrderByDescending(sort)
                    .Skip(skinCount)
                    .Take(pageSize)
                    .ToList();
            else
                return Set()
                .OrderByDescending(sort)
                .Skip(skinCount)
                .Take(pageSize)
                .ToList();
        }


        /// <summary>
        /// 简化后的分页方法
        /// 针对单表的分页
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <param name="total"></param>
        /// <param name="includeProperties">请传个空值</param>
        /// <returns></returns>
        public IQueryable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy, int index, int size, out int total, string includeProperties = "")
        {
            return GetPaged(out total, filter, orderBy, includeProperties, index, size);
        }
        /// <summary>
        /// 完整的分页方法
        /// </summary>
        /// <param name="total"></param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="includeProperties"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public IQueryable<TEntity> GetPaged(out int total, Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>,
          IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "", int index = 0, int size = 50)
        {
            int skipCount = index * size - size;
            var _reset = Get(filter, orderBy, includeProperties);
            total = _reset.Count();
            //total = Context.Set<TEntity>().Count(filter);
            _reset = skipCount == 0 ? _reset.Take(size) : _reset.Skip(skipCount).Take(size);
            return _reset.AsQueryable();
        }


        /// <summary>
        /// 通用查询语句
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> Get(
                    Expression<Func<TEntity, bool>> filter = null,
                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                    string includeProperties = "")
        {
            IQueryable<TEntity> query = this.Set();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            //注释原因：
            //foreach (var includeProperty in includeProperties.Split
                //(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            //{
               // query = query.Include(includeProperty);

            //}

            if (orderBy != null)
            {
                return orderBy(query);
            }
            else
            {
                return query;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DbSet<TEntity> Set()
        {
            return Context.Set<TEntity>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public TEntity FindByID(int ID)
        {
            return Set().Find(ID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private TEntity Create()
        {
            return Context.Set<TEntity>().Create();
        }
    }
}