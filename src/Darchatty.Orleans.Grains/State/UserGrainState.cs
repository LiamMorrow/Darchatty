using System;
using System.Collections.Generic;

namespace Darchatty.Orleans.Grains.State
{
    public class UserGrainState
    {
        public string? Name { get; set; }

        public HashSet<Guid>? ParticipatingChats { get; set; } = null!;
    }
}
