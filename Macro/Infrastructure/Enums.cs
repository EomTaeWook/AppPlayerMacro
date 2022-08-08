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
        ComboProcessChanged,

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

        TreeGridViewFocus,

        UpdatedTime,

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

    public enum ConditionType
    {
        Above,
        Below,
        
        Max
    }

    public enum InitialTab
    {
        Common,
        //Game,

        Max
    }
}
