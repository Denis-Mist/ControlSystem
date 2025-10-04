using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ControlSystem.Services;
using ControlSystem.DTOs;
using ControlSystem.Models;

namespace ControlSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class ControlController : ControllerBase
    {
        private readonly DefectService _defectService;
        private readonly ProjectService _projectService;
        private readonly ReportService _reportService;

        public ControlController(
            DefectService defectService,
            ProjectService projectService,
            ReportService reportService)
        {
            _defectService = defectService;
            _projectService = projectService;
            _reportService = reportService;
        }

        // Helper to get current user's id from claims (null if anonymous)
        private string CurrentUserId =>
            User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        #region Defects

        // Create defect
        [HttpPost("defects")]
        [Authorize] // optional - require auth if needed
        public async Task<ActionResult<Defect>> CreateDefect([FromBody] CreateDefectDto dto)
        {
            if (dto == null) return BadRequest();
            var userId = CurrentUserId;
            var created = await _defectService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetDefect), new { id = created.Id }, created);
        }

        // Get defect by id (with includes)
        [HttpGet("defects/{id:guid}")]
        public async Task<ActionResult<Defect>> GetDefect(Guid id)
        {
            var d = await _defectService.GetByIdAsync(id);
            if (d == null) return NotFound();
            return Ok(d);
        }

        // Query/paged list
        [HttpGet("defects")]
        public async Task<ActionResult> QueryDefects([FromQuery] DefectFilterDto filter)
        {
            filter ??= new DefectFilterDto();
            var (items, total) = await _defectService.QueryAsync(filter);
            return Ok(new
            {
                Items = items,
                Total = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            });
        }

        // Update defect (partial via DTO)
        [HttpPut("defects/{id:guid}")]
        [Authorize]
        public async Task<ActionResult> UpdateDefect(Guid id, [FromBody] UpdateDefectDto dto)
        {
            if (dto == null) return BadRequest();
            var userId = CurrentUserId;
            var ok = await _defectService.UpdateAsync(id, dto, userId);
            if (!ok) return NotFound();
            return NoContent();
        }

        // Change status
        [HttpPost("defects/{id:guid}/status")]
        [Authorize]
        public async Task<ActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto)
        {
            if (dto == null) return BadRequest();
            var userId = CurrentUserId;
            var (Ok, Error) = await _defectService.ChangeStatusAsync(id, dto.NewStatus, userId);
            if (!Ok) return BadRequest(new { error = Error });
            return NoContent();
        }

        // Add comment
        [HttpPost("defects/{id:guid}/comments")]
        [Authorize]
        public async Task<ActionResult> AddComment(Guid id, [FromBody] CommentDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Text)) return BadRequest();
            var authorId = CurrentUserId;
            var c = await _defectService.AddCommentAsync(id, dto.Text, authorId);
            if (c == null) return NotFound();
            return CreatedAtAction(nameof(GetDefect), new { id = id }, c);
        }

        // Attach file
        [HttpPost("defects/{id:guid}/attachments")]
        [Authorize]
        [RequestSizeLimit(50_000_000)] // example: 50MB limit; adjust as needed
        public async Task<ActionResult> AttachFile(Guid id, IFormFile file)
        {
            if (file == null) return BadRequest(new { error = "File is required" });
            var uploaderId = CurrentUserId;
            var att = await _defectService.AttachFileAsync(id, file, uploaderId);
            if (att == null) return NotFound();
            return CreatedAtAction(nameof(GetDefect), new { id = id }, att);
        }

        #endregion

        #region Projects & Stages

        // Get all projects (with stages)
        [HttpGet("projects")]
        public async Task<ActionResult> GetProjects()
        {
            var all = await _projectService.GetAllAsync();
            return Ok(all);
        }

        // Get project by id
        [HttpGet("projects/{id:guid}")]
        public async Task<ActionResult> GetProject(Guid id)
        {
            var p = await _projectService.GetByIdAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        // Create project
        [HttpPost("projects")]
        [Authorize]
        public async Task<ActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            if (dto == null) return BadRequest();
            var p = await _projectService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetProject), new { id = p.Id }, p);
        }

        // Update project
        [HttpPut("projects/{id:guid}")]
        [Authorize]
        public async Task<ActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectDto dto)
        {
            if (dto == null) return BadRequest();
            var ok = await _projectService.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        // Delete project
        [HttpDelete("projects/{id:guid}")]
        [Authorize]
        public async Task<ActionResult> DeleteProject(Guid id)
        {
            var ok = await _projectService.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // Add stage to project
        [HttpPost("projects/stages")]
        [Authorize]
        public async Task<ActionResult> AddStage([FromBody] CreateStageDto dto)
        {
            if (dto == null) return BadRequest();
            var st = await _projectService.AddStageAsync(dto);
            if (st == null) return NotFound(new { error = "Project not found" });
            return CreatedAtAction(nameof(GetProject), new { id = dto.ProjectId }, st);
        }

        #endregion

        #region Reports / Analytics

        // Export defects filtered to CSV
        [HttpGet("reports/defects/export")]
        public async Task<IActionResult> ExportDefectsToCsv([FromQuery] DefectFilterDto filter)
        {
            filter ??= new DefectFilterDto();
            var ms = await _reportService.ExportDefectsToCsvAsync(filter);
            ms.Position = 0;
            // Use "text/csv" and suggest filename
            return File(ms, "text/csv", $"defects_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }

        // Counts by status
        [HttpGet("reports/counts/status")]
        public async Task<ActionResult> CountsByStatus()
        {
            var res = await _reportService.GetCountsByStatusAsync();
            return Ok(res);
        }

        // Counts by priority
        [HttpGet("reports/counts/priority")]
        public async Task<ActionResult> CountsByPriority()
        {
            var res = await _reportService.GetCountsByPriorityAsync();
            return Ok(res);
        }

        // Created trend: accepts from/to as query params (ISO format or date)
        [HttpGet("reports/trend/created")]
        public async Task<ActionResult> CreatedTrend([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            // default last 30 days if not provided
            var toDt = to ?? DateTime.UtcNow;
            var fromDt = from ?? toDt.AddDays(-30);
            if (fromDt > toDt) return BadRequest(new { error = "'from' must be <= 'to'" });

            var res = await _reportService.GetCreatedTrendAsync(fromDt, toDt);
            return Ok(res);
        }

        #endregion
    }
}
