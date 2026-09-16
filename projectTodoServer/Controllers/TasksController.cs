using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projectTodoServer.Contracts.Requests;
using projectTodoServer.Contracts.Responses;
using projectTodoServer.Data;
using projectTodoServer.Entities;

namespace projectTodoServer.Controllers;

[ApiController]
[Route("api/collections/tasks/record")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public TasksController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> CreateTask(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                code = 400,
                message = "Task Name is required"
            });
        }

        if (string.IsNullOrWhiteSpace(request.ProjectId))
        {
            return BadRequest(new
            {
                code = 400,
                message = "Project Id is required"
            });
        }

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return BadRequest(new
            {
                code = 400,
                message = "User Id is required"
            });
        }

        var project = await _dbContext.Projects.AsNoTracking().FirstOrDefaultAsync(project => project.Id == request.ProjectId, cancellationToken);
        if (project is null)
        {
            return NotFound(new
            {
                code = 404,
                message = "Project not found"
            });
        }

        if (!string.Equals(project.UserId, request.UserId, StringComparison.Ordinal))
        {
            return BadRequest(new
            {
                code = 400,
                message = "Task user ID must match the project user ID"
            });
        }

        var now = DateTime.UtcNow;

        var taskItem = new TaskItem
        {
            Name = request.Name.Trim(),
            ProjectId = request.ProjectId.Trim(),
            UserId = request.UserId.Trim(),
            IsCompleted = request.IsCompleted,
            DueDate = NormalizeToUtc(request.DueDate),
            PreviousTaskId = NormalizeOptionalId(request.PreviousTaskId),
            CompletedAt = NormalizeToUtc(request.CompletedAt),
            IsFolded = request.IsFolded,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.TaskItems.Add(taskItem);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(
            nameof(GetTaskById),
            new { id = taskItem.Id },
            MapToResponse(taskItem)
        );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskResponse>> GetTaskById(string id, CancellationToken cancellationToken)
    {
        var taskItem = await _dbContext.TaskItems.AsNoTracking().FirstOrDefaultAsync(
            taskItem => taskItem.Id == id, cancellationToken);

        if (taskItem == null)
        {
            return NotFound(new
            {
                code = 404,
                message = "Task Not Found"
            });
        }
        return Ok(MapToResponse(taskItem));
    }



    private static TaskResponse MapToResponse
    (TaskItem taskItem)
    {
        return new TaskResponse
        {
            Id = taskItem.Id,
            Name = taskItem.Name,
            ProjectId = taskItem.ProjectId,
            UserId = taskItem.UserId,
            IsCompleted = taskItem.IsCompleted,
            DueDate = taskItem.DueDate,
            PreviousTaskId = taskItem.PreviousTaskId,
            CompletedAt = taskItem.CompletedAt,
            IsFolded = taskItem.IsFolded,
            CreatedAt = taskItem.CreatedAt,
            UpdatedAt = taskItem.UpdatedAt,
        };
    }

    private static string? NormalizeOptionalId(string? id)
    {
        return string.IsNullOrWhiteSpace(id) ? null : id.Trim();
    }


    private static DateTime? NormalizeToUtc(DateTime? value)
    {
        if (value is null)
        {
            return null;
        }

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        };
    }

    [HttpGet]
    public async Task<ActionResult<TaskListResponse>> GetTasks(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 30,
        [FromQuery] string? projectId = null,
        [FromQuery] string? previousTaskId = null,
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
                message = "perPage must be between 1 and 100."
            });
        }

        var query = _dbContext.TaskItems
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(projectId))
        {
            var normalizedProjectId = projectId.Trim();

            query = query.Where(
                taskItem => taskItem.ProjectId == normalizedProjectId);
        }

        if (!string.IsNullOrWhiteSpace(previousTaskId))
        {
            var normalizedPreviousTaskId = previousTaskId.Trim();

            query = query.Where(
                taskItem =>
                    taskItem.PreviousTaskId == normalizedPreviousTaskId);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var skip = (page - 1) * perPage;

        var taskItems = await query
            .OrderByDescending(taskItem => taskItem.UpdatedAt)
            .ThenByDescending(taskItem => taskItem.Id)
            .Skip(skip)
            .Take(perPage)
            .ToListAsync(cancellationToken);

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(
                totalItems / (double)perPage);

        var response = new TaskListResponse
        {
            Page = page,
            PerPage = perPage,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = taskItems
                .Select(MapToResponse)
                .ToList()
        };

        return Ok(response);
    }
}