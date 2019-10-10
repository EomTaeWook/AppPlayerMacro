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
        MouseWheel,
        WheelData,
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
        RepeatSearch,

        Cancel,
        Rollback,
        Download,
        SearchPatchList,
        Patching,
        AddSameContent,
        TriggerToNext,

        FailedPatch,
        Common,
        Game,
        HpCondition,
        MpCondition,

        Above,
        Below,

        Max
    }

    public enum Message
    {
        Success,

        FailedInvalidateData,
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
        FailedSaveFile,

        NewVersion,
        CancelPatch,
        FailedPatchUpdate,

        Max
    }
}
