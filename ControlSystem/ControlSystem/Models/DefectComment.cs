using System;
using YourProject.Models;

namespace ControlSystem.Models
{
    public class DefectComment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DefectId { get; set; }
        public Defect Defect { get; set; }

        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }

        public string Text { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
