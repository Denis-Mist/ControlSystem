using System;
using System.Collections.Generic;
using YourProject.Models;

namespace ControlSystem.Models
{
    public class Defect
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public DefectPriority Priority { get; set; } = DefectPriority.Medium;
        public DefectStatus Status { get; set; } = DefectStatus.New;

        public Guid ProjectId { get; set; }
        public Project Project { get; set; }

        public Guid? StageId { get; set; }
        public ProjectStage Stage { get; set; }

        public string AssignedToId { get; set; } // FK to ApplicationUser Id
        public ApplicationUser AssignedTo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }

        public ICollection<DefectAttachment> Attachments { get; set; } = new List<DefectAttachment>();
        public ICollection<DefectComment> Comments { get; set; } = new List<DefectComment>();
        public ICollection<DefectHistory> History { get; set; } = new List<DefectHistory>();
    }
}
