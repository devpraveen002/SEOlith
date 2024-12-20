using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SEOlith.Models;
using SEOlith.Models.ViewModels;
using SEOlith.Services;

namespace SEOlith.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISeoAuditService _seoAuditService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ISeoAuditService seoAuditService, ILogger<HomeController> logger)
        {
            _seoAuditService = seoAuditService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new HomeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Analyse(HomeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Index", model);
                }

                // Normalize the URL
                if (!model.WebsiteUrl.StartsWith("http://") && !model.WebsiteUrl.StartsWith("https://"))
                {
                    model.WebsiteUrl = "https://" + model.WebsiteUrl;
                }

                var audit = await _seoAuditService.GetSeoAuditAsync(model.WebsiteUrl);

                // Create a new model to ensure clean state
                var resultModel = new HomeViewModel
                {
                    WebsiteUrl = model.WebsiteUrl,
                    ShowResults = true,
                    AuditResult = audit,
                    NoAuditFound = audit == null
                };

                _logger.LogInformation($"Audit completed for {model.WebsiteUrl}");
                return View(resultModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SEO audit");
                return View(new HomeViewModel
                {
                    WebsiteUrl = model.WebsiteUrl,
                    ErrorMessage = "An error occurred while analyzing the website. Please try again."
                });
            }
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
