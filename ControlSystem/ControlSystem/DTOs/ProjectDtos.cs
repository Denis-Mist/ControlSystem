using System;

namespace ControlSystem.DTOs
{
    public class CreateProjectDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UpdateProjectDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CreateStageDto
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; }
    }
}
