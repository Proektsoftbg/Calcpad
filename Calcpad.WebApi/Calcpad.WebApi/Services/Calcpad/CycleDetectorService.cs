using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Calcpad.WebApi.Configs;
using Calcpad.WebApi.Models;
using Calcpad.WebApi.Models.Base;
using Calcpad.WebApi.Utils.Web.Service;
using MongoDB.Driver.Linq;

namespace Calcpad.WebApi.Services.Calcpad
{
    /// <summary>
    /// cpd file cycle detect
    /// use kahn's algorithm to detect cycles in include files
    /// </summary>
    /// <param name="db"></param>
    public class CycleDetectorService(MongoDBContext db) : IScopedService
    {
        /// <summary>
        /// check if the updated file with includeUids will cause cycle
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="includeUids"></param>
        /// <returns></returns>
        public async Task<bool> HasCycleAsync(string uniqueId, List<string> includeUids)
        {
            if (includeUids == null || includeUids.Count == 0)
                return false;

            // load all files (project is small enough to load indexes only)
            var allFiles = await db.AsQueryable<CalcpadFileModel>()
                .Where(x=>x.IncludeUniqueIds.Count>0)
                .Select(x => new FileStub(x.UniqueId, x.IncludeUniqueIds))
                .ToListAsync();

            // include the updated file projection (so BuildAdjacency can take it into account)
            allFiles.Add(new FileStub(uniqueId, includeUids));

            // build adjacency list and detect cycle
            var adj = BuildAdjacency(allFiles);
            return HasCycleByKahn(adj);
        }

        // Kahn's algorithm implementation extracted for clarity and reuse
        private static bool HasCycleByKahn(Dictionary<string, List<string>> adj)
        {
            // collect nodes from adjacency keys and values
            var nodes = new HashSet<string>(StringComparer.Ordinal);
            foreach (var kv in adj)
            {
                if (!string.IsNullOrEmpty(kv.Key))
                    nodes.Add(kv.Key);
                if (kv.Value == null)
                    continue;
                foreach (var v in kv.Value)
                    if (!string.IsNullOrEmpty(v))
                        nodes.Add(v);
            }

            // compute in-degrees
            var inDegree = nodes.ToDictionary(n => n, n => 0);
            foreach (var kv in adj)
            {
                if (kv.Value == null)
                    continue;
                foreach (var to in kv.Value)
                {
                    if (string.IsNullOrEmpty(to))
                        continue;
                    if (!inDegree.ContainsKey(to))
                        inDegree[to] = 0;
                    inDegree[to]++;
                }
            }

            // queue nodes with in-degree 0
            var q = new Queue<string>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
            var visited = 0;

            while (q.Count > 0)
            {
                var n = q.Dequeue();
                visited++;

                if (!adj.TryGetValue(n, out var neighbours) || neighbours == null)
                    continue;

                foreach (var m in neighbours)
                {
                    if (string.IsNullOrEmpty(m))
                        continue;
                    inDegree[m]--;
                    if (inDegree[m] == 0)
                        q.Enqueue(m);
                }
            }

            return visited != nodes.Count;
        }

        // small DTO for projecting file info from DB
        private sealed record FileStub(string UniqueId, List<string> IncludeUniqueIds);

        // Build adjacency list from a list of file stubs and the updated file's include list
        private static Dictionary<string, List<string>> BuildAdjacency(List<FileStub> files)
        {
            var adj = new Dictionary<string, List<string>>();
            var nodes = new HashSet<string>(StringComparer.Ordinal);

            foreach (var f in files)
            {
                if (string.IsNullOrEmpty(f.UniqueId))
                    continue;
                if (f.IncludeUniqueIds == null)
                    continue;

                var list = f.IncludeUniqueIds;
                adj[f.UniqueId] = [.. list];
                nodes.Add(f.UniqueId);
            }
            return adj;
        }
    }
}
