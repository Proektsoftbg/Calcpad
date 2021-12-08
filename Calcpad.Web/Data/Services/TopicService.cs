using Calcpad.web.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Calcpad.web.Data.Services
{
    public class TopicService : ITopicService
    {
        private readonly ApplicationDbContext _context;
        public TopicService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Topic> AddAsync(Topic topic)
        {
            await _context.Topics.AddAsync(topic);
            await _context.SaveChangesAsync();
            return topic;
        }
        public async Task<List<Topic>> GetAllAsync()
        {
            return await _context.Topics
                .Include(x => x.Articles)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Topic> GetByIdAsync(int id)
        {
            return await _context.Topics
                .Include(x => x.Articles)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Topic> RenameAsync(int id, string name)
        {
            Topic topic = await _context.Topics.FindAsync(id);
            if (topic != null)
            {
                _context.Entry(topic).Property(nameof(Topic.Name)).CurrentValue = name;
                await _context.SaveChangesAsync();
            }
            return topic;
        }

        public async Task<Topic> DeleteAsync(int id)
        {
            Topic topic = await GetByIdAsync(id);
            if (topic != null)
            {
                if (topic.Articles.Any())
                    return null;

                _context.Topics.Remove(topic);
                await _context.SaveChangesAsync();
            }
            return topic;
        }
    }
}
