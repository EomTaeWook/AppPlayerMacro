namespace Utils.Infrastructure
{
    public enum PROCESS_DPI_AWARENESS
    {
        PROCESS_DPI_UNAWARE = 0,
        PROCESS_SYSTEM_DPI_AWARE = 1,
        PROCESS_PER_MONITOR_DPI_AWARE = 2,

        PROCESS_DPI_AWARENESS_CONTEXT_UNAWARE = 16,
        PROCESS_DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = 17,
        PROCESS_DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = 18,
        PROCESS_DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = 34
    }
    
    public enum DpiFlags
    {
        Effective = 0,
        Angular = 1,
        Raw = 2,
    }
}
