using System;

namespace Darchatty.WebApp.Model
{
    public class ParticipantState : IEquatable<ParticipantState>
    {
        public ParticipantState(Guid participantId, string participantName)
        {
            ParticipantId = participantId;
            ParticipantName = participantName;
        }

        public Guid ParticipantId { get; }

        public string ParticipantName { get; }

        public override bool Equals(object? obj)
        {
            return obj is ParticipantState other && Equals(other);
        }

        public bool Equals(ParticipantState other)
        {
            return ParticipantId.Equals(other.ParticipantId) &&
                   ParticipantName == other.ParticipantName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ParticipantId, ParticipantName);
        }
    }
}
