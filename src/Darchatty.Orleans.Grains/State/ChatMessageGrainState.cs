using System;

namespace Darchatty.Orleans.Grains.State
{
    public class ChatMessageGrainState
    {
        public int ChatMessageNumber { get; set; }

        public Guid SenderUserId { get; set; }

        public DateTimeOffset SendTime { get; set; }

        public string? MessageContentsRaw { get; set; }
    }
}
