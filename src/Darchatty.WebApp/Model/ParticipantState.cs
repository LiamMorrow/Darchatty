using System;

namespace Darchatty.WebApp.Model
{
    public class ParticipantState
    {
        public ParticipantState(Guid participantId, string participantName)
        {
            ParticipantId = participantId;
            ParticipantName = participantName;
        }

        public Guid ParticipantId { get; }

        public string ParticipantName { get; }
    }
}
