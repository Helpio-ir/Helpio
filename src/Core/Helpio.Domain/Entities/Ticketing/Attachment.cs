using System;
using System.Collections.Generic;

namespace Helpio.Ir.Domain.Entities.Ticketing
{
    public class Attachment : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long Size { get; set; }
        public string? Description { get; set; }
        
        // Navigation properties
        public virtual ICollection<AttachmentNote> AttachmentNotes { get; set; } = new List<AttachmentNote>();
        public virtual ICollection<AttachmentResponse> AttachmentResponses { get; set; } = new List<AttachmentResponse>();
    }
}