using System;
using System.Collections.Generic;

namespace Darchatty.Orleans.GrainInterfaces.Model
{
    public class ChatDetails
    {
        public Guid ChatId { get; set; }

        public string ChatName { get; set; } = null!;

        public HashSet<Guid> Participants { get; set; } = null!;
    }
}
