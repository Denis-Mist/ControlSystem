namespace ControlSystem.Models
{
    public enum DefectPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    public enum DefectStatus
    {
        New = 0,
        InProgress = 1,
        UnderReview = 2,
        Closed = 3,
        Cancelled = 4
    }
}
