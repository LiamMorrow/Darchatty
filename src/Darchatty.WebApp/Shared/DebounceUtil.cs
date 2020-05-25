using System;
using System.Threading;
using System.Threading.Tasks;

namespace Darchatty.WebApp.Shared
{
    public static class DebounceUtil
    {
        public static Action Debounce(this Action func, int milliseconds = 300)
        {
            CancellationTokenSource? cancelTokenSource = null;

            return () =>
            {
                cancelTokenSource?.Cancel();
                cancelTokenSource = new CancellationTokenSource();

                _ = Task.Delay(milliseconds, cancelTokenSource.Token)
                    .ContinueWith(
                        t =>
                        {
                            if (t.IsCompletedSuccessfully)
                            {
                                func();
                            }
                        },
                        TaskScheduler.Default);
            };
        }
    }
}
