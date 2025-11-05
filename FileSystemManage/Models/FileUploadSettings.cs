namespace FileSystemManage.Models
{
    public class FileUploadSettings
    {
        public long LargeFileThreshold { get; set; }
        public long ChunkSize { get; set; }
    }
}