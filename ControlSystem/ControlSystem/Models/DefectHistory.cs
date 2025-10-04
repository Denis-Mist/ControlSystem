using System;
using YourProject.Models;

namespace ControlSystem.Models
{
    public class DefectHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DefectId { get; set; }
        public Defect Defect { get; set; }

        public string ChangedById { get; set; }
        public ApplicationUser ChangedBy { get; set; }

        public string Field { get; set; } // e.g., "Status", "AssignedTo", "Title"
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
