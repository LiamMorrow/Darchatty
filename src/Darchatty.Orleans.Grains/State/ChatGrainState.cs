using System;
using System.Collections.Generic;

namespace Darchatty.Orleans.Grains.State
{
    public class ChatGrainState
    {
        public int MessageCount { get; set; }

        public string? ChatName { get; set; }

        public HashSet<Guid>? Participants { get; set; }
    }
}
