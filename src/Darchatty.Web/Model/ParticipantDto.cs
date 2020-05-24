using System;

namespace Darchatty.Web.Model
{
    public class ParticipantDto
    {
        public Guid ParticipantId { get; set; }

        public string ParticipantName { get; set; } = null!;
    }
}
