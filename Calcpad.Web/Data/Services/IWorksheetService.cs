using Calcpad.web.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calcpad.web.Data.Services
{
    public interface IWorksheetService
    {
        public Task<Worksheet> AddAsync(Worksheet worksheet);
        public Task<Worksheet> GetByIdAsync(int id);
        public Task<string> GetSourceCodeAsync(int id);
        public Task<Worksheet> UpdateAsync(Worksheet worksheet);
        public Task<Worksheet> DeleteAsync(int id);
        public Task<IEnumerable<Worksheet>> SearchAsync(string seachTerm);
    }
}
