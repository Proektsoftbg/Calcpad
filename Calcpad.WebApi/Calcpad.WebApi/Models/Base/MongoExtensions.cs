using Calcpad.WebApi.Models.Base;
using Calcpad.WebApi.Models.Base.Updater;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Calcpad.WebApi.Models.Base
{
    public static class MongoExtensions
    {
        #region 查找
        /// <summary>
        /// 查找所有的元素
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="dBContext"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static async Task<List<TDocument>> FindAsync<TDocument>(this MongoDBContext dBContext, Expression<Func<TDocument, bool>>? filter = null)
        {
            var emptyFilter = Builders<TDocument>.Filter.Empty;
            return await dBContext.Collection<TDocument>().Find(filter ?? emptyFilter).ToListAsync();
        }

        /// <summary>
        /// 查找第一个匹配的值
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="collection"></param>
        /// <param name="filter"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TDocument> FirstOrDefaultAsync<TDocument>(this MongoDBContext dBContext, Expression<Func<TDocument, bool>>? filter = null, FindOptions options = null)
        {
            filter ??= x => true;
            return await dBContext.Collection<TDocument>().Find(filter, options).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 使用 LinQ 查询
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="dBContext"></param>
        /// <returns></returns>
        public static IQueryable<TDocument> AsQueryable<TDocument>(this MongoDBContext dBContext)
        {
            return dBContext.Collection<TDocument>().AsQueryable();
        }
        #endregion

        #region 更新       
        /// <summary>
        /// 使用 Fluent 进行更新
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="dBContext"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static FluentMongoUpdater<TDocument> AsFluentUpdate<TDocument>(this MongoDBContext dBContext)
        {
            var mongoUpdater = new FluentMongoUpdater<TDocument>(dBContext);
            return mongoUpdater;
        }
        #endregion

        #region 添加
        /// <summary>
        /// 添加文档
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="dBContext"></param>
        /// <param name="document"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task InsertOneAsync<TDocument>(this MongoDBContext dBContext, TDocument document, InsertOneOptions options = null)
        {
            await dBContext.Collection<TDocument>().InsertOneAsync(document, options);
        }
        #endregion 
    }
}
