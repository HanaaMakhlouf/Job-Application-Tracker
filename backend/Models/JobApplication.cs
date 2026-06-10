namespace JobApplicationTracker.Models;

public class JobApplication
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string JobLink { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string WorkType { get; set; } = "Remote"; // On-site, Remote, Hybrid
    public string ApplicationStatus { get; set; } = "Applied"; // Applied, In Progress, Interview, Offer, Rejected
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
}

public class JobExtractionRequest
{
    public string Url { get; set; } = string.Empty;
}

public class JobExtractionResponse
{
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string WorkType { get; set; } = "Remote";
}
