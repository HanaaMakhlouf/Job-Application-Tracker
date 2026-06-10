using HtmlAgilityPack;
using JobApplicationTracker.Models;
using System.Text.RegularExpressions;

namespace JobApplicationTracker.Services;

public interface IJobScraperService
{
    Task<JobExtractionResponse> ExtractJobInfoAsync(string url);
}

public class JobScraperService : IJobScraperService
{
    private readonly HttpClient _httpClient;

    public JobScraperService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    public async Task<JobExtractionResponse> ExtractJobInfoAsync(string url)
    {
        try
        {
            var html = await FetchHtmlAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var response = new JobExtractionResponse();

            // Try to extract based on the job site
            if (url.Contains("linkedin.com"))
            {
                ExtractFromLinkedIn(doc, response);
            }
            else if (url.Contains("indeed.com"))
            {
                ExtractFromIndeed(doc, response);
            }
            else if (url.Contains("glassdoor.com"))
            {
                ExtractFromGlassdoor(doc, response);
            }
            else
            {
                // Generic extraction for other job sites
                ExtractGeneric(doc, response);
            }

            // If extraction failed, use basic extraction
            if (string.IsNullOrEmpty(response.JobTitle))
            {
                ExtractGeneric(doc, response);
            }

            return response;
        }
        catch (Exception ex)
        {
            return new JobExtractionResponse
            {
                JobTitle = "Job Title",
                CompanyName = "Company Name",
                Description = "Failed to extract job information",
                Location = "Location",
                WorkType = "Remote"
            };
        }
    }

    private async Task<string> FetchHtmlAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private void ExtractFromLinkedIn(HtmlDocument doc, JobExtractionResponse response)
    {
        // LinkedIn job title
        var titleNode = doc.DocumentNode.SelectSingleNode("//h1[@class='show-more-less-html__markup']");
        response.JobTitle = titleNode?.InnerText?.Trim() ?? "Software Developer";

        // LinkedIn company name
        var companyNode = doc.DocumentNode.SelectSingleNode("//a[@class='topcard__org-name-link']");
        response.CompanyName = companyNode?.InnerText?.Trim() ?? "Company";

        // LinkedIn description
        var descNode = doc.DocumentNode.SelectSingleNode("//div[@class='show-more-less-html__markup']");
        response.Description = descNode?.InnerText?.Trim() ?? "";

        // LinkedIn location
        var locationNode = doc.DocumentNode.SelectSingleNode("//span[@class='topcard__location']");
        response.Location = locationNode?.InnerText?.Trim() ?? "";

        // Try to detect work type from description
        response.WorkType = DetectWorkType(response.Description);
    }

    private void ExtractFromIndeed(HtmlDocument doc, JobExtractionResponse response)
    {
        // Indeed job title
        var titleNode = doc.DocumentNode.SelectSingleNode("//h1[@class='jobsearch-JobInfoHeader-title']");
        response.JobTitle = titleNode?.InnerText?.Trim() ?? "Job Title";

        // Indeed company name
        var companyNode = doc.DocumentNode.SelectSingleNode("//div[@class='jobsearch-InlineCompanyRating-companyHeader']");
        response.CompanyName = companyNode?.InnerText?.Trim() ?? "Company";

        // Indeed location
        var locationNode = doc.DocumentNode.SelectSingleNode("//div[@data-testid='jobsearch-JobInfoHeader-jobLocation']");
        response.Location = locationNode?.InnerText?.Trim() ?? "";

        // Indeed description
        var descNode = doc.DocumentNode.SelectSingleNode("//div[@id='jobDescriptionText']");
        response.Description = descNode?.InnerText?.Trim() ?? "";

        response.WorkType = DetectWorkType(response.Description);
    }

    private void ExtractFromGlassdoor(HtmlDocument doc, JobExtractionResponse response)
    {
        // Glassdoor job title
        var titleNode = doc.DocumentNode.SelectSingleNode("//h1[@class='header__h1']");
        response.JobTitle = titleNode?.InnerText?.Trim() ?? "Job Title";

        // Glassdoor company name
        var companyNode = doc.DocumentNode.SelectSingleNode("//div[@class='EmployerProfile__name']");
        response.CompanyName = companyNode?.InnerText?.Trim() ?? "Company";

        // Glassdoor location
        var locationNode = doc.DocumentNode.SelectSingleNode("//div[@class='JobDetails__location']");
        response.Location = locationNode?.InnerText?.Trim() ?? "";

        // Glassdoor description
        var descNode = doc.DocumentNode.SelectSingleNode("//div[@class='JobDetails__desc']");
        response.Description = descNode?.InnerText?.Trim() ?? "";

        response.WorkType = DetectWorkType(response.Description);
    }

    private void ExtractGeneric(HtmlDocument doc, JobExtractionResponse response)
    {
        // Generic extraction using common patterns
        var titleNodes = doc.DocumentNode.SelectNodes("//h1 | //h2[@class*='title'] | //title");
        if (titleNodes != null && titleNodes.Count > 0)
        {
            response.JobTitle = CleanText(titleNodes[0].InnerText);
        }

        // Try to find company name
        var companyPatterns = new[] { "company", "employer", "organization" };
        var companyNode = titleNodes?.FirstOrDefault(n => n.InnerText.ToLower().Contains("company"));
        if (companyNode != null)
        {
            response.CompanyName = CleanText(companyNode.InnerText);
        }

        // Extract all text and look for description
        var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
        if (bodyNode != null)
        {
            var fullText = bodyNode.InnerText;
            response.Description = CleanText(fullText.Substring(0, Math.Min(1000, fullText.Length)));
        }

        // Look for location in common patterns
        var textContent = doc.DocumentNode.InnerText;
        var locationMatch = Regex.Match(textContent, @"(Location|City|Based in)[:\s]+([^,\n]+)", RegexOptions.IgnoreCase);
        if (locationMatch.Success)
        {
            response.Location = CleanText(locationMatch.Groups[2].Value);
        }

        response.WorkType = DetectWorkType(response.Description);
    }

    private string DetectWorkType(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "Remote";

        var lowerText = text.ToLower();

        if (lowerText.Contains("hybrid"))
            return "Hybrid";
        if (lowerText.Contains("on-site") || lowerText.Contains("onsite") || lowerText.Contains("office"))
            return "On-site";
        if (lowerText.Contains("remote") || lowerText.Contains("work from home"))
            return "Remote";

        return "Remote"; // Default
    }

    private string CleanText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // Remove extra whitespace and HTML entities
        text = System.Net.WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"\s+", " ");
        return text.Trim();
    }
}
