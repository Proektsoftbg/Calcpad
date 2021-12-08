using Calcpad.web.Data.Models;
using System.Threading.Tasks;

namespace Calcpad.web.Data.Services
{
    public interface ICategoryService
    {
        public Task<Category> AddAsync(Category category);
        public Task<Category> GetByIdAsync(int id);
        public Task<Category> RenameAsync(int id, string name);
        public Task<Category> DeleteAsync(int id);
    }
}
