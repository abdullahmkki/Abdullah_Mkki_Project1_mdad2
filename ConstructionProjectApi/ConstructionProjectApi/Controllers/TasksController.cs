using Microsoft.AspNetCore.Mvc;
using ConstructionProjectApi.Data;
using ConstructionProjectApi.Models;
using ConstructionProjectApi.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

namespace ConstructionProjectApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projects/{projectId}/tasks")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ConstructionProjectContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ConstructionProjectContext context, IMapper mapper, ILogger<TasksController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ProjectTaskDto>>> GetTasksForProject(int projectId)
        {
            try
            {
                _logger.LogInformation("Attempting to retrieve tasks for project ID: {ProjectId}", projectId);
                var project = await _context.ConstructionProjects.AnyAsync(p => p.Id == projectId);
                if (!project)
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found.", projectId);
                    return NotFound($"Project with ID {projectId} not found.");
                }

                var tasks = await _context.ProjectTasks
                                    .Where(t => t.ProjectId == projectId)
                                    .ToListAsync();
                return Ok(_mapper.Map<IEnumerable<ProjectTaskDto>>(tasks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for project ID: {ProjectId}.", projectId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

      
        [HttpGet("{taskId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProjectTaskDto>> GetTaskForProject(int projectId, int taskId)
        {
            try
            {
                _logger.LogInformation("Attempting to retrieve task ID: {TaskId} for project ID: {ProjectId}", taskId, projectId);
                var project = await _context.ConstructionProjects.AnyAsync(p => p.Id == projectId);
                if (!project)
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found.", projectId);
                    return NotFound($"Project with ID {projectId} not found.");
                }

                var task = await _context.ProjectTasks
                                .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);

                if (task == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found in project {ProjectId}.", taskId, projectId);
                    return NotFound();
                }

                return Ok(_mapper.Map<ProjectTaskDto>(task));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task ID: {TaskId} for project ID: {ProjectId}.", taskId, projectId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

       
        [HttpPost]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProjectTaskDto>> PostTaskForProject(int projectId, [FromBody] ProjectTaskDto taskDto)
        {
            try
            {
                _logger.LogInformation("Attempting to create a new task for project ID: {ProjectId}", projectId);
                var project = await _context.ConstructionProjects.AnyAsync(p => p.Id == projectId);
                if (!project)
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found.", projectId);
                    return NotFound($"Project with ID {projectId} not found.");
                }

                var task = _mapper.Map<ProjectTask>(taskDto);
                task.ProjectId = projectId;
                _context.ProjectTasks.Add(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Task with ID {TaskId} created successfully for project ID: {ProjectId}.", task.Id, projectId);
                return CreatedAtAction(nameof(GetTaskForProject), new { projectId = projectId, taskId = task.Id }, _mapper.Map<ProjectTaskDto>(task));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating a new task for project ID: {ProjectId}.", projectId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPut("{taskId}")]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutTaskForProject(int projectId, int taskId, [FromBody] ProjectTaskDto taskDto)
        {
            try
            {
                if (taskId != taskDto.Id)
                {
                    _logger.LogWarning("Mismatched task ID in URL and body. URL ID: {UrlId}, Body ID: {BodyId}", taskId, taskDto.Id);
                    return BadRequest();
                }

                _logger.LogInformation("Attempting to update task ID: {TaskId} for project ID: {ProjectId}", taskId, projectId);
                var project = await _context.ConstructionProjects.AnyAsync(p => p.Id == projectId);
                if (!project)
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found.", projectId);
                    return NotFound($"Project with ID {projectId} not found.");
                }

                var taskToUpdate = await _context.ProjectTasks
                                        .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);

                if (taskToUpdate == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found in project {ProjectId} for update.", taskId, projectId);
                    return NotFound();
                }

                _mapper.Map(taskDto, taskToUpdate);
                _context.Entry(taskToUpdate).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Task with ID {TaskId} updated successfully for project ID: {ProjectId}.", taskId, projectId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task ID: {TaskId} for project ID: {ProjectId}.", taskId, projectId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpDelete("{taskId}")]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTaskForProject(int projectId, int taskId)
        {
            try
            {
                _logger.LogInformation("Attempting to delete task ID: {TaskId} for project ID: {ProjectId}", taskId, projectId);
                var project = await _context.ConstructionProjects.AnyAsync(p => p.Id == projectId);
                if (!project)
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found.", projectId);
                    return NotFound($"Project with ID {projectId} not found.");
                }

                var task = await _context.ProjectTasks
                                .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);
                if (task == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found in project {ProjectId} for deletion.", taskId, projectId);
                    return NotFound();
                }

                _context.ProjectTasks.Remove(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Task with ID {TaskId} deleted successfully for project ID: {ProjectId}.", taskId, projectId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task ID: {TaskId} for project ID: {ProjectId}.", taskId, projectId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
