using Calcpad.web.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calcpad.web.Data.Services
{
    public interface ITopicService
    {
        public Task<Topic> AddAsync(Topic topic);
        public Task<Topic> GetByIdAsync(int id);
        public Task<List<Topic>> GetAllAsync();
        public Task<Topic> RenameAsync(int id, string name);
        public Task<Topic> DeleteAsync(int id);
    }
}
