namespace Utils.Document
{
    public enum Label
    {
        Title,
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
        TriggerList,
        MouseCoordinates,
        Save,
        Delete,
        Start,
        Stop,
        Setting,
        Language,
        SavePath,
        Period,
        ProcessDelay,
        Similarity,
        AfterDelay,
        VersionCheck,

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
        FailedOSVersion,
        FailedFileBroken,
        FailedPeriodValidate,
        FailedProcessDelayValidate,
        FailedSimilarityValidate,

        NewVersion,

        Max
    }
}
