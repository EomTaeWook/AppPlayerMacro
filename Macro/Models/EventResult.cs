namespace Macro.Models
{
    internal class EventResult
    {
        public bool IsSuccess { get; private set; }
        public EventTriggerModel NextEventTrigger { get; private set; }

        public EventResult(bool success, EventTriggerModel nextEventTrigger)
        {
            IsSuccess = success;
            NextEventTrigger = nextEventTrigger;
        }
    }
}
