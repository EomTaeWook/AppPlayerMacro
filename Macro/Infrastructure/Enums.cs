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
        ScreenCaptureDataBind,
        TreeItemOrderChanged,
        SelctTreeViewItemChanged,
        EventTriggerOrderChanged,
        EventTriggerInserted,
        EventTriggerRemoved,

        Save,
        Delete,

        TreeGridViewFocus,

        UpdatedTime,
        ROICaptureDataBind,

        Max
    }

    public enum RepeatType
    {
        Once,
        Count,
        NoSearchChild,
        SearchParent,

        Max
    }

    public enum ConditionType
    {
        Above,
        Below,

        Max
    }

    public enum CaptureModeType
    {
        ImageCapture,
        ROICapture,

        Max
    }

    public enum InitialTab
    {
        Common,
        //Game,

        Max
    }
}
