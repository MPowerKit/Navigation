namespace MPowerKit.Navigation.Utilities;

public static class ThreadUtil
{
    private static SynchronizationContext? _uiContext = null;
    public static void Init(SynchronizationContext uiContext)
    {
        _uiContext = uiContext;
    }

    /// <summary>
    /// Determines if the current synchronization context is the UI Thread
    /// </summary>
    /// <param name="context">SynchronizationContext you want to compare with the UIThread SynchronizationContext</param>
    /// <returns>true or false</returns>
    public static bool IsOnUIThread(SynchronizationContext context)
    {
        return context == _uiContext;
    }

    /// <summary>
    /// Will run the Func on the UIThread asynchronously.
    /// PERFORMANCE: Do not use in a loop. Context switching can cause significant performance degradation. Call this as infrequently as possible.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task RunOnUIThreadAsync(Func<Task> action)
    {
        if (_uiContext == null)
            throw new Exception("You must call ThreadUtil.Init() before calling this method.");

        if (IsOnUIThread(SynchronizationContext.Current!) || SynchronizationContext.Current is ExclusiveSynchronizationContext)
            await action();
        else
            await RunOnUIThreadHelper(action);
    }

    /// <summary>
    /// Will run the Func on the UIThread asynchronously.
    /// PERFORMANCE: Do not use in a loop. Context switching can cause significant performance degradation. Call this as infrequently as possible.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async ValueTask RunOnUIThreadAsync(Func<ValueTask> action)
    {
        if (_uiContext == null)
            throw new Exception("You must call ThreadUtil.Init() before calling this method.");

        if (IsOnUIThread(SynchronizationContext.Current!) || SynchronizationContext.Current is ExclusiveSynchronizationContext)
            await action();
        else
            await RunOnUIThreadHelper(action);
    }

    /// <summary>
    /// Will run the Action on the UI Thread.
    /// PERFORMANCE: Do not use in a loop. Context switching can cause significant performance degradation. Call this as infrequently as possible.
    /// </summary>
    /// <param name="action"></param>
    public static void RunOnUIThread(Action action)
    {
        if (_uiContext is null)
            throw new Exception("You must call ThreadUtil.Init() before calling this method.");

        if (IsOnUIThread(SynchronizationContext.Current!) || SynchronizationContext.Current is ExclusiveSynchronizationContext)
            action();
        else
            RunOnUIThreadHelper(action).Wait(); // I can wait because I am not on the same thread.
    }

    /// <summary>
    /// Will run the Func on the UI Thread.
    /// PERFORMANCE: Do not use in a loop. Context switching can cause significant performance degradation. Call this as infrequently as possible.
    /// </summary>
    /// <param name="action"></param>
    public static void RunOnUIThread(Func<Task> action)
    {
        if (_uiContext == null)
            throw new Exception("You must call ThreadUtil.Init() before calling this method.");

        if (IsOnUIThread(SynchronizationContext.Current!) || SynchronizationContext.Current is ExclusiveSynchronizationContext)
        {
            RunSync(action);
        }
        else
            RunOnUIThreadHelper(action).Wait(); // I can wait because I am not on the same thread.
    }

    /// <summary>
    /// Will run the Func on the UI Thread.
    /// PERFORMANCE: Do not use in a loop. Context switching can cause significant performance degradation. Call this as infrequently as possible.
    /// </summary>
    /// <param name="action"></param>
    public static void RunOnUIThread(Func<ValueTask> action)
    {
        if (_uiContext == null)
            throw new Exception("You must call ThreadUtil.Init() before calling this method.");

        if (IsOnUIThread(SynchronizationContext.Current!) || SynchronizationContext.Current is ExclusiveSynchronizationContext)
        {
            RunSync(action);
        }
        else
            RunOnUIThreadHelper(action).Wait(); // I can wait because I am not on the same thread.
    }

    public static void Post(Action action)
    {
        _uiContext!.Post((e) =>
        {
            try
            {
                action?.Invoke();
            }
            catch { }
        }, null);
    }

    private static Task RunOnUIThreadHelper(Action action)
    {
        var tcs = new TaskCompletionSource();

        _uiContext!.Post((e) =>
        {
            try
            {
                action();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, null);

        return tcs.Task;
    }

    private static Task RunOnUIThreadHelper(Func<Task> action)
    {
        var tcs = new TaskCompletionSource();

        _uiContext!.Post((e) =>
        {
            try
            {
                action().ContinueWith(t =>
                {
                    tcs.SetResult();
                });
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, null);

        return tcs.Task;
    }

    private static Task RunOnUIThreadHelper(Func<ValueTask> action)
    {
        var tcs = new TaskCompletionSource();

        _uiContext!.Post(async (e) =>
        {
            try
            {
                await action();

                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, null);

        return tcs.Task;
    }

    private static void RunSync(Func<Task> task)
    {
        var oldContext = SynchronizationContext.Current;
        var sync = new ExclusiveSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(sync);
        sync.Post(async _ =>
        {
            try
            {
                await task();
            }
            catch (Exception e)
            {
                sync.InnerException = e;
                throw;
            }
            finally
            {
                sync.EndMessageLoop();
            }
        }, null);
        sync.BeginMessageLoop();

        SynchronizationContext.SetSynchronizationContext(oldContext);
    }

    private static void RunSync(Func<ValueTask> task)
    {
        var oldContext = SynchronizationContext.Current;
        var sync = new ExclusiveSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(sync);
        sync.Post(async _ =>
        {
            try
            {
                await task();
            }
            catch (Exception e)
            {
                sync.InnerException = e;
                throw;
            }
            finally
            {
                sync.EndMessageLoop();
            }
        }, null);
        sync.BeginMessageLoop();

        SynchronizationContext.SetSynchronizationContext(oldContext);
    }

    private class ExclusiveSynchronizationContext : SynchronizationContext
    {
        private bool _done;
        private readonly AutoResetEvent _workItemsWaiting = new(false);
        private readonly Queue<Tuple<SendOrPostCallback, object?>> _items = [];

        public Exception? InnerException { get; set; }

        public override void Send(SendOrPostCallback d, object? state)
        {
            throw new NotSupportedException("Cannot send to the same thread");
        }

        public override void Post(SendOrPostCallback d, object? state)
        {
            lock (_items)
            {
                _items.Enqueue(Tuple.Create(d, state));
            }
            _workItemsWaiting.Set();
        }

        public void EndMessageLoop()
        {
            Post(_ => _done = true, null);
        }

        public void BeginMessageLoop()
        {
            while (!_done)
            {
                Tuple<SendOrPostCallback, object?>? task = null;
                lock (_items)
                {
                    if (_items.Count > 0)
                    {
                        task = _items.Dequeue();
                    }
                }
                if (task is not null)
                {
                    task.Item1(task.Item2);
                    if (InnerException is not null) // the method threw an exception
                    {
                        throw new AggregateException("ThreadUtil.Run method threw an exception.", InnerException);
                    }
                }
                else
                {
                    _workItemsWaiting.WaitOne();
                }
            }
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }
    }
}