using ControlSystem.Models;
using System;

namespace ControlSystem.DTOs
{
    public class CreateDefectDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DefectPriority Priority { get; set; } = DefectPriority.Medium;
        public Guid ProjectId { get; set; }
        public Guid? StageId { get; set; }
        public string AssignedToId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UpdateDefectDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DefectPriority? Priority { get; set; }
        public Guid? StageId { get; set; }
        public string AssignedToId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class ChangeStatusDto
    {
        public DefectStatus NewStatus { get; set; }
    }

    public class CommentDto
    {
        public string Text { get; set; }
    }

    public class DefectFilterDto
    {
        public Guid? ProjectId { get; set; }
        public DefectStatus? Status { get; set; }
        public DefectPriority? Priority { get; set; }
        public string AssignedToId { get; set; }
        public string Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDir { get; set; } = "desc";
    }
}
