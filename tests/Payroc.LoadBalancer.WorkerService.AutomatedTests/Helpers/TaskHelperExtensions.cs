namespace Payroc.LoadBalancer.WorkerService.AutomatedTests.Helpers;

public static class TaskHelperExtensions
{
    /// <summary>
    /// Blocks the calling thread until this has finished executing.
    /// If an exception is thrown the actual exception and not an
    /// aggregate exception is thrown.
    /// </summary>
    /// <param name="this"></param>
    public static void RunSync(this Task @this) =>
        @this.ConfigureAwait(false).GetAwaiter().GetResult();

    public static void RunSync(this ValueTask @this) =>
        @this.ConfigureAwait(false).GetAwaiter().GetResult();

    /// <summary>
    /// Blocks the calling thread until this has finished executing.
    /// If an exception is thrown the actual exception and not an
    /// aggregate exception is thrown.
    /// </summary>
    /// <typeparam name="TOut">The value of the Task of T</typeparam>
    /// <param name="this"></param>
    /// <returns></returns>
    public static TOut RunSync<TOut>(this Task<TOut> @this) =>
        @this.ConfigureAwait(false).GetAwaiter().GetResult();
}
