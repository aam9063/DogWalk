namespace DogWalk_Infrastructure.Services.FileStorage
{
    public class FileStorageOptions
    {
        public string BasePath { get; set; } = "uploads";
        public long MaxFileSizeBytes { get; set; } = 5242880; // 5MB
        public string[] AllowedExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png", ".gif" };
    }
}
