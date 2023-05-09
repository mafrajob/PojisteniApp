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
            ViewBag.PersonCount = _context.Person.Count();
            int insuranceCount = _context.Insurance.Count();
            ViewBag.InsuranceCount = insuranceCount;
            int maxInsuranceLimit = 0;
            if (insuranceCount > 0)
            {
                // This LINQ query throws exception when no insurances in the DB
                maxInsuranceLimit = _context.Insurance.Max(i => i.InsuranceAmount);
            }
            // https://stackoverflow.com/questions/3456926/how-to-insert-a-thousand-separator-comma-with-convert-to-double
            ViewBag.MaxInsuranceLimit = maxInsuranceLimit.ToString("N");

            return View();
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