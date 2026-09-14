namespace projectTodoServer.Contracts.Responses;

public class ProjectListResponse
{
    public int Page { get; set; }
    public int PerPage { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public List<ProjectResponse> Items { get; set; } = [];
}