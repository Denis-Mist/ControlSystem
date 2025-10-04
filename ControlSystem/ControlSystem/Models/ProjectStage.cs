using System;

namespace ControlSystem.Models
{
    public class ProjectStage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
