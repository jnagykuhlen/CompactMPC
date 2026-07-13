using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompactMPC;

public static class TaskExtensions
{
    public static Task<T>[] ToSubTasks<T>(this Task<T[]> superTask, int numberOfSubTasks)
    {
        var subTasks = new Task<T>[numberOfSubTasks];
        for (var i = 0; i < numberOfSubTasks; ++i)
        {
            var index = i;
            subTasks[i] = superTask.ContinueWith(task => task.Result[index]);
        }

        return subTasks;
    }

    public static Task<TSub>[] ToSubTasks<TSub, TSuper>(this Task<TSuper> superTask, Func<TSuper, TSub[]> selector, int numberOfSubTasks) =>
        superTask.ContinueWith(task => selector(task.Result)).ToSubTasks(numberOfSubTasks);

    public static Task<T[]> ToSuperTask<T>(this Task<T>[] subTasks) =>
        Task.WhenAll(subTasks);

    public static Task<TSuper> ToSuperTask<TSub, TSuper>(this Task<TSub>[] subTasks, Func<TSub[], TSuper> selector) =>
        subTasks.ToSuperTask().ContinueWith(task => selector(task.Result));

    public static async Task<T[]> AndThenAll<T>(this Task<T> task, IEnumerable<Task<T>> otherTasks)
    {
        await task;
        return await Task.WhenAll(otherTasks.Prepend(task));
    }
}