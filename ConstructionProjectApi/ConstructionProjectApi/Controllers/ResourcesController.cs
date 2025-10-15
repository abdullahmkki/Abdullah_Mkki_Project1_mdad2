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
    [Route("api/v{version:apiVersion}/resources")]
    [Authorize]
    public class ResourcesController : ControllerBase
    {
        private readonly ConstructionProjectContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ResourcesController> _logger;

        public ResourcesController(ConstructionProjectContext context, IMapper mapper, ILogger<ResourcesController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ResourceDto>>> GetResources()
        {
            try
            {
                _logger.LogInformation("Attempting to retrieve all resources.");
                var resources = await _context.Resources.ToListAsync();
                return Ok(_mapper.Map<IEnumerable<ResourceDto>>(resources));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resources.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResourceDto>> GetResource(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to retrieve resource with ID: {ResourceId}", id);
                var resource = await _context.Resources.FindAsync(id);

                if (resource == null)
                {
                    _logger.LogWarning("Resource with ID {ResourceId} not found.", id);
                    return NotFound();
                }

                return Ok(_mapper.Map<ResourceDto>(resource));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resource with ID: {ResourceId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResourceDto>> PostResource([FromBody] ResourceDto resourceDto)
        {
            try
            {
                _logger.LogInformation("Attempting to create a new resource.");
                var resource = _mapper.Map<Resource>(resourceDto);
                _context.Resources.Add(resource);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Resource with ID {ResourceId} created successfully.", resource.Id);
                return CreatedAtAction(nameof(GetResource), new { id = resource.Id }, _mapper.Map<ResourceDto>(resource));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating a new resource.");
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
        public async Task<IActionResult> PutResource(int id, [FromBody] ResourceDto resourceDto)
        {
            try
            {
                if (id != resourceDto.Id)
                {
                    _logger.LogWarning("Mismatched resource ID in URL and body. URL ID: {UrlId}, Body ID: {BodyId}", id, resourceDto.Id);
                    return BadRequest();
                }

                _logger.LogInformation("Attempting to update resource with ID: {ResourceId}", id);
                var resourceToUpdate = await _context.Resources.FindAsync(id);

                if (resourceToUpdate == null)
                {
                    _logger.LogWarning("Resource with ID {ResourceId} not found for update.", id);
                    return NotFound();
                }

                _mapper.Map(resourceDto, resourceToUpdate);
                _context.Entry(resourceToUpdate).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Resource with ID {ResourceId} updated successfully.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating resource with ID: {ResourceId}.", id);
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
        public async Task<IActionResult> DeleteResource(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete resource with ID: {ResourceId}", id);
                var resource = await _context.Resources.FindAsync(id);
                if (resource == null)
                {
                    _logger.LogWarning("Resource with ID {ResourceId} not found for deletion.", id);
                    return NotFound();
                }

                _context.Resources.Remove(resource);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Resource with ID {ResourceId} deleted successfully.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting resource with ID: {ResourceId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
