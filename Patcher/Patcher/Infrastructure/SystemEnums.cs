namespace Patcher.Infrastructure
{
    public enum Language
    {
        Kor,
        Eng,

        Max
    }
    public enum Label
    {
        Cancel,
        Rollback,
        Download,
        SearchPatchList,
        Patching,

        CancelPatch,

        Max
    }
    public enum Message
    {
        CancelPatch,

        Max
    }

    public interface IDocumentData
    {
        string Code { get; }
    }

    public class DocumentData : IDocumentData
    {
        public string Code { get; set; }
        public string Kor { get; set; }
        public string Eng { get; set; }
    }

    public interface IDocument
    {
        void Init(string path);
    }
}
