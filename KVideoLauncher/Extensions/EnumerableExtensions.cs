using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KVideoLauncher.Extensions;

public static class EnumerableExtensions
{
    public static async Task<IEnumerable<T>> WhereAsync<T>
    (
        this IEnumerable<T> source, Func<T, Task<bool>> predicate
    )
    {
        var results = new ConcurrentQueue<T>();
        IEnumerable<Task> tasks = source.Select
        (
            async x =>
            {
                if (await predicate(x))
                    results.Enqueue(x);
            }
        );
        await Task.WhenAll(tasks);
        return results;
    }
}