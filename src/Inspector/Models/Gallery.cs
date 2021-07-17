using NativeMedia;
using System.IO;

namespace Inspector.Models
{
    public class Gallery
    {
        public IMediaFile File { get; set; }
        public MediaFile Attachment { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public enum FileType
    {
        Image,
        Video
    }

    public class MediaFile
    {
        public Stream Data { get; set; }
        public FileType Type { get; set; }
        public string FileName { get; set; }
    }
}
