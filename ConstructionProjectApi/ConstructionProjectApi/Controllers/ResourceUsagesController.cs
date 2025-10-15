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
    [Route("api/v{version:apiVersion}/tasks/{taskId}/usages")]
    [Authorize]
    public class ResourceUsagesController : ControllerBase
    {
        private readonly ConstructionProjectContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ResourceUsagesController> _logger;

        public ResourceUsagesController(ConstructionProjectContext context, IMapper mapper, ILogger<ResourceUsagesController> logger)
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
        public async Task<ActionResult<IEnumerable<ResourceUsageDto>>> GetResourceUsagesForTask(int taskId)
        {
            try
            {
                _logger.LogInformation("Attempting to retrieve resource usages for task ID: {TaskId}", taskId);
                var taskExists = await _context.ProjectTasks.AnyAsync(t => t.Id == taskId);
                if (!taskExists)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found.", taskId);
                    return NotFound($"Task with ID {taskId} not found.");
                }

                var resourceUsages = await _context.ResourceUsages
                                            .Where(ru => ru.TaskId == taskId)
                                            .ToListAsync();
                return Ok(_mapper.Map<IEnumerable<ResourceUsageDto>>(resourceUsages));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resource usages for task ID: {TaskId}.", taskId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResourceUsageDto>> GetResourceUsageForTask(int taskId, int id)
        {
            try
            {
                _logger.LogInformation("Attempting to retrieve resource usage ID: {ResourceUsageId} for task ID: {TaskId}", id, taskId);
                var taskExists = await _context.ProjectTasks.AnyAsync(t => t.Id == taskId);
                if (!taskExists)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found.", taskId);
                    return NotFound($"Task with ID {taskId} not found.");
                }

                var resourceUsage = await _context.ResourceUsages
                                            .FirstOrDefaultAsync(ru => ru.TaskId == taskId && ru.Id == id);

                if (resourceUsage == null)
                {
                    _logger.LogWarning("Resource usage with ID {ResourceUsageId} not found for task {TaskId}.", id, taskId);
                    return NotFound();
                }

                return Ok(_mapper.Map<ResourceUsageDto>(resourceUsage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resource usage ID: {ResourceUsageId} for task ID: {TaskId}.", id, taskId);
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
        public async Task<ActionResult<ResourceUsageDto>> PostResourceUsageForTask(int taskId, [FromBody] ResourceUsageDto resourceUsageDto)
        {
            try
            {
                _logger.LogInformation("Attempting to create a new resource usage for task ID: {TaskId}", taskId);
                var taskExists = await _context.ProjectTasks.AnyAsync(t => t.Id == taskId);
                if (!taskExists)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found.", taskId);
                    return NotFound($"Task with ID {taskId} not found.");
                }

                var resourceUsage = _mapper.Map<ResourceUsage>(resourceUsageDto);
                resourceUsage.TaskId = taskId; 
                _context.ResourceUsages.Add(resourceUsage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Resource usage with ID {ResourceUsageId} created successfully for task ID: {TaskId}.", resourceUsage.Id, taskId);
                return CreatedAtAction(nameof(GetResourceUsageForTask), new { taskId = taskId, id = resourceUsage.Id }, _mapper.Map<ResourceUsageDto>(resourceUsage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating a new resource usage for task ID: {TaskId}.", taskId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutResourceUsageForTask(int taskId, int id, [FromBody] ResourceUsageDto resourceUsageDto)
        {
            try
            {
                if (id != resourceUsageDto.Id)
                {
                    _logger.LogWarning("Mismatched resource usage ID in URL and body. URL ID: {UrlId}, Body ID: {BodyId}", id, resourceUsageDto.Id);
                    return BadRequest();
                }

                _logger.LogInformation("Attempting to update resource usage ID: {ResourceUsageId} for task ID: {TaskId}", id, taskId);
                var taskExists = await _context.ProjectTasks.AnyAsync(t => t.Id == taskId);
                if (!taskExists)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found.", taskId);
                    return NotFound($"Task with ID {taskId} not found.");
                }

                var resourceUsageToUpdate = await _context.ResourceUsages
                                                .FirstOrDefaultAsync(ru => ru.TaskId == taskId && ru.Id == id);

                if (resourceUsageToUpdate == null)
                {
                    _logger.LogWarning("Resource usage with ID {ResourceUsageId} not found for task {TaskId} for update.", id, taskId);
                    return NotFound();
                }

                _mapper.Map(resourceUsageDto, resourceUsageToUpdate);
                _context.Entry(resourceUsageToUpdate).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Resource usage with ID {ResourceUsageId} updated successfully for task ID: {TaskId}.", id, taskId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating resource usage ID: {ResourceUsageId} for task ID: {TaskId}.", id, taskId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteResourceUsageForTask(int taskId, int id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete resource usage ID: {ResourceUsageId} for task ID: {TaskId}", id, taskId);
                var taskExists = await _context.ProjectTasks.AnyAsync(t => t.Id == taskId);
                if (!taskExists)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found.", taskId);
                    return NotFound($"Task with ID {taskId} not found.");
                }

                var resourceUsage = await _context.ResourceUsages
                                            .FirstOrDefaultAsync(ru => ru.TaskId == taskId && ru.Id == id);
                if (resourceUsage == null)
                {
                    _logger.LogWarning("Resource usage with ID {ResourceUsageId} not found for task {TaskId} for deletion.", id, taskId);
                    return NotFound();
                }

                _context.ResourceUsages.Remove(resourceUsage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Resource usage with ID {ResourceUsageId} deleted successfully for task ID: {TaskId}.", id, taskId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting resource usage ID: {ResourceUsageId} for task ID: {TaskId}.", id, taskId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
