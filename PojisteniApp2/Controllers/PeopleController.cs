using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PojisteniApp2.Data;
using PojisteniApp2.Models;

namespace PojisteniApp2.Controllers
{
    public class PeopleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly int _imageMaxSize;

        public PeopleController(ApplicationDbContext context)
        {
            _context = context;
            _imageMaxSize = 2;
        }

        // GET: People
        public async Task<IActionResult> Index()
        {
              return _context.Person != null ? 
                          View(await _context.Person.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Person'  is null.");
        }

        // GET: People/Details/5
        public async Task<IActionResult> Details(int? id)
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

            // Find insurances of the selected person in the database
            person.Insurances = _context.Insurance.Where(i => i.PersonId == id).ToList();

            foreach (Insurance insurance in person.Insurances)
            {
                // Find insurance type details for each of the person's insurances
                insurance.InsuranceType = await _context.InsuranceType
                    .FirstOrDefaultAsync(insuranceType => insuranceType.InsuranceTypeId == insurance.InsuranceTypeId);
            }

            // Save image URL to display into ViewBag
            ViewBag.ImageDataUrl = CreateImageURL(person.ImageData);

            return View(person);
        }

        // GET: People/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: People/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PersonId,FirstName,LastName,Email,Phone,Street,City,PostalCode")] Person person)
        {
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
                        return View(person);
                    }
                }
                else // is not an image
                {
                    ModelState.AddModelError("ImageData", "Vybraný soubor musí být obrázek");
                    return View(person);
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(person);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = person.PersonId});
            }
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
                        SetExistingImageData(person);
                        // Save image URL to display into ViewBag
                        ViewBag.ImageDataUrl = CreateImageURL(person.ImageData);
                        return View(person);
                    }
                }
                else // is not an image
                {
                    ModelState.AddModelError("ImageData", "Vybraný soubor musí být obrázek");
                    SetExistingImageData(person);
                    // Save image URL to display into ViewBag
                    ViewBag.ImageDataUrl = CreateImageURL(person.ImageData);
                    return View(person);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(person);
                    await _context.SaveChangesAsync();
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
            string imageBase64Data = Convert.ToBase64String(imageData);
            return string.Format($"data:image/jpg;base64,{imageBase64Data}");
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
        private bool IsInSizeRange(int maxSize, IFormFile file)
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
