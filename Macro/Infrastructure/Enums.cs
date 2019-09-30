namespace Macro.Infrastructure
{
    public enum EventType
    {
        Mouse,
        Keyboard,
        Image,
        RelativeToImage,

        Max
    }
    public enum MouseEventType
    {
        None,
        LeftClick,
        RightClick,
        Drag,
        Wheel,

        Max
    }

    public enum NotifyEventType
    {
        ConfigChanged,
        MousePointDataBind,
        ScreenCaptureDataBInd,
        TreeItemOrderChanged,
        SelctTreeViewItemChanged,
        EventTriggerOrderChanged,
        EventTriggerInserted,
        EventTriggerRemoved,

        Save,
        Delete,

        Max
    }

    public enum RepeatType
    {
        Once,
        Count,
        NoSearch,
        Search,

        Max
    }
}
