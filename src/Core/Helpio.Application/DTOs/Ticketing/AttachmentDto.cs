using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.DTOs.Ticketing
{
    public class AttachmentDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long Size { get; set; }
        public string? Description { get; set; }
        
        // Computed Properties
        public string SizeFormatted => FormatFileSize(Size);
        public string FileExtension => Path.GetExtension(Name);
        public bool IsImage => IsImageFile(Type);
        
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
        
        private static bool IsImageFile(string contentType)
        {
            return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class CreateAttachmentDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long Size { get; set; }
        public string? Description { get; set; }
    }
}