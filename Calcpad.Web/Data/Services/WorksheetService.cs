using Calcpad.web.Data.Models;
using Calcpad.web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Calcpad.web.Data.Services
{
    public class WorksheetService : IWorksheetService
    {
        private readonly ApplicationDbContext _context;
        private readonly IParserService _parserService;
        public WorksheetService(ApplicationDbContext context, IParserService parserService)
        {
            _context = context;
            _parserService = parserService;
        }

        public async Task<Worksheet> AddAsync(Worksheet worksheet)
        {
            if (string.IsNullOrWhiteSpace(worksheet.Html))
                worksheet.Html = await _parserService.ParseWorksheetAsync(worksheet.SourceCode);
            worksheet.Category = null;
            await _context.Worksheets.AddAsync(worksheet);
            await UpdateParentCount(worksheet.CategoryId, 1);
            await _context.SaveChangesAsync();
            return worksheet;
        }

        public async Task<Worksheet> GetByIdAsync(int id)
        {
            return await
                _context.Worksheets
                .Include(x => x.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<string> GetSourceCodeAsync(int id)
        {
            Worksheet worksheet = await _context.Worksheets.FindAsync(id);
            string code = worksheet?.SourceCode;
            if (!string.IsNullOrWhiteSpace(code))
            {
                int end = code.IndexOf('\v');
                if (end > 0)
                    code = code[..end];
            }
            return code;
        }

        public async Task<Worksheet> UpdateAsync(Worksheet worksheet)
        {
            if (string.IsNullOrWhiteSpace(worksheet.Html))
                worksheet.Html = await _parserService.ParseWorksheetAsync(worksheet.SourceCode);

            EntityEntry worksheetEntry = _context.Worksheets.Attach(worksheet);
            worksheetEntry.State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return worksheet;
        }

        public async Task<Worksheet> DeleteAsync(int id)
        {
            Worksheet worksheet = await _context.Worksheets.FindAsync(id);
            if (worksheet != null)
            {
                _context.Worksheets.Remove(worksheet);
                await UpdateParentCount(worksheet.CategoryId, -1);
                await _context.SaveChangesAsync();
            }
            return worksheet;
        }

        private async Task UpdateParentCount(int id, int increment)
        {
            Category parent = await _context.Categories.FindAsync(id);
            while (parent != null)
            {
                parent.Count += increment;
                parent = await _context.Categories.FindAsync(parent.ParentId);
            }
        }

        public async Task<IEnumerable<Worksheet>> SearchAsync(string seachTerm)
        {
            IQueryable<Worksheet> result;

            if (string.IsNullOrWhiteSpace(seachTerm))
                result = _context.Worksheets
                    .Include(x => x.Category)
                    .ThenInclude(c => c.Parent);
            else
                result = _context.Worksheets
                    .Where(x =>
                        x.Title.Contains(seachTerm) ||
                        x.SourceCode.Contains(seachTerm))
                    .Include(x => x.Category)
                    .ThenInclude(c => c.Parent);

            foreach (Worksheet worksheet in result)
            {
                Category parent = worksheet.Category.Parent;
                while (parent != null)
                {
                    await _context.Entry(parent).Reference(c => c.Parent).LoadAsync();
                    parent = parent.Parent;
                }
            }
            return await result.AsNoTracking().ToListAsync();
        }
    }
}
