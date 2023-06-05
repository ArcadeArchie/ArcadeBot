using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.Net.Websockets;

/// <summary>
/// Class that wraps SemaphoreSlim and enables to use locking inside 'using' blocks easily
/// Don't need to bother with releasing and handling SemaphoreSlim correctly
/// Example:
/// <code>
/// using(await _asyncLock.LockAsync())
/// {
///     // do your synchronized work
/// }
/// </code>
/// </summary>
internal sealed class AsyncLock
{
    private readonly Task<IDisposable> _releaserTask;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly IDisposable _releaser;

    public AsyncLock()
    {
        _releaser = new Releaser(_semaphore);
        _releaserTask = Task.FromResult(_releaser);
    }

    /// <summary>
    /// Use inside 'using' block
    /// </summary>
    public IDisposable Lock()
    {
        _semaphore.Wait();
        return _releaser;
    }
    /// <summary>
    /// Use inside 'using' block with await
    /// </summary>
    public Task<IDisposable> LockAsync()
    {
        var waitTask = _semaphore.WaitAsync();
        return waitTask.IsCompleted ? _releaserTask :
        waitTask.ContinueWith(
            (_, releaser) => (IDisposable)releaser!,
            _releaser,
            CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }


    private class Releaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        public Releaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }
        public void Dispose() => _semaphore.Dispose();
    }
}
