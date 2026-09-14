using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projectTodoServer.Contracts.Requests;
using projectTodoServer.Contracts.Responses;
using projectTodoServer.Data;
using projectTodoServer.Entities;

namespace projectTodoServer.Controllers;

[ApiController]
[Route("api/collections/projects/records")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProjectsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> CreateProject(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            ModelState.AddModelError(
                nameof(request.Name),
                "Project name is required");

            return ValidationProblem(ModelState);
        }

        var now = DateTimeOffset.UtcNow;

        var project = new Project
        {
            Name = request.Name.Trim(),
            UserId = request.UserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Projects.Add(project);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(project);

        return CreatedAtAction(
            nameof(GetProjectById), // equal to "GetProjectById", but more safely
            new { id = project.Id },
            response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectResponse>> GetProjectById(
        string id,
        CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(
                project => project.Id == id,
                cancellationToken);

        if (project is null)
        {
            return NotFound(new
            {
                code = 404,
                message = "Project not found"
            });
        }

        return Ok(MapToResponse(project));
    }

    private static ProjectResponse MapToResponse(Project project)
    {
        return new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            UserId = project.UserId,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }
}