using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobApplicationTracker.Data;
using JobApplicationTracker.Models;
using JobApplicationTracker.Services;

namespace JobApplicationTracker.Controllers;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobApplication>>> GetAll()
    {
        try
        {
            var jobs = await _context.JobApplications.OrderByDescending(j => j.CreatedDate).ToListAsync();
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
            var job = await _context.JobApplications.FindAsync(id);
            if (job == null)
                return NotFound();

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
            var existingJob = await _context.JobApplications.FindAsync(id);
            if (existingJob == null)
                return NotFound();

            existingJob.JobTitle = job.JobTitle;
            existingJob.CompanyName = job.CompanyName;
            existingJob.JobLink = job.JobLink;
            existingJob.Description = job.Description;
            existingJob.Location = job.Location;
            existingJob.WorkType = job.WorkType;
            existingJob.ApplicationStatus = job.ApplicationStatus;
            existingJob.Notes = job.Notes;
            existingJob.UpdatedDate = DateTime.UtcNow;

            _context.JobApplications.Update(existingJob);
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
            var job = await _context.JobApplications.FindAsync(id);
            if (job == null)
                return NotFound();

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

            var extractedData = await _scraperService.ExtractJobInfoAsync(request.Url);
            return Ok(extractedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting job info");
            return StatusCode(500, new { message = "Error extracting job information", error = ex.Message });
        }
    }
}
