namespace WebAPI.Models
{
    public class SaveImageRequest
    {
        public string PhotoFolderUrl { get; set; } = ""; // örn "uploads/owners"
        public string BaseFile { get; set; } = "";       // data URL ya da başka string
    }
}
