using System;
using System.Threading.Tasks;

// Helpers for calls to the Cloud Code module
public static class CloudCall
{
    // The task's result, or a TimeoutException when it takes longer than the timeout. A late failure of the
    // abandoned call is observed so it doesn't surface as an unobserved task exception.
    public static async Task<T> WithTimeout<T>(Task<T> task, int timeoutMilliseconds)
    {
        if (await Task.WhenAny(task, Task.Delay(timeoutMilliseconds)) != task)
        {
            _ = task.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);
            throw new TimeoutException($"No answer within {timeoutMilliseconds} ms");
        }

        return await task;
    }
}
