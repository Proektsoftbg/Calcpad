using Calcpad.WebApi.Models.Base;
using Calcpad.WebApi.Utils.Attributes;
using log4net;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Calcpad.WebApi.Models.Base
{
    public class MongoDBContext
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MongoDBContext));

        private readonly MongoUrl _mongoUrl;

        public string DatabaseName => _mongoUrl.DatabaseName;
        public MongoClient Client { get; private set; }

        public MongoDBContext(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new Exception("mongoDb:connectionString is null or empty");
            // get database name from connection string
            _mongoUrl = new MongoUrl(connectionString);
            if (string.IsNullOrEmpty(_mongoUrl.DatabaseName)) throw new Exception("mongoDb:connectionString does not contain database name");


            Client = new MongoClient(_mongoUrl);
            ConventionRegistry.Register("IgnoreExtraElements", new ConventionPack { new IgnoreExtraElementsConvention(true) }, t => true);
        }

        /// <summary>
        /// 获取集合
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <returns></returns>
        public IMongoCollection<TDocument> Collection<TDocument>(string modelName = "")
        {
            if (string.IsNullOrEmpty(modelName))
            {
                // 获取 modelName
                modelName = CollectionNameAttribute.ResolveCollectionName(typeof(TDocument));
            }
            return Client.GetDatabase(DatabaseName).GetCollection<TDocument>(modelName);
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        /// <returns></returns>
        public async Task<T?> StartTransaction<T>(Func<IClientSessionHandle, Task<T>> func)
        {
            if (func == null) return default;
            using var session = await Client.StartSessionAsync();
            try
            {
                session.StartTransaction();
                var result = await func(session);
                await session.CommitTransactionAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                await session.AbortTransactionAsync();
                return default;
            }
        }

        #region 集合
        /// <summary>
        /// 用户集合
        /// </summary>
        public IMongoCollection<CalcpadUserModel> CalcpadUsers => Collection<CalcpadUserModel>();
        public IMongoCollection<CalcpadFileModel> FileObjects => Collection<CalcpadFileModel>();
        #endregion
    }
}
