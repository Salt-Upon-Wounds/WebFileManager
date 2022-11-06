using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebFileManager.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;


namespace WebFileManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment Environment)
        {
            _logger = logger;
            _environment = Environment;
        }

        public IActionResult Index()
        {
            DataContext context = HttpContext.RequestServices.GetService(typeof(WebFileManager.Models.DataContext)) as DataContext;

            return View(context.GetAllFileNames());
        }

        [HttpPost("FileUpload")]
        public async Task<IActionResult> Index(List<IFormFile> files)
        {
            DataContext context = HttpContext.RequestServices.GetService(typeof(WebFileManager.Models.DataContext)) as DataContext;

            var filePaths = new List<string>();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    // full path to file in temp location
                    //var filePath = Path.Combine(_environment.WebRootPath, "files", formFile.Name);
                    context.UploadFile(formFile);
                }
            }
            // process uploaded files
            // Don't rely on or trust the FileName property without validation.
            return Ok(new { count = files.Count, filePaths });
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