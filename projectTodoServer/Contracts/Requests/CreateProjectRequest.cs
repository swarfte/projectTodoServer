namespace projectTodoServer.Contracts.Requests;

using System.ComponentModel.DataAnnotations;

public class CreateProjectRequest
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, MinimumLength =1, ErrorMessage =  "Project name cannot be more than 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; } = string.Empty;
}