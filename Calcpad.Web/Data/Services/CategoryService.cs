using Calcpad.web.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Calcpad.web.Data.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category> AddAsync(Category category)
        {
            category.Parent = null;
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                await _context.Entry(category).Reference(c => c.Parent).LoadAsync();
                Category parent = category.Parent;
                while (parent != null)
                {
                    await _context.Entry(parent).Reference(c => c.Parent).LoadAsync();
                    parent = parent.Parent;
                }
                await _context.Entry(category).Collection(c => c.Worksheets).LoadAsync();
                await _context.Entry(category).Collection(c => c.Children).LoadAsync();
            }
            return category;
        }

        public async Task<Category> RenameAsync(int id, string name)
        {
            Category category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Entry(category).Property(nameof(Category.Name)).CurrentValue = name;
                await _context.SaveChangesAsync();
            }
            return category;
        }

        public async Task<Category> DeleteAsync(int id)
        {
            Category category = await GetByIdAsync(id);
            if (category != null)
            {
                if (category.Children.Any() || category.Worksheets.Any())
                    return null;

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return category;
        }
    }
}
