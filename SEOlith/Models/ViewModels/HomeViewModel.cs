using System.ComponentModel.DataAnnotations;

namespace SEOlith.Models.ViewModels;

public class HomeViewModel
{
    [Required(ErrorMessage = "Website URL is required")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    [Display(Name = "Website URL")]
    public string WebsiteUrl { get; set; }

    public SeoAudit AuditResult { get; set; }
    public string ErrorMessage { get; set; }
    public bool ShowResults { get; set; }
    public bool NoAuditFound { get; set; }
}
