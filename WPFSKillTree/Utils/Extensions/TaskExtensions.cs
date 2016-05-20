using System;
using System.Threading.Tasks;

namespace POESKillTree.Utils.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<bool> WithTimeout(this Task task, TimeSpan timeout)
        {
            return await Task.WhenAny(task, Task.Delay(timeout)) == task;
        }
    }
}