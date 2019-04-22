using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public static class TaskHelper
    {
        public static Task<T>[] ToSubTasks<T>(this Task<T[]> superTask, int numberOfSubTasks)
        {
            Task<T>[] subTasks = new Task<T>[numberOfSubTasks];
            for (int i = 0; i < numberOfSubTasks; ++i)
            {
                int index = i;
                subTasks[i] = superTask.ContinueWith(task => task.Result[index]);
            }

            return subTasks;
        }

        public static Task<TSub>[] ToSubTasks<TSub, TSuper>(this Task<TSuper> superTask, Func<TSuper, TSub[]> selector, int numberOfSubTasks)
        {
            return superTask.ContinueWith(task => selector(task.Result)).ToSubTasks(numberOfSubTasks);
        }

        public static Task<T[]> ToSuperTask<T>(this Task<T>[] subTasks)
        {
            return Task.WhenAll(subTasks);
        }

        public static Task<TSuper> ToSuperTask<TSub, TSuper>(this Task<TSub>[] subTasks, Func<TSub[], TSuper> selector)
        {
            return subTasks.ToSuperTask().ContinueWith(task => selector(task.Result));
        }
    }
}
