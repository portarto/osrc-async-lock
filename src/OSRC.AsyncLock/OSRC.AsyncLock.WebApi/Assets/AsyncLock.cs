namespace OSRC.AsyncLock.WebApi.Assets
{
    public class AsyncLock
    {
        private class Releaser : IDisposable
        {
            private bool disposedValue;

            private SemaphoreSlim Locker { get; }

            public Releaser(SemaphoreSlim locker)
            {
                Locker = locker;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        Console.WriteLine("Releasing locker");
                        Locker.Release();
                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        private SemaphoreSlim AsyncLocker { get; } = new SemaphoreSlim(1, 1);

        public async Task<IDisposable> AwaitAsync(CancellationToken cancellationToken)
        {
            await AsyncLocker.WaitAsync(cancellationToken);
            return new Releaser(AsyncLocker);
        }
    }
}
