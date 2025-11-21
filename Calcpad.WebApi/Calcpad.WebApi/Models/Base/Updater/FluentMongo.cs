using System.Linq.Expressions;
using MongoDB.Driver;

namespace Calcpad.WebApi.Models.Base.Updater
{
    public class FluentMongo<TDocument>(MongoDBContext dBContext)
    {
        private readonly List<FilterDefinition<TDocument>> _filters = [];
        private readonly List<UpdateDefinition<TDocument>> _updaters = [];

        private readonly MongoDBContext _db = dBContext;

        #region 过滤器添加
        /// <summary>
        /// 添加 FilterDefinition
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public FluentMongo<TDocument> Match(FilterDefinition<TDocument> filter)
        {
            _filters.Add(filter);
            return this;
        }

        /// <summary>
        /// 匹配值
        /// 可以多次调用以添加多个过滤器，最终会将它们组合在一起
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentMongo<TDocument> Where(Expression<Func<TDocument, bool>>? expression)
        {
            if (expression != null)
                _filters.Add(Builders<TDocument>.Filter.Where(expression));
            return this;
        }
        #endregion

        #region 获取值
        /// <summary>
        /// 获取所有匹配项
        /// </summary>
        /// <returns></returns>
        public async Task<List<TDocument>> ToListAsync()
        {
            var filter = GetFilterDefinition();
            return (await _db.Collection<TDocument>().FindAsync(filter)).ToList();
        }

        /// <summary>
        /// 获取游标
        /// </summary>
        /// <returns></returns>
        public async Task<IAsyncCursor<TDocument>> ToCursorAsync()
        {
            var filter = GetFilterDefinition();
            return await _db.Collection<TDocument>().FindAsync(filter);
        }

        /// <summary>
        /// 获取第一个匹配项
        /// </summary>
        /// <returns></returns>
        public async Task<TDocument> FirstOrDefaultAsync()
        {
            var filter = GetFilterDefinition();
            return await _db.Collection<TDocument>().Find(filter).FirstOrDefaultAsync();
        }

        public async Task<long> CountDocumentsAsync()
        {
            var filter = GetFilterDefinition();
            return await _db.Collection<TDocument>().CountDocumentsAsync(filter);
        }
        #endregion


        #region 设置值
        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public FluentMongo<TDocument> Set<TField>(
            FieldDefinition<TDocument, TField> field,
            TField value
        )
        {
            _updaters.Add(Builders<TDocument>.Update.Set(field, value));
            return this;
        }

        public FluentMongo<TDocument> Set<TField>(
            Expression<Func<TDocument, TField>> field,
            TField value
        )
        {
            _updaters.Add(Builders<TDocument>.Update.Set(field, value));
            return this;
        }

        /// <summary>
        /// 使用对象中的指定字段来设置值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public FluentMongo<TDocument> Set(TDocument data, List<string> keys)
        {
            foreach (var key in keys)
            {
                try
                {
                    // 使用表达式树构建 getter 以提升性能，避免直接反射调用 GetValue
                    var param = Expression.Parameter(typeof(TDocument), "x");
                    var propExpr = Expression.Property(param, key);
                    var convertExpr = Expression.Convert(propExpr, typeof(object));
                    var lambda = Expression.Lambda<Func<TDocument, object>>(convertExpr, param);
                    var getter = lambda.Compile();
                    var value = getter(data);
                    _updaters.Add(Builders<TDocument>.Update.Set(key, value));
                }
                catch (Exception)
                {
                    // 说明属性不存在，跳过
                    continue;
                }
            }
            return this;
        }

        public FluentMongo<TDocument> Inc<TField>(
            Expression<Func<TDocument, TField>> field,
            TField value
        )
        {
            _updaters.Add(Builders<TDocument>.Update.Inc(field, value));
            return this;
        }

        /// <summary>
        /// 通过添加 updater 来更新值
        /// </summary>
        /// <param name="updater"></param>
        /// <returns></returns>
        public FluentMongo<TDocument> Set(UpdateDefinition<TDocument> updater)
        {
            _updaters.Add(updater);
            return this;
        }

        public FluentMongo<TDocument> Unset<TField>(FieldDefinition<TDocument, TField> field)
        {
            _updaters.Add(Builders<TDocument>.Update.Unset(field));
            return this;
        }

        public FluentMongo<TDocument> AddToSet<TField>(
            Expression<Func<TDocument, IEnumerable<TField>>> field,
            TField value
        )
        {
            _updaters.Add(Builders<TDocument>.Update.AddToSet(field, value));
            return this;
        }
        #endregion


        #region 更新
        public async Task<UpdateResult> UpdateOneAsync(UpdateOptions options = null)
        {
            var filter = GetFilterDefinition();
            var updater = GetUpdateDefinition();

            return await _db.Collection<TDocument>().UpdateOneAsync(filter, updater, options);
        }

        public async Task<UpdateResult> UpdateManyAsync(UpdateOptions options = null)
        {
            var filter = GetFilterDefinition();
            var updater = GetUpdateDefinition();
            return await _db.Collection<TDocument>().UpdateManyAsync(filter, updater, options);
        }

        /// <summary>
        /// 执行更新
        /// </summary>
        /// <returns></returns>
        public async Task<TDocument> FindOneAndUpdateAsync(
            FindOneAndUpdateOptions<TDocument>? options = null
        )
        {
            var _filter = GetFilterDefinition();
            var _updater = GetUpdateDefinition();

            return await _db.Collection<TDocument>()
                .FindOneAndUpdateAsync(_filter, _updater, options);
        }

        /// <summary>
        /// 更新并返回更新前或更新后的文档，取决于提供的选项
        /// </summary>
        /// <param name="optionsFunc"></param>
        /// <returns></returns>
        public async Task<TDocument> FindOneAndUpdateAsync(
            Action<FindOneAndUpdateOptions<TDocument>> optionsFunc
        )
        {
            var _filter = GetFilterDefinition();
            var _updater = GetUpdateDefinition();
            var options = new FindOneAndUpdateOptions<TDocument>();
            optionsFunc.Invoke(options);

            return await _db.Collection<TDocument>()
                .FindOneAndUpdateAsync(_filter, _updater, options);
        }

        private FilterDefinition<TDocument> GetFilterDefinition()
        {
            return Builders<TDocument>.Filter.And(_filters);
        }

        private UpdateDefinition<TDocument> GetUpdateDefinition()
        {
            return Builders<TDocument>.Update.Combine(_updaters);
        }
        #endregion


        #region 删除
        /// <summary>
        /// 删除第一个匹配的文档
        /// </summary>
        /// <returns></returns>
        public async Task<DeleteResult> DeleteOneAsync()
        {
            var filter = GetFilterDefinition();
            return await _db.Collection<TDocument>().DeleteOneAsync(filter);
        }
        #endregion
    }
}
