using System;
using System.Collections.Generic;

namespace Darchatty.Web.Model
{
    public class ChatDto
    {
        public Guid ChatId { get; set; }

        public string ChatName { get; set; } = null!;

        public HashSet<Guid> ParticipantIds { get; set; } = null!;
    }
}
