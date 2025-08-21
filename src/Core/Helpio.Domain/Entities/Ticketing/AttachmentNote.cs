using System;

namespace Helpio.Ir.Domain.Entities.Ticketing
{
    public class AttachmentNote : BaseEntity
    {
        public int AttachmentId { get; set; }
        public int NoteId { get; set; }
        
        // Navigation properties
        public virtual Attachment Attachment { get; set; } = null!;
        public virtual Note Note { get; set; } = null!;
    }
}