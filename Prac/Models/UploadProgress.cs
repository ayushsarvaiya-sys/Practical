namespace Prac.Models
{
    public class UploadProgress
    {
        public string FileId { get; set; } = "";
        public string FileName { get; set; } = "";
        public int Progress { get; set; }
        public string Status { get; set; } = "";
    }
}
