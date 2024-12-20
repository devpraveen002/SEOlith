namespace SEOlith.Models;

public class SeoAudit
{
    public int Id { get; set; }
    public string WebsiteUrl { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool HasSitemap { get; set; }
    public bool HasRobotsTxt { get; set; }
    public List<string> HeadingStructure { get; set; }
    public int ImageCount { get; set; }
    public int BrokenLinksCount { get; set; }
    public double LoadTime { get; set; }
    public bool IsMobileResponsive { get; set; }
    public DateTime LastChecked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
