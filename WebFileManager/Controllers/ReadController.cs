using Microsoft.AspNetCore.Mvc;
using WebFileManager.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace WebFileManager.Controllers
{
    public class ReadController : Controller
    {

        [Route("/ReadFile")]
        public IActionResult Index(int id, int? sheet_id = null, int? class_id = null)
        {
            DataContext context = HttpContext.RequestServices.GetService(typeof(WebFileManager.Models.DataContext)) as DataContext;

            RenderModel model = context.RenderData(id, sheet_id, class_id);
            return View("ReadFile", model);
        }
    }
}
