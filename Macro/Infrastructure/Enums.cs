﻿namespace Macro.Infrastructure
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
        GameSave,
        Delete,
        GameDelete,

        TreeGridViewFocus,

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

    public enum CaptureViewMode
    {
        Common,
        Game,
        HP,
        Mp,

        Max
    }
    public enum MousePointViewMode
    {
        Common,
        Game,

        Max
    }
    public enum InitialTab
    {
        Common,
        Game,

        Max
    }

}
