using System.IO;

namespace eFMS.API.Common.Models
{
    public class EmailAttachment
    {
        public MemoryStream Stream { get; set; }
        public string FileName { get; set; }
        public FileAttachType FileType { get; set; }
    }


    public enum FileAttachType
    {
        Pdf,
        Excel2013,
        Excel2017,
        Word2013,
        Word2017,
        Word,
        PowerPoint,
        Txt
    }
}
