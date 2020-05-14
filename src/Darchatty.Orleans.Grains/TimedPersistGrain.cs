using System;
using System.Threading.Tasks;
using Orleans;

namespace Darchatty.Orleans.Grains
{
    public abstract class TimedPersistGrain<T> : Grain<T>
    {
        protected bool Dirty { get; set; }

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            RegisterTimer(WriteStateIfDirtyAsync, null!, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public override async Task OnDeactivateAsync()
        {
            await WriteStateIfDirtyAsync(null);
            await OnDeactivateAsync();
        }

        protected Task WriteStateIfDirtyAsync(object? ignore)
        {
            if (!Dirty)
            {
                return Task.CompletedTask;
            }

            return WriteStateAsync();
        }
    }
}
