using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobApplicationTracker.Data;
using JobApplicationTracker.Models;
using JobApplicationTracker.Services;

namespace JobApplicationTracker.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IJobScraperService _scraperService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(ApplicationDbContext context, IJobScraperService scraperService, ILogger<JobsController> logger)
    {
        _context = context;
        _scraperService = scraperService;
        _logger = logger;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobApplication>>> GetAll()
    {
        try
        {
            var jobs = await _context.JobApplications
                .Where(j => j.UserId == UserId)
                .OrderByDescending(j => j.CreatedDate)
                .ToListAsync();
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching jobs");
            return StatusCode(500, "Error fetching jobs");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobApplication>> GetById(string id)
    {
        try
        {
            var job = await _context.JobApplications
                .FirstOrDefaultAsync(j => j.Id == id && j.UserId == UserId);
            if (job == null) return NotFound();
            return Ok(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching job");
            return StatusCode(500, "Error fetching job");
        }
    }

    [HttpPost]
    public async Task<ActionResult<JobApplication>> Create(JobApplication job)
    {
        try
        {
            if (string.IsNullOrEmpty(job.JobTitle) || string.IsNullOrEmpty(job.CompanyName))
                return BadRequest("Job title and company name are required");

            job.Id = Guid.NewGuid().ToString();
            job.UserId = UserId;
            job.CreatedDate = DateTime.UtcNow;
            job.UpdatedDate = DateTime.UtcNow;

            _context.JobApplications.Add(job);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = job.Id }, job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job");
            return StatusCode(500, "Error creating job");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, JobApplication job)
    {
        try
        {
            var existing = await _context.JobApplications
                .FirstOrDefaultAsync(j => j.Id == id && j.UserId == UserId);
            if (existing == null) return NotFound();

            existing.JobTitle = job.JobTitle;
            existing.CompanyName = job.CompanyName;
            existing.JobLink = job.JobLink;
            existing.Description = job.Description;
            existing.Location = job.Location;
            existing.WorkType = job.WorkType;
            existing.ApplicationStatus = job.ApplicationStatus;
            existing.Notes = job.Notes;
            existing.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job");
            return StatusCode(500, "Error updating job");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var job = await _context.JobApplications
                .FirstOrDefaultAsync(j => j.Id == id && j.UserId == UserId);
            if (job == null) return NotFound();

            _context.JobApplications.Remove(job);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job");
            return StatusCode(500, "Error deleting job");
        }
    }

    [HttpPost("extract")]
    public async Task<ActionResult<JobExtractionResponse>> ExtractJobInfo(JobExtractionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Url))
                return BadRequest("URL is required");

            var data = await _scraperService.ExtractJobInfoAsync(request.Url);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting job info");
            return StatusCode(500, new { message = "Error extracting job information", error = ex.Message });
        }
    }
}
