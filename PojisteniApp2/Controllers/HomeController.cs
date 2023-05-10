using Microsoft.AspNetCore.Mvc;
using PojisteniApp2.Models;
using System.Diagnostics;
using PojisteniApp2.Data;

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

            return View(statistics);
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