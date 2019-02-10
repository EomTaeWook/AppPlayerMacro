namespace Utils.Document
{
    public enum Language
    {
        Kor,
        Eng,

        Max
    }

    public enum Label
    {
        Title,
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
        ItemDelay,
        Similarity,
        AfterDelay,
        SearchResultDisplay,
        VersionCheck,
        Refresh,
        Fix,
        RepeatSubItems,

        RepeatOnce,
        RepeatCount,
        RepeatNoSearch,
        RepeatContinue,

        Cancel,
        Rollback,
        Download,
        SearchPatchList,
        Patching,

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
        CancelPatch,

        Max
    }
}
