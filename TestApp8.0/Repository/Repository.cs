using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using TestApp8._0.Domain;
using Microsoft.AspNetCore.JsonPatch;
using Z.EntityFramework.Extensions;
using System.Linq.Dynamic.Core;


namespace TestApp8._0.Repository
{
    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
    {

        #region PRIVATE MEMBERS

        private readonly DataContext _context;
        private readonly DbSet<TEntity> _dbset;
        protected readonly ILogger<Repository<TEntity, TKey>> _logger;


        #endregion


        #region CONSTRUCTOR

        public Repository(ILogger<Repository<TEntity, TKey>> logger, IConfiguration config, DataContext context)
        {
            _context = context; // new DataContext(config);
            _dbset = _context.Set<TEntity>();
            _logger = logger;
        }



        #endregion


        #region PUBLIC METHODS

        //public void SetDBContext(string dbName)
        //{
        //    if (dbName != null)
        //    {
        //        _context.Database.GetDbConnection().ConnectionString = "Server=vaderancodb.database.windows.net;User Id=vaderanco@vaderancodb;Password=Vadera@2019;initial catalog=" + dbName + ";integrated security=False;MultipleActiveResultSets=False;App=EntityFramework";
        //    }
        //    else
        //    {
        //        string selecteddb = _context.getLoggedInUserInfo().SelectedDatabase;
        //        _context.Database.GetDbConnection().ConnectionString = "Server=vaderancodb.database.windows.net;User Id=vaderanco@vaderancodb;Password=Vadera@2019;initial catalog=" + selecteddb + ";integrated security=False;MultipleActiveResultSets=False;App=EntityFramework";
        //    }
        //}

        //public IUserInfoService getLoggedInUserInfoAuthServer()
        //{
        //    return _context.getLoggedInUserInfoAuthServer();
        //}

        //public IUserInfoService getLoggedInUserInfo()
        //{
        //    return _context.getLoggedInUserInfo();
        //}

        // Note: SetDBContext Should be called and the connection string should be properly set before calling this method.
        public bool CheckPendingMigrations()
        {
            return !_context.Database.GetPendingMigrations().Any();
        }

        // Note: SetDBContext Should be called and the connection string should be properly set before calling this method.
        // Creates the DB if it does not exist and runs all the migrations. 
        public void CheckDBCreated()
        {
            _context.Database.Migrate();
        }

        public bool CheckIfDBExists()
        {
            return (_context.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists();
        }



        public IQueryable<TEntity> GetAllEntities(bool bIsAsTrackable = false)
        {
            if (bIsAsTrackable)
            {
                return _dbset;
            }
            else
            {
                return _dbset.AsNoTracking();
            }
        }

        #region CRUD METHODS

        #region SYNC METHODS

        public TEntity GetEntityById(TKey id, bool bIsAsTrackable = false)
        {
            if (bIsAsTrackable)
            {
                return ((DbSet<TEntity>)GetFilteredEntities()).Find(id);
            }
            else
            {
                var entityExpression = Expression.Parameter(typeof(TEntity), "e");
                var memberExpression = Expression.Property(entityExpression, GetPrimaryKeyColumnName());
                var costExpression = Expression.Constant(id, typeof(TKey));
                var equalityExp = Expression.Equal(memberExpression, costExpression);

                Expression<Func<TEntity, bool>> predicate = Expression.Lambda<Func<TEntity, bool>>(equalityExp, entityExpression);
                Func<TEntity, bool> compiled = predicate.Compile();
                return GetFilteredEntities().Where(predicate).FirstOrDefault();
            }
        }

        public dynamic GetPartialEntity(TKey id, string columns = "")
        {

            var entityExpression = Expression.Parameter(typeof(TEntity), "e");
            var memberExpression = Expression.Property(entityExpression, GetPrimaryKeyColumnName());
            var costExpression = Expression.Constant(id, typeof(TKey));
            var equalityExp = Expression.Equal(memberExpression, costExpression);
            Expression<Func<TEntity, bool>> predicate = Expression.Lambda<Func<TEntity, bool>>(equalityExp, entityExpression);
            Func<TEntity, bool> compiled = predicate.Compile();

            string selectClause = "new (" + columns + " )";

            return GetFilteredEntities(false).Where(predicate).Select(selectClause).FirstOrDefault();
        }

        public dynamic GetPartialEntities(Expression<Func<TEntity, bool>> filterExpression = null, string columns = "")
        {
            string selectClause = "new (" + columns + " )";
            if (filterExpression != null)
            {
                return GetFilteredEntities(false).Where(filterExpression).Select(selectClause).ToDynamicList();
            }
            else
            {
                return GetFilteredEntities(false).Select(selectClause).ToDynamicList();
            }
        }

        public void CreateEntity(TEntity entity)
        {
            if (entity != null && IsEntityValid(entity))
            {
                _dbset.Add(entity);
                _context.Entry(entity).State = EntityState.Added;
            }
        }

        public void CreateBulkEntity(List<TEntity> entities)
        {
            foreach (TEntity ent in entities)
            {
                if (ent != null && IsEntityValid(ent))
                {
                    _dbset.Add(ent);
                    _context.Entry(ent).State = EntityState.Added;
                }
            }
        }

        public void DetachAllEntities()
        {
            _dbset.Local.ToList().ForEach(x =>
            {
                _context.Entry(x).State = EntityState.Detached;
                x = null; // <- this doesn't seem to be required for garbage collection
            });
        }

        public void UpdateEntity(TEntity entity)
        {
            if (entity != null && IsEntityValid(entity))
            {
                _dbset.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        public void UpdateBulkEntity(List<TEntity> entities)
        {
            foreach (TEntity ent in entities)
            {
                if (ent != null && IsEntityValid(ent))
                {
                    _dbset.Attach(ent);
                    _context.Entry(ent).State = EntityState.Modified;
                }

            }
        }

        public void UpdatePartialEntity(TEntity entity, JsonPatchDocument<TEntity> patchent)
        {
            if (entity != null && IsEntityValid(entity))
            {
                _dbset.Attach(entity);

                foreach (var op in patchent.Operations)
                {
                    if (op.OperationType == Microsoft.AspNetCore.JsonPatch.Operations.OperationType.Replace)
                    {
                        var exp = GetPropertySelector<TEntity>(op.path.Substring(1));
                        _context.Entry(entity).Property(exp).IsModified = true;
                    }
                }
            }
        }

        public void DeleteEntityByID(TKey id)
        {
            TEntity entity = GetEntityById(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Deleted;
            }
        }

        public bool Exist(Expression<Func<TEntity, bool>> filterExpression)
        {
            return GetFilteredEntities().Where(filterExpression).Any();
        }

        public bool Any(Expression<Func<TEntity, bool>> filterExpression)
        {
            return GetFilteredEntities().Any(filterExpression);
        }

        public int Count(Expression<Func<TEntity, bool>> filterExpression)
        {
            return GetFilteredEntities().Count(filterExpression);
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0 ? true : false;
        }

        #endregion

        #region ASYNC METHODS

        public async Task<List<TEntity>> GetAllEntitiesAsync()
        {
            return await GetAllEntities().ToListAsync();
        }

        public async Task<TEntity> GetEntityByIdAsync(TKey id, bool bIsAsTrackable = false)
        {
            if (bIsAsTrackable)
            {
                return await ((DbSet<TEntity>)GetFilteredEntities(bIsAsTrackable)).FindAsync(id);
            }
            else
            {

                var entityExpression = Expression.Parameter(typeof(TEntity), "e");
                var memberExpression = Expression.Property(entityExpression, GetPrimaryKeyColumnName());
                var costExpression = Expression.Constant(id, typeof(TKey));
                var equalityExp = Expression.Equal(memberExpression, costExpression);

                Expression<Func<TEntity, bool>> predicate = Expression.Lambda<Func<TEntity, bool>>(equalityExp, entityExpression);
                Func<TEntity, bool> compiled = predicate.Compile();
                return await GetFilteredEntities(bIsAsTrackable).Where(predicate).FirstOrDefaultAsync();
            }
        }

        public async Task<dynamic> GetPartialEntityAsync(TKey id, string columns = "")
        {

            var entityExpression = Expression.Parameter(typeof(TEntity), "e");
            var memberExpression = Expression.Property(entityExpression, GetPrimaryKeyColumnName());
            var costExpression = Expression.Constant(id, typeof(TKey));
            var equalityExp = Expression.Equal(memberExpression, costExpression);
            Expression<Func<TEntity, bool>> predicate = Expression.Lambda<Func<TEntity, bool>>(equalityExp, entityExpression);
            Func<TEntity, bool> compiled = predicate.Compile();

            string selectClause = "new (" + columns + " )";

            return await GetFilteredEntities(false).Where(predicate).Select(selectClause).FirstOrDefaultAsync();

        }

        public async Task<int> UpdatePartialEntityAsync<TDto>(TEntity entity, JsonPatchDocument<TDto> patchent) where TDto : class
        {
            if (entity != null && IsEntityValid(entity))
            {
                _dbset.Attach(entity);

                foreach (var op in patchent.Operations)
                {
                    if (op.OperationType == Microsoft.AspNetCore.JsonPatch.Operations.OperationType.Replace)
                    {
                        var exp = GetPropertySelector<TEntity>(op.path.Substring(1));
                        _context.Entry<TEntity>(entity).Property(exp).IsModified = true;
                    }
                }

                return await _context.SaveChangesAsync();
            }
            return await Task.FromResult(0);
        }

        public async Task<int> UpdateEntityAsync(TKey key, Expression<Func<TEntity, TEntity>> updateExpression)
        {
            var entityExpression = Expression.Parameter(typeof(TEntity), "e");
            var memberExpression = Expression.Property(entityExpression, GetPrimaryKeyColumnName());
            var costExpression = Expression.Constant(key, typeof(TKey));
            var equalityExp = Expression.Equal(memberExpression, costExpression);

            Expression<Func<TEntity, bool>> predicate = Expression.Lambda<Func<TEntity, bool>>(equalityExp, entityExpression);
            Func<TEntity, bool> compiled = predicate.Compile();

            BatchUpdateManager.BatchUpdateBuilder = builder =>
            {
                builder.Executing = command =>
                {
                    Console.WriteLine(command.CommandText);
                };
            };

            return await (GetFilteredEntities().Where(predicate)).UpdateFromQueryAsync(updateExpression);
        }

        public async Task<int> UpdateBulkEntityAsync(Expression<Func<TEntity, TEntity>> updateExpression, Expression<Func<TEntity, bool>> filterExpression = null)
        {
            //BatchUpdateManager.BatchUpdateBuilder = builder =>
            //{
            //    builder.Executing = command => {
            //        Console.WriteLine(command.CommandText);
            //    };
            //};

            if (filterExpression == null)
            {
                return await GetFilteredEntities().UpdateFromQueryAsync(updateExpression);
            }
            else
            {
                return await GetFilteredEntities().Where(filterExpression).UpdateFromQueryAsync(updateExpression);
            }
        }

        public async Task<int> DeleteEntityAsync(TKey key)
        {
            var entityExpression = Expression.Parameter(typeof(TEntity), "e");
            var memberExpression = Expression.Property(entityExpression, GetPrimaryKeyColumnName());
            var costExpression = Expression.Constant(key, typeof(TKey));
            var equalityExp = Expression.Equal(memberExpression, costExpression);

            Expression<Func<TEntity, bool>> predicate = Expression.Lambda<Func<TEntity, bool>>(equalityExp, entityExpression);
            Func<TEntity, bool> compiled = predicate.Compile();

            return await (GetFilteredEntities().Where(predicate)).DeleteFromQueryAsync();
        }

        public async Task<int> DeleteBulkEntityAsync(Expression<Func<TEntity, bool>> filterExpression = null, int timeOut = 1000, int batchSize = 200)
        {
            if (filterExpression == null)
            {
                _context.Database.SetCommandTimeout(timeOut);
                var result = await GetFilteredEntities().DeleteFromQueryAsync(x => x.BatchSize = batchSize);
                _context.Database.SetCommandTimeout(30);
                return result;
            }
            else
            {
                _context.Database.SetCommandTimeout(timeOut);
                var result = await ((IQueryable<TEntity>)GetFilteredEntities().Where(filterExpression)).DeleteFromQueryAsync(x => x.BatchSize = batchSize);
                _context.Database.SetCommandTimeout(30);
                return result;
            }
        }

        public async Task<bool> ExistAsync(Expression<Func<TEntity, bool>> filterExpression)
        {
            return await GetFilteredEntities().Where(filterExpression).AnyAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filterExpression)
        {
            return await GetFilteredEntities().AnyAsync(filterExpression);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> filterExpression)
        {
            return await GetFilteredEntities().CountAsync(filterExpression);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0 ? true : false;
        }

        #endregion

        #endregion

        #region VIRTUAL MEMBERS

        public virtual string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public virtual IQueryable<TEntity> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        public virtual bool IsEntityValid(TEntity entity)
        {
            return true;
        }

        #endregion

        #endregion


        #region READ PROPERTIES

        public static Expression<Func<T, object>> GetPropertySelector<T>(string propertyName)
        {
            var arg = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(arg, propertyName);
            //return the property as object
            var conv = Expression.Convert(property, typeof(object));
            var exp = Expression.Lambda<Func<T, object>>(conv, new ParameterExpression[] { arg });
            return exp;
        }

        #endregion


    }
}
