using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PojisteniApp2.Data;
using PojisteniApp2.Helpers;
using PojisteniApp2.Models;

namespace PojisteniApp2.Controllers
{
    public class PeopleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotyfService _notyf;
        private readonly float _imageMaxSize;
        private readonly string _defaultImagePath;
        private readonly string _genericErrorMessage = "Opravte zadaná data";

        public PeopleController(ApplicationDbContext context, INotyfService notyf, IConfiguration configuration)
        {
            _context = context;
            _notyf = notyf;
            _imageMaxSize = configuration.GetValue<float>("Person:ImageMaxSize");
            _defaultImagePath = configuration?.GetValue<string>("Person:DefaultImagePath") ?? "/images/img-person-default.png";
        }

        // GET: People
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            // Sort based on tutorial: https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/sort-filter-page?view=aspnetcore-7.0#add-sorting-functionality-to-the-index-method
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "descending" : "";
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;

            var people = (IQueryable<Person>)_context.Person;

            if (!String.IsNullOrEmpty(searchString))
            {
                people = people.Where(e => e.LastName.Contains(searchString) || e.FirstName.Contains(searchString));
            }

            if (sortOrder == "descending")
            {
                people = people.OrderByDescending(e => e.LastName)
                                .ThenByDescending(e => e.FirstName)
                                .ThenByDescending(e => e.City)
                                .ThenByDescending(e => e.Street);
            }
            else
            {
                people = people.OrderBy(e => e.LastName)
                                .ThenBy(e => e.FirstName)
                                .ThenBy(e => e.City)
                                .ThenBy(e => e.Street);
            }

            int pageSize = 3;
            return View(await PaginatedList<Person>.CreateAsync(people.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: People/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Person == null)
            {
                return NotFound();
            }

            var person = await _context.Person
                .Include(p => p.Insurances) // Find related insurances
                .ThenInclude(i => i.InsuranceType) // Find insurance type
                .AsNoTracking() // Improves performance returned entities won't be updated
                .FirstOrDefaultAsync(m => m.PersonId == id);
            if (person == null)
            {
                return NotFound();
            }

            // Save image URL to display into ViewBag
            ViewBag.ImageDataUrl = CreateImageURL(person.ImageData);
    
            return View(person);
        }

        // GET: People/Create
        public IActionResult Create()
        {
            // Default image to display in view
            ViewBag.ImageDataUrl = _defaultImagePath;
            
            // Profile image max size for client validation
            ViewBag.ImageMaxSize = _imageMaxSize;
            
            return View();
        }

        // POST: People/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PersonId,FirstName,LastName,Email,Phone,Street,City,PostalCode")] Person person)
        {
            // Default image to display in view
            ViewBag.ImageDataUrl = _defaultImagePath;
            
            // Profile image max size for client validation
            ViewBag.ImageMaxSize = _imageMaxSize;

            // Save provided image to ImageData
            if (Request.Form.Files.Count > 0) // Image provided by user
            {
                IFormFile file = Request.Form.Files[0];
                if (IsImage(file)) // Provided file is an image file
                {
                    if (IsInSizeRange(_imageMaxSize, file)) 
                    {
                        person.ImageData = FileToByteArray(file);
                    }
                    else // Image is larger than set limit
                    {
                        ModelState.AddModelError("ImageData", $"Maximální velikost obrázku je {_imageMaxSize} MB");
                        _notyf.Error(_genericErrorMessage);
                        return View(person);
                    }
                }
                else // is not an image
                {
                    ModelState.AddModelError("ImageData", "Vybraný soubor musí být obrázek");
                    _notyf.Error(_genericErrorMessage);
                    return View(person);
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(person);
                await _context.SaveChangesAsync();
                _notyf.Success($"Nový pojištěnec {person.FullName} přidán");
                return RedirectToAction(nameof(Details), new { id = person.PersonId});
            }
            _notyf.Error(_genericErrorMessage);
            return View(person);
        }

        // GET: People/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Person == null)
            {
                return NotFound();
            }

            var person = await _context.Person.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            // Save image URL to display into ViewBag
            ViewBag.ImageDataUrl = CreateImageURL(person.ImageData);
            
            // Profile image max size for client validation
            ViewBag.ImageMaxSize = _imageMaxSize;
            
            // Saves URL user is coming from to be used in POST Edit action
            TempData["PreviousUrl"] = Request.Headers["Referer"].ToString();

            return View(person);
        }

        // POST: People/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PersonId,FirstName,LastName,Email,Phone,Street,City,PostalCode")] Person person)
        {
            if (id != person.PersonId)
            {
                return NotFound();
            }

            // Profile image max size for client validation
            ViewBag.ImageMaxSize = _imageMaxSize;

            // Save provided image to ImageData
            if (Request.Form.Files.Count == 0) // Image not provided by user
            {
                // Find existing image data in DB and set it to person.ImageData
                SetExistingImageData(person);
            }
            else // Image provided by user
            {
                IFormFile file = Request.Form.Files[0];
                if (IsImage(file)) // Provided file is an image file
                {
                    if (IsInSizeRange(_imageMaxSize, file))
                    {
                        person.ImageData = FileToByteArray(file);
                    }
                    else // Image is larger than set limit
                    {
                        ModelState.AddModelError("ImageData", $"Maximální velikost obrázku je {_imageMaxSize} MB");
                        RetrieveExistingImageData(person);
                        _notyf.Error(_genericErrorMessage);
                        return View(person);
                    }
                }
                else // is not an image
                {
                    ModelState.AddModelError("ImageData", "Vybraný soubor musí být obrázek");
                    RetrieveExistingImageData(person);
                    _notyf.Error(_genericErrorMessage);
                    return View(person);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(person);
                    await _context.SaveChangesAsync();
                    _notyf.Success($"Změny pojištěnce {person.FullName} uloženy");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonExists(person.PersonId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
                // Redirect to previous URL if available
                if (TryGetPreviousUrl(out string previousUrl))
                {
                    return Redirect(previousUrl);
                }
                // Redirect to defaul action People -> Index if condition above is not true
                return RedirectToAction(nameof(Index));
            }
            RetrieveExistingImageData(person);
            _notyf.Error(_genericErrorMessage);
            return View(person);
        }

        // GET: People/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Person == null)
            {
                return NotFound();
            }

            var person = await _context.Person
                .FirstOrDefaultAsync(m => m.PersonId == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // POST: People/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Person == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Person'  is null.");
            }
            var person = await _context.Person.FindAsync(id);
            if (person != null)
            {
                _context.Person.Remove(person);
                _notyf.Success($"Pojištěnec {person.FullName} smazán");
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PersonExists(int id)
        {
          return (_context.Person?.Any(e => e.PersonId == id)).GetValueOrDefault();
        }

        private string CreateImageURL(byte[] imageData)
        {
            if (imageData.Length > 0) // Check if profile image exists in DB
            {
                string imageBase64Data = Convert.ToBase64String(imageData);
                return string.Format($"data:image/jpg;base64,{imageBase64Data}");
            }
            else // Return default image if not
            {
                return _defaultImagePath;
            }
        }

        private byte[] FileToByteArray(IFormFile file)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                file.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private bool IsImage(IFormFile file)
        {
            return file.ContentType.StartsWith("image/");
        }

        /// <summary>
        /// Valides file size for set maximum limit
        /// </summary>
        /// <param name="maxSize">Maximum file size in MB</param>
        /// <param name="file">File for validation</param>
        /// <returns>Validation result</returns>
        private bool IsInSizeRange(float maxSize, IFormFile file)
        {
            bool result = true;
            if (file.Length > maxSize * 1024 * 1024) // maxSize in bytes
            {
                result = false;
            }
            return result;
        }

        private void SetExistingImageData(Person person)
        {
            // Find existing record in DB
            Person? existingPerson = _context.Person.Find(person.PersonId);
            if (existingPerson == null) // Person not found
            {
                person.ImageData = Array.Empty<byte>();
            }
            else // Person found
            {
                // Detach the existing person from the context to prevent error:
                // The instance of entity type 'Person' cannot be tracked because another instance
                // with the same key value for { 'PersonId'} is already being tracked.
                _context.Entry(existingPerson).State = EntityState.Detached;
                person.ImageData = existingPerson.ImageData;
            }
        }

        private void RetrieveExistingImageData(Person person)
        {
            SetExistingImageData(person);
            // Save image URL to display into ViewBag
            ViewBag.ImageDataUrl = CreateImageURL(person.ImageData);
        }

        private bool TryGetPreviousUrl(out string? previousUrl)
        {
            bool result = false;
            previousUrl = null;
            // Check if URL user is coming from is available
            if (TempData.ContainsKey("PreviousUrl"))
            {
                if (TempData["PreviousUrl"] != null)
                {
                    previousUrl = TempData["PreviousUrl"].ToString();
                    result = true;
                }
            }
            return result;
        }
    }
}
