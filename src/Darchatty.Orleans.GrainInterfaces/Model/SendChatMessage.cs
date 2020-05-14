using System;

namespace Darchatty.Orleans.GrainInterfaces.Model
{
    public class SendChatMessage
    {
        public Guid SenderUserId { get; set; }

        public string MessageContentsRaw { get; set; } = null!;
    }
}
