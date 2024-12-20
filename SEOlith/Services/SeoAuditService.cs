using SEOlith.Contexts;
using SEOlith.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using HtmlAgilityPack;

namespace SEOlith.Services;

public class SeoAuditService : ISeoAuditService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SeoAuditService> _logger;
    private readonly SeolithDbContext _context;

    public SeoAuditService(
        IHttpClientFactory httpClientFactory,
        ILogger<SeoAuditService> logger,
        SeolithDbContext context)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _context = context;
    }

    public async Task<bool> CheckIfAuditExistsAsync(string websiteUrl)
    {
        try
        {
            return await _context.SeoAudits
                .AnyAsync(a => a.WebsiteUrl == websiteUrl &&
                              a.LastChecked >= DateTime.UtcNow.AddHours(-24));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking audit existence for {WebsiteUrl}", websiteUrl);
            return false;
        }
    }

    public async Task<SeoAudit> GetSeoAuditAsync(string websiteUrl)
    {
        try
        {
            _logger.LogInformation("Starting SEO audit for {WebsiteUrl}", websiteUrl);

            // Check for recent audit
            var existingAudit = await _context.SeoAudits
                .Where(a => a.WebsiteUrl == websiteUrl)
                .OrderByDescending(a => a.LastChecked)
                .FirstOrDefaultAsync();

            if (existingAudit != null && existingAudit.LastChecked >= DateTime.UtcNow.AddHours(-24))
            {
                _logger.LogInformation("Returning cached audit for {WebsiteUrl}", websiteUrl);
                return existingAudit;
            }

            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var response = await client.GetAsync(websiteUrl);
            var content = await response.Content.ReadAsStringAsync();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);

            var audit = new SeoAudit
            {
                WebsiteUrl = websiteUrl,
                Title = htmlDocument.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim() ?? "No title found",
                Description = htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='description']")?.GetAttributeValue("content", "No description found"),
                HasSitemap = await CheckSitemapExistsAsync(websiteUrl),
                HasRobotsTxt = await CheckRobotsTxtExistsAsync(websiteUrl),
                HeadingStructure = GetHeadingStructure(htmlDocument),
                ImageCount = htmlDocument.DocumentNode.SelectNodes("//img")?.Count ?? 0,
                BrokenLinksCount = await CheckBrokenLinksAsync(htmlDocument, websiteUrl),
                LoadTime = await MeasureLoadTimeAsync(websiteUrl),
                IsMobileResponsive = CheckMobileResponsiveness(htmlDocument),
                LastChecked = DateTime.UtcNow
            };

            _logger.LogInformation("Saving new audit for {WebsiteUrl}", websiteUrl);

            // Using tracking for new entity
            _context.SeoAudits.Add(audit);
            await _context.SaveChangesAsync();

            return audit;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing SEO audit for {WebsiteUrl}", websiteUrl);
            throw;
        }
    }

    private string ExtractTitle(HtmlDocument document)
    {
        return document.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim()
            ?? document.DocumentNode.SelectSingleNode("//meta[@property='og:title']")?.GetAttributeValue("content", "")?.Trim()
            ?? "No title found";
    }

    private string ExtractDescription(HtmlDocument document)
    {
        return document.DocumentNode.SelectSingleNode("//meta[@name='description']")?.GetAttributeValue("content", "")?.Trim()
            ?? document.DocumentNode.SelectSingleNode("//meta[@property='og:description']")?.GetAttributeValue("content", "")?.Trim()
            ?? "No description found";
    }

    private int CountImages(HtmlDocument document)
    {
        var imgTags = document.DocumentNode.SelectNodes("//img") ?? Enumerable.Empty<HtmlNode>();
        var pictureTags = document.DocumentNode.SelectNodes("//picture") ?? Enumerable.Empty<HtmlNode>();
        return imgTags.Count() + pictureTags.Count();
    }

    private async Task<bool> CheckSitemapExistsAsync(string websiteUrl)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var sitemapUrl = $"{websiteUrl.TrimEnd('/')}/sitemap.xml";
            var response = await client.GetAsync(sitemapUrl);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking sitemap for {WebsiteUrl}", websiteUrl);
            return false;
        }
    }

    private async Task<bool> CheckRobotsTxtExistsAsync(string websiteUrl)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var robotsUrl = $"{websiteUrl.TrimEnd('/')}/robots.txt";
            var response = await client.GetAsync(robotsUrl);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking robots.txt for {WebsiteUrl}", websiteUrl);
            return false;
        }
    }

    private List<string> GetHeadingStructure(HtmlDocument document)
    {
        var headings = new List<string>();
        for (int i = 1; i <= 6; i++)
        {
            var nodes = document.DocumentNode.SelectNodes($"//h{i}");
            if (nodes != null)
            {
                headings.AddRange(nodes.Select(node => $"H{i}: {node.InnerText.Trim()}"));
            }
        }
        return headings;
    }

    private async Task<double> MeasureLoadTimeAsync(string websiteUrl)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            await client.GetAsync(websiteUrl);
            stopwatch.Stop();
            return Math.Round(stopwatch.ElapsedMilliseconds / 1000.0, 2);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error measuring load time for {WebsiteUrl}", websiteUrl);
            return -1;
        }
    }

    private bool CheckMobileResponsiveness(HtmlDocument document)
    {
        try
        {
            var viewport = document.DocumentNode.SelectSingleNode("//meta[@name='viewport']");
            var mediaQueries = document.DocumentNode.SelectNodes("//link[@media]");
            var responsiveMetaTags = document.DocumentNode.SelectNodes("//meta[@name='viewport' or @name='mobile-web-app-capable' or @name='apple-mobile-web-app-capable']");

            return viewport != null ||
                   (mediaQueries != null && mediaQueries.Any()) ||
                   (responsiveMetaTags != null && responsiveMetaTags.Any());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking mobile responsiveness");
            return false;
        }
    }

    private async Task<int> CheckBrokenLinksAsync(HtmlDocument document, string baseUrl)
    {
        var brokenLinks = 0;
        var links = document.DocumentNode.SelectNodes("//a[@href]");

        if (links != null)
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            var tasks = links.Take(10)
                .Select(link => link.GetAttributeValue("href", ""))
                .Where(href => !string.IsNullOrEmpty(href))
                .Select(async href =>
                {
                    try
                    {
                        if (Uri.TryCreate(new Uri(baseUrl), href, out Uri absoluteUri))
                        {
                            var response = await client.GetAsync(absoluteUri);
                            return !response.IsSuccessStatusCode;
                        }
                        return false;
                    }
                    catch
                    {
                        return true;
                    }
                });

            var results = await Task.WhenAll(tasks);
            brokenLinks = results.Count(isBroken => isBroken);
        }

        return brokenLinks;
    }
}