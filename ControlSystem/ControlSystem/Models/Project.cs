using System;
using System.Collections.Generic;

namespace ControlSystem.Models
{
    public class Project
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProjectStage> Stages { get; set; } = new List<ProjectStage>();
        public ICollection<Defect> Defects { get; set; } = new List<Defect>();
    }
}
