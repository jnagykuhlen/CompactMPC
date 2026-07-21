using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompactMPC;

public static class TaskExtensions
{
    public static async Task<T[]> AndThenAll<T>(this Task<T> task, IEnumerable<Task<T>> otherTasks)
    {
        await task;
        return await Task.WhenAll(otherTasks.Prepend(task));
    }
}
