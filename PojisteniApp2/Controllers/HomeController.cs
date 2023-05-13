using Microsoft.AspNetCore.Mvc;
using PojisteniApp2.Models;
using System.Diagnostics;
using PojisteniApp2.Data;
using System.Security.Claims;

namespace PojisteniApp2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            Statistics statistics = new();
            statistics.PersonCount = _context.Person.Count();
            statistics.InsuranceCount = _context.Insurance.Count();
            if (statistics.InsuranceCount > 0) // Following LINQ query throws exception when no insurances in the DB
            {
                // https://stackoverflow.com/questions/3456926/how-to-insert-a-thousand-separator-comma-with-convert-to-double
                statistics.MaxInsuranceLimit = _context.Insurance.Max(i => i.InsuranceAmount).ToString("# ### ### ###");
            }

            // Logged user or anonymous visitor?
            bool isLoggedUser = User.FindFirstValue(ClaimTypes.NameIdentifier) != null;
            // Viewbags to adjust view content
            ViewBag.ShowLoginRequest = !isLoggedUser;
            if (isLoggedUser)
            {
                ViewBag.Slide1Active = string.Empty;
                ViewBag.Slide2Active = "active";
            }
            else
            {
				ViewBag.Slide1Active = "active";
				ViewBag.Slide2Active = string.Empty;
            }

			return View(statistics);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}