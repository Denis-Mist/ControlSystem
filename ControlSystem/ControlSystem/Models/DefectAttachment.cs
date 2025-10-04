using System;

namespace ControlSystem.Models
{
    public class DefectAttachment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DefectId { get; set; }
        public Defect Defect { get; set; }

        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string UploadedById { get; set; }
    }
}
