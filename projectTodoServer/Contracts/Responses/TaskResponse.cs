namespace projectTodoServer.Contracts.Responses;

public class TaskResponse
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ProjectId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public DateTime? DueDate { get; set; }

    public string? PreviousTaskId { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool IsFolded { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}