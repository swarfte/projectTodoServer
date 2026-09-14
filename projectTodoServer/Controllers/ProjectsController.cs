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
    
    [HttpGet]
    public async Task<ActionResult<ProjectListResponse>> GetProjects(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 30,
        [FromQuery] string? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return BadRequest(new
            {
                code = 400,
                message = "page must be greater than or equal to 1."
            });
        }

        if (perPage < 1 || perPage > 100)
        {
            return BadRequest(new
            {
                code = 400,
                message = "perPage must be greater than or equal to 1 and less than 100."
            });
        }

        var query = _dbContext.Projects
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(project => project.UserId == userId);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var projects = await query
            .OrderByDescending(project => project.UpdatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(cancellationToken);

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)perPage);

        var response = new ProjectListResponse
        {
            Page = page,
            PerPage = perPage,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = projects
                .Select(MapToResponse)
                .ToList()
        };

        return Ok(response);
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