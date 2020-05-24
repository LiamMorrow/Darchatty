using System;

namespace Darchatty.Web.Model
{
    public class ChatMessageDto
    {
        public int ChatMessageNumber { get; set; }

        public Guid SenderUserId { get; set; }

        public DateTimeOffset SendTime { get; set; }

        public string MessageContentsRaw { get; set; } = null!;
    }
}
