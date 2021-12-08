using Calcpad.web.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Calcpad.web.Data.Services
{
    public interface IArticleService
    {
        public Task<Article> AddAsync(Article article);
        public Task<Article> GetByIdAsync(int id);
        public Task<Article> UpdateAsync(Article article);
        public Task<Article> DeleteAsync(int id);
    }
}
