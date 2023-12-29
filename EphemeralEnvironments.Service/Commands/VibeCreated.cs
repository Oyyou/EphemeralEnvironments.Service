namespace EphemeralEnvironments.Service.Commands
{
    public class VibeCreated
    {
        public string EventId { get; set; }
        public string Value { get; set; }
        public DateTime TimeAdded { get; set; }

        public VibeCreated(
            string eventId,
            string value,
            DateTime timeAdded)
        {
            EventId = eventId;
            Value = value;
            TimeAdded = timeAdded;
        }
    }
}
