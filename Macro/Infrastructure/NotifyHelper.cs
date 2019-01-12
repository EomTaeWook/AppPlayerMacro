using System;

namespace Macro.Infrastructure
{
    public class NotifyHelper
    {
        public static event Action<ConfigEventArgs> ConfigChanged;
        public static event Action<MousePointEventArgs> MousePositionDataBind;
        public static event Action<CaptureEventArgs> ScreenCaptureDataBind;

        public static void InvokeNotify(EventType eventType, INotifyEventArgs args)
        {
            switch(eventType)
            {
                case EventType.ConfigChanged:
                    ConfigChanged? .Invoke(args as ConfigEventArgs);
                    break;
                case EventType.MousePointDataBind:
                    MousePositionDataBind? .Invoke(args as MousePointEventArgs);
                    break;
                case EventType.ScreenCapture:
                    ScreenCaptureDataBind? .Invoke(args as CaptureEventArgs);
                    break;

            }
        }
    }
}
