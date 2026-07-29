using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using JobApplicationTracker.Models;

namespace JobApplicationTracker.Services;

public interface IJobScraperService
{
    Task<JobExtractionResponse> ExtractJobInfoAsync(string url);
}

public class JobScraperService : IJobScraperService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JobScraperService> _logger;

    public JobScraperService(HttpClient httpClient, ILogger<JobScraperService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
    }

    public async Task<JobExtractionResponse> ExtractJobInfoAsync(string url)
    {
        try
        {
            var html = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var response = new JobExtractionResponse();

            // 1. Try JSON-LD structured data (most reliable when present)
            TryExtractJsonLd(doc, response);

            // 2. Try Open Graph meta tags
            if (string.IsNullOrEmpty(response.JobTitle))
                TryExtractOpenGraph(doc, response);

            // 3. Site-specific fallbacks
            if (string.IsNullOrEmpty(response.JobTitle))
            {
                if (url.Contains("linkedin.com")) TryExtractLinkedIn(doc, response);
                else if (url.Contains("indeed.com")) TryExtractIndeed(doc, response);
                else TryExtractGeneric(doc, response);
            }

            if (!string.IsNullOrEmpty(response.Description))
                response.WorkType = DetectWorkType(response.Description);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to scrape {Url}", url);
            return new JobExtractionResponse();
        }
    }

    private void TryExtractJsonLd(HtmlDocument doc, JobExtractionResponse response)
    {
        var scripts = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
        if (scripts == null) return;

        foreach (var script in scripts)
        {
            try
            {
                var json = script.InnerText.Trim();
                using var doc2 = JsonDocument.Parse(json);
                var root = doc2.RootElement;

                var type = root.TryGetProperty("@type", out var t) ? t.GetString() : "";
                if (type != "JobPosting") continue;

                if (root.TryGetProperty("title", out var title))
                    response.JobTitle = CleanText(title.GetString() ?? "");

                if (root.TryGetProperty("hiringOrganization", out var org) &&
                    org.TryGetProperty("name", out var orgName))
                    response.CompanyName = CleanText(orgName.GetString() ?? "");

                if (root.TryGetProperty("description", out var desc))
                    response.Description = CleanText(StripHtml(desc.GetString() ?? ""), maxLength: 2000);

                if (root.TryGetProperty("jobLocation", out var loc))
                {
                    if (loc.TryGetProperty("address", out var addr))
                    {
                        var city = addr.TryGetProperty("addressLocality", out var c) ? c.GetString() : "";
                        var country = addr.TryGetProperty("addressCountry", out var cn) ? cn.GetString() : "";
                        response.Location = string.Join(", ", new[] { city, country }.Where(s => !string.IsNullOrEmpty(s)));
                    }
                }

                return; // Found a JobPosting, stop
            }
            catch { /* malformed JSON, skip */ }
        }
    }

    private void TryExtractOpenGraph(HtmlDocument doc, JobExtractionResponse response)
    {
        var ogTitle = GetMetaContent(doc, "og:title");
        if (string.IsNullOrEmpty(ogTitle)) return;

        // Strip site name suffix: "Title at Company | LinkedIn" → "Title at Company"
        ogTitle = Regex.Replace(ogTitle, @"\s*\|.*$", "").Trim();
        // Strip numeric prefix from LinkedIn: "(1) Title" → "Title"
        ogTitle = Regex.Replace(ogTitle, @"^\(\d+\)\s*", "").Trim();

        // Try to split "Job Title at Company Name"
        var atMatch = Regex.Match(ogTitle, @"^(.+?)\s+at\s+(.+)$", RegexOptions.IgnoreCase);
        if (atMatch.Success)
        {
            response.JobTitle = atMatch.Groups[1].Value.Trim();
            if (string.IsNullOrEmpty(response.CompanyName))
                response.CompanyName = atMatch.Groups[2].Value.Trim();
        }
        else
        {
            response.JobTitle = ogTitle;
        }

        if (string.IsNullOrEmpty(response.Description))
        {
            var ogDesc = GetMetaContent(doc, "og:description");
            if (!string.IsNullOrEmpty(ogDesc))
                response.Description = CleanText(ogDesc, maxLength: 1000);
        }
    }

    private void TryExtractLinkedIn(HtmlDocument doc, JobExtractionResponse response)
    {
        response.JobTitle = CleanText(
            doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'top-card-layout__title')]")?.InnerText
            ?? doc.DocumentNode.SelectSingleNode("//h1")?.InnerText ?? "");

        response.CompanyName = CleanText(
            doc.DocumentNode.SelectSingleNode("//a[contains(@class,'topcard__org-name-link')]")?.InnerText
            ?? doc.DocumentNode.SelectSingleNode("//span[contains(@class,'topcard__flavor--black-link')]")?.InnerText ?? "");

        response.Location = CleanText(
            doc.DocumentNode.SelectSingleNode("//span[contains(@class,'topcard__flavor--bullet')]")?.InnerText ?? "");

        response.Description = CleanText(
            doc.DocumentNode.SelectSingleNode("//div[contains(@class,'show-more-less-html__markup')]")?.InnerText ?? "",
            maxLength: 2000);
    }

    private void TryExtractIndeed(HtmlDocument doc, JobExtractionResponse response)
    {
        response.JobTitle = CleanText(
            doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'jobsearch-JobInfoHeader-title')]")?.InnerText ?? "");

        response.CompanyName = CleanText(
            doc.DocumentNode.SelectSingleNode("//div[@data-company-name='true']")?.InnerText ?? "");

        response.Description = CleanText(
            doc.DocumentNode.SelectSingleNode("//div[@id='jobDescriptionText']")?.InnerText ?? "",
            maxLength: 2000);
    }

    private void TryExtractGeneric(HtmlDocument doc, JobExtractionResponse response)
    {
        response.JobTitle = CleanText(
            doc.DocumentNode.SelectSingleNode("//h1")?.InnerText ?? "");

        var bodyText = doc.DocumentNode.SelectSingleNode("//body")?.InnerText ?? "";
        response.Description = CleanText(bodyText, maxLength: 1000);
    }

    private string? GetMetaContent(HtmlDocument doc, string property)
    {
        return doc.DocumentNode
            .SelectSingleNode($"//meta[@property='{property}']")?
            .GetAttributeValue("content", null);
    }

    private string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return html;
        return Regex.Replace(html, "<[^>]+>", " ");
    }

    private string CleanText(string text, int maxLength = 500)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        text = System.Net.WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"\s+", " ").Trim();
        return text.Length > maxLength ? text[..maxLength] : text;
    }

    private string DetectWorkType(string text)
    {
        var lower = text.ToLower();
        if (lower.Contains("hybrid")) return "Hybrid";
        if (lower.Contains("on-site") || lower.Contains("onsite") || lower.Contains("office")) return "On-site";
        if (lower.Contains("remote") || lower.Contains("work from home")) return "Remote";
        return "Remote";
    }
}
