using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Document
{
    public enum Label
    {
        Refresh,
        SelectProcess,
        NotSelectProcessMessage,
        CompareImage,
        ScreenCapture,
        FailCapture,
        SaveConfig,
        Config,
        EventType,
        EventDataSet,
        ConfigList,
        MouseCoordinates,
        Save,
        Delete,
        Start,
        Stop,

        Max
    }

    public enum Message
    {
        Success,

        FailedImageValidate,
        FailedMouseCoordinatesValidate,
        FailedKeyboardCommandValidate,
        FailedProcessValidate,
        FailedLoadSaveFile,

        Max
    }
}
