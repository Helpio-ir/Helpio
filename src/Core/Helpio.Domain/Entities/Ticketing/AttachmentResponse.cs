using System;

namespace Helpio.Ir.Domain.Entities.Ticketing
{
    public class AttachmentResponse : BaseEntity
    {
        public int AttachmentId { get; set; }
        public int ResponseId { get; set; }
        
        // Navigation properties
        public virtual Attachment Attachment { get; set; } = null!;
        public virtual Response Response { get; set; } = null!;
    }
}