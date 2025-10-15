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
    [Route("api/v{version:apiVersion}/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly ConstructionProjectContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(ConstructionProjectContext context, IMapper mapper, ILogger<ProjectsController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all construction projects.
        /// </summary>
        /// <returns>A list of construction projects.</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ConstructionProjectDto>>> GetProjects()
        {
            try
            {
                _logger.LogInformation("Attempting to retrieve all construction projects.");
                var projects = await _context.ConstructionProjects.ToListAsync();
                return Ok(_mapper.Map<IEnumerable<ConstructionProjectDto>>(projects));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving construction projects.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieves a specific construction project by ID.
        /// </summary>
        /// <param name="id">The ID of the project.</param>
        /// <returns>The construction project with the specified ID.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ConstructionProjectDto>> GetProject(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to retrieve project with ID: {ProjectId}", id);
                var project = await _context.ConstructionProjects.FindAsync(id);

                if (project == null)
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found.", id);
                    return NotFound();
                }

                return Ok(_mapper.Map<ConstructionProjectDto>(project));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project with ID: {ProjectId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Creates a new construction project.
        /// </summary>
        /// <param name="projectDto">The project data.</param>
        /// <returns>The newly created project.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admins can create projects
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ConstructionProjectDto>> PostProject([FromBody] ConstructionProjectDto projectDto)
        {
            try
            {
                _logger.LogInformation("Attempting to create a new project.");
                var project = _mapper.Map<ConstructionProject>(projectDto);
                _context.ConstructionProjects.Add(project);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Project with ID {ProjectId} created successfully.", project.Id);
                return CreatedAtAction(nameof(GetProject), new { id = project.Id }, _mapper.Map<ConstructionProjectDto>(project));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating a new project.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Updates an existing construction project.
        /// </summary>
        /// <param name="id">The ID of the project to update.</param>
        /// <param name="projectDto">The updated project data.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only Admins can update projects
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutProject(int id, [FromBody] ConstructionProjectDto projectDto)
        {
            try
            {
                if (id != projectDto.Id)
                {
                    _logger.LogWarning("Mismatched project ID in URL and body. URL ID: {UrlId}, Body ID: {BodyId}", id, projectDto.Id);
                    return BadRequest();
                }

                _logger.LogInformation("Attempting to update project with ID: {ProjectId}", id);
                var projectToUpdate = await _context.ConstructionProjects.FindAsync(id);

                if (projectToUpdate == null)
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found for update.", id);
                    return NotFound();
                }

                _mapper.Map(projectDto, projectToUpdate);
                _context.Entry(projectToUpdate).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Project with ID {ProjectId} updated successfully.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project with ID: {ProjectId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Deletes a specific construction project.
        /// </summary>
        /// <param name="id">The ID of the project to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admins can delete projects
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete project with ID: {ProjectId}", id);
                var project = await _context.ConstructionProjects.FindAsync(id);
                if (project == null)
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found for deletion.", id);
                    return NotFound();
                }

                _context.ConstructionProjects.Remove(project);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Project with ID {ProjectId} deleted successfully.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project with ID: {ProjectId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
