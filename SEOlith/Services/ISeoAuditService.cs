using SEOlith.Models;

namespace SEOlith.Services;

public interface ISeoAuditService
{
    Task<SeoAudit> GetSeoAuditAsync(string websiteUrl);
    Task<bool> CheckIfAuditExistsAsync(string websiteUrl);
}
