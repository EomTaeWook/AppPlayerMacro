using Utils.Document;

namespace Macro.Infrastructure
{
    public class ApplicationData : IDocumentData
    {
        public string Code { get; set; }
        public bool IsDynamic { get; set; } = false;
        public string HandleName { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
    }
}
