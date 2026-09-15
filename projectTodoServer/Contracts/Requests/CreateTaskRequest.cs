using System.ComponentModel.DataAnnotations;

namespace projectTodoServer.Contracts.Requests;

public class CreateTaskRequest
{
    [Required(ErrorMessage = "Task name is required.")]
    [StringLength(200,MinimumLength = 1,ErrorMessage = "Task name must be between 1 and 200 characters long.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Project ID is required.")]
    public string ProjectId { get; set; } = string.Empty;

    [Required(ErrorMessage = "User id is required.")]
    public string UserId { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public DateTime? DueDate { get; set; }

    public string? PreviousTaskId { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool IsFolded { get; set; }
}