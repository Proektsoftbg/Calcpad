using System.Linq.Expressions;
using Calcpad.WebApi.Models.Base;
using Calcpad.WebApi.Models.Base.Updater;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Calcpad.WebApi.Models.Base
{
    public static class MongoExtensions
    {
        #region ObjectId 判断
        /// <summary>
        /// 是否为空 ObjectId
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public static bool IsEmpty(this ObjectId objectId)
        {
            return ObjectId.Empty == objectId;
        }
        #endregion

        #region 查找
        /// <summary>
        /// 查找所有的元素
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="dBContext"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static async Task<List<TDocument>> FindAsync<TDocument>(
            this MongoDBContext dBContext,
            Expression<Func<TDocument, bool>>? filter = null
        )
        {
            var emptyFilter = Builders<TDocument>.Filter.Empty;
            return await dBContext
                .Collection<TDocument>()
                .Find(filter ?? emptyFilter)
                .ToListAsync();
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
        public static async Task<TDocument> FirstOrDefaultAsync<TDocument>(
            this MongoDBContext dBContext,
            Expression<Func<TDocument, bool>>? filter = null,
            FindOptions options = null
        )
        {
            filter ??= x => true;
            return await dBContext
                .Collection<TDocument>()
                .Find(filter, options)
                .FirstOrDefaultAsync();
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
        public static FluentMongo<TDocument> AsFluentMongo<TDocument>(
            this MongoDBContext dBContext
        )
        {
            var mongoUpdater = new FluentMongo<TDocument>(dBContext);
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
        public static async Task InsertOneAsync<TDocument>(
            this MongoDBContext dBContext,
            TDocument document,
            InsertOneOptions options = null
        )
        {
            await dBContext.Collection<TDocument>().InsertOneAsync(document, options);
        }

        public static async Task InserManyAsync<TDocument>(
            this MongoDBContext dBContext,
            IEnumerable<TDocument> document,
            InsertManyOptions options = null
        )
        {
            await dBContext.Collection<TDocument>().InsertManyAsync(document, options);
        }
        #endregion
    }
}
