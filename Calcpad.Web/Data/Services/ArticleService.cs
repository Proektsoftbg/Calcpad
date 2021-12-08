using Calcpad.web.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;

namespace Calcpad.web.Data.Services
{
    public class ArticleService : IArticleService
    {
        private readonly ApplicationDbContext _context;
        public ArticleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Article> AddAsync(Article article)
        {
            await _context.Articles.AddAsync(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<Article> GetByIdAsync(int id)
        {
            if (id == 0)
                return await _context.Articles.FirstOrDefaultAsync();

            return await _context.Articles
                .Include(c => c.Topic)
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Article> UpdateAsync(Article article)
        {
            EntityEntry articleEntry = _context.Articles.Attach(article);
            articleEntry.State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<Article> DeleteAsync(int id)
        {
            Article article = await _context.Articles.FindAsync(id);
            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }
            return article;
        }
    }
}
