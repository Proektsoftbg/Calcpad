using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Calcpad.WebApi.Models.Base.Updater
{
    public class FluentMongoUpdater<TDocument>(MongoDBContext dBContext)
    {
        private readonly List<FilterDefinition<TDocument>> _filters = [];
        private readonly List<UpdateDefinition<TDocument>> _updaters = [];
        private Expression<Func<TDocument, bool>>? _where = null;

        private readonly MongoDBContext _db = dBContext;

        #region 匹配
        /// <summary>
        /// 添加 FilterDefinition
        /// 该方法仅对 <see cref="FindOneAndUpdateAsync(FindOneAndUpdateOptions{TDocument}?)"/> 有效
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public FluentMongoUpdater<TDocument> Match(FilterDefinition<TDocument> filter)
        {
            _filters.Add(filter);
            return this;
        }

        /// <summary>
        /// 匹配值
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentMongoUpdater<TDocument> Where(Expression<Func<TDocument, bool>> expression)
        {
            _where = expression;
            _filters.Add(Builders<TDocument>.Filter.Where(expression));
            return this;
        }
        #endregion


        #region 更新
        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public FluentMongoUpdater<TDocument> Set<TField>(FieldDefinition<TDocument, TField> field, TField value)
        {
            _updaters.Add(Builders<TDocument>.Update.Set(field, value));
            return this;
        }

        public FluentMongoUpdater<TDocument> Set<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            _updaters.Add(Builders<TDocument>.Update.Set(field, value));
            return this;
        }

        public FluentMongoUpdater<TDocument> Inc<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            _updaters.Add(Builders<TDocument>.Update.Inc(field, value));
            return this;
        }

        /// <summary>
        /// 通过添加 updater 来更新值
        /// </summary>
        /// <param name="updater"></param>
        /// <returns></returns>
        public FluentMongoUpdater<TDocument> Set(UpdateDefinition<TDocument> updater)
        {
            _updaters.Add(updater);
            return this;
        }
        #endregion


        #region 执行
        public async Task<UpdateResult> UpdateOneAsync(UpdateOptions options = null)
        {
            if (_where == null)
            {
                throw new Exception("where is null");
            }

            var _updater = Builders<TDocument>.Update.Combine(_updaters);
            return await _db.Collection<TDocument>().UpdateOneAsync<TDocument>(_where, _updater, options);
        }

        public async Task<UpdateResult> UpdateManyAsync(UpdateOptions options = null)
        {
            if (_where == null)
            {
                throw new Exception("where is null");
            }

            var _updater = Builders<TDocument>.Update.Combine(_updaters);
            return await _db.Collection<TDocument>().UpdateManyAsync<TDocument>(_where, _updater, options);
        }

        /// <summary>
        /// 执行更新
        /// </summary>
        /// <returns></returns>
        public async Task<TDocument> FindOneAndUpdateAsync(FindOneAndUpdateOptions<TDocument>? options = null)
        {
            var _filter = Builders<TDocument>.Filter.And(_filters);
            var _updater = Builders<TDocument>.Update.Combine(_updaters);
            return await _db.Collection<TDocument>().FindOneAndUpdateAsync(_filter, _updater, options);
        }

        public async Task<TDocument> FindOneAndUpdateAsync(Action<FindOneAndUpdateOptions<TDocument>> optionsFunc)
        {
            var _filter = Builders<TDocument>.Filter.And(_filters);
            var _updater = Builders<TDocument>.Update.Combine(_updaters);
            var options = new FindOneAndUpdateOptions<TDocument>();
            optionsFunc.Invoke(options);

            return await _db.Collection<TDocument>().FindOneAndUpdateAsync(_filter, _updater, options);
        }
        #endregion
    }
}
