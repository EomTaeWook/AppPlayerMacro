namespace Macro.Models
{
    internal class ProcessResultModel
    {
        public bool Excuted = false;
        public int DelayMillisecond { get; set; }
        public EventTriggerModel NextExcuteModel { get; set; }
    }
}
