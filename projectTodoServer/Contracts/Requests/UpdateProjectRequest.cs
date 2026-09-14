namespace projectTodoServer.Contracts.Requests;
using System.ComponentModel.DataAnnotations;

public class UpdateProjectRequest
{
    [StringLength(200, MinimumLength = 1,ErrorMessage = "Project name must be between 1 and 200 characters")]
    public string? Name { get; set; }
}