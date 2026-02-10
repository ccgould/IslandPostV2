using System;
using System.Threading.Tasks;

namespace IslandPostPOS.Shared.Helpers
{
    public class DebounceDispatcher
    {
        private System.Threading.CancellationTokenSource _cts = null;

        public async Task DebounceAsync(Func<Task> action, int delayMs = 300)
        {
            _cts?.Cancel();
            _cts = new System.Threading.CancellationTokenSource();

            try
            {
                await Task.Delay(delayMs, _cts.Token);
                await action();
            }
            catch (TaskCanceledException)
            {
                // Ignore — another keystroke reset the timer
            }
        }

    }
}
