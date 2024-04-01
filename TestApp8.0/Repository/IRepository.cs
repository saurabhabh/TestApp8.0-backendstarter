using Microsoft.AspNetCore.JsonPatch;
using System.Linq.Expressions;

namespace TestApp8._0.Repository
{
    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        bool CheckIfDBExists();
        bool CheckPendingMigrations();
        void CheckDBCreated();
        //void SetDBContext(string dbName);

        IQueryable<TEntity> GetAllEntities(bool bIsAsTrackable = false);

        IQueryable<TEntity> GetFilteredEntities(bool bIsAsTrackable = false);

        //IUserInfoService getLoggedInUserInfo();

        //IUserInfoService getLoggedInUserInfoAuthServer();


        #region SYNC METHODS

        TEntity GetEntityById(TKey id, bool bIsAsTrackable = false);

        dynamic GetPartialEntity(TKey id, string columns = "");

        dynamic GetPartialEntities(Expression<Func<TEntity, bool>> filterExpression = null, string columns = "");

        void CreateEntity(TEntity entity);

        void CreateBulkEntity(List<TEntity> entities);

        void UpdateEntity(TEntity entity);

        void UpdateBulkEntity(List<TEntity> entities);

        void UpdatePartialEntity(TEntity entity, JsonPatchDocument<TEntity> patchent);

        Task<int> UpdatePartialEntityAsync<TDto>(TEntity entity, JsonPatchDocument<TDto> patchent) where TDto : class;

        void DeleteEntityByID(TKey id);

        bool Exist(Expression<Func<TEntity, bool>> filterExpression);

        bool Any(Expression<Func<TEntity, bool>> filterExpression);

        int Count(Expression<Func<TEntity, bool>> filterExpression);

        bool SaveChanges();

        #endregion

        #region ASYNC METHODS

        Task<List<TEntity>> GetAllEntitiesAsync();

        Task<TEntity> GetEntityByIdAsync(TKey id, bool bIsAsTrackable = false);

        Task<dynamic> GetPartialEntityAsync(TKey id, string columns = "");

        Task<int> UpdateEntityAsync(TKey key, Expression<Func<TEntity, TEntity>> updateExpression);

        Task<int> UpdateBulkEntityAsync(Expression<Func<TEntity, TEntity>> updateExpression, Expression<Func<TEntity, bool>> filterExpression = null);

        Task<int> DeleteEntityAsync(TKey key);

        Task<int> DeleteBulkEntityAsync(Expression<Func<TEntity, bool>> filterExpression = null, int timeOut = 1000, int batchSize = 200);

        Task<bool> ExistAsync(Expression<Func<TEntity, bool>> filterExpression);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filterExpression);

        Task<int> CountAsync(Expression<Func<TEntity, bool>> filterExpression);

        Task<bool> SaveChangesAsync();

        #endregion

        void DetachAllEntities();


    }
}
