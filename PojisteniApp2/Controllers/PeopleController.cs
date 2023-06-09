﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

			// Assign string "disabled" if current user is neither author nor admin
            ViewBag.Disabled = UserHelper.IsAuthorOrAdmin(User, person) ? string.Empty : "disabled";

            return View(person);
        }

        // GET: People/Create
        public IActionResult Create()
        {
            // Default image to display in view
            ViewBag.ImageDataUrl = _defaultImagePath;
            
            // Profile image max size for client validation
            ViewBag.ImageMaxSize = _imageMaxSize;

			// Assign string "disabled" if no user logged in
			ViewBag.Disabled = UserHelper.IsLoggedUser(User) ? string.Empty : "disabled";

			return View();
        }

        // POST: People/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Create([Bind("PersonId,FirstName,LastName,Email,Phone,Street,City,PostalCode")] Person person)
        {
            // Default image to display in view
            ViewBag.ImageDataUrl = _defaultImagePath;
            
            // Profile image max size for client validation
            ViewBag.ImageMaxSize = _imageMaxSize;

			// No disabled functionality for POST method of Create action needed
			ViewBag.Disabled = string.Empty;

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

            // Server side validations
            RunServerValidations(person);

			if (ModelState.IsValid)
            {
                // Set currently logged in user's id as AuthorId
                person.AuthorId = UserHelper.GetUserId(User);
				_context.Add(person);
                await _context.SaveChangesAsync();
                _notyf.Success($"Přidán pojištěnec<br>{person.FirstName} {person.LastName}");
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

			// Assign string "disabled" if current user is neither author nor admin
			ViewBag.Disabled = UserHelper.IsAuthorOrAdmin(User, person) ? string.Empty : "disabled";

            return View(person);
        }

        // POST: People/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Edit(int id, [Bind("PersonId,FirstName,LastName,Email,Phone,Street,City,PostalCode")] Person person)
        {
            if (id != person.PersonId)
            {
                return NotFound();
            }

            // Profile image max size for client validation
            ViewBag.ImageMaxSize = _imageMaxSize;

            // No disabled functionality for POST method of Edit action needed
            ViewBag.Disabled = string.Empty;

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

			// Server side validations
			RunServerValidations(person);

			if (ModelState.IsValid)
            {
                try
                {
                    // Set AuthorId from existing record, otherwise it will be set to NULL by following update
                    SetExistingAuthorId(person);
                    _context.Update(person);
                    await _context.SaveChangesAsync();
                    _notyf.Success($"Změněn pojištěnec<br>{person.FirstName} {person.LastName}");
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

            // Save image URL to display into ViewBag
            ViewBag.ImageDataUrl = CreateImageURL(person.ImageData);

			// Assign string "disabled" if current user is neither author nor admin
			ViewBag.Disabled = UserHelper.IsAuthorOrAdmin(User, person) ? string.Empty : "disabled";

			return View(person);
        }

        // POST: People/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
		[Authorize]
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
                _notyf.Success($"Smazán pojištěnec<br>{person.FirstName} {person.LastName}");
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
            return !(file.Length > maxSize * 1024 * 1024);
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

        private void SetExistingAuthorId(Person person)
        {
			// Find existing record in DB
			Person? existingPerson = _context.Person.Find(person.PersonId);
			if (existingPerson != null) // Person found
			{
				// Detach the existing person from the context to prevent error:
				// The instance of entity type 'Person' cannot be tracked because another instance
				// with the same key value for { 'PersonId'} is already being tracked.
				_context.Entry(existingPerson).State = EntityState.Detached;
				person.AuthorId = existingPerson.AuthorId;
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
            previousUrl = null;
            // Check if URL user is coming from is available
            if (TempData.ContainsKey("PreviousUrl"))
            {
                if (TempData["PreviousUrl"] != null)
                {
                    previousUrl = TempData["PreviousUrl"].ToString();
                    return true;
                }
            }
            return false;
        }

		// Email validation based on tutorial https://learn.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
		private bool IsValidEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return false;

			try
			{
				// Normalize the domain
				email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
									  RegexOptions.None, TimeSpan.FromMilliseconds(200));

				// Examines the domain part of the email and normalizes it.
				string DomainMapper(Match match)
				{
					// Use IdnMapping class to convert Unicode domain names.
					var idn = new IdnMapping();

					// Pull out and process domain name (throws ArgumentException on invalid)
					string domainName = idn.GetAscii(match.Groups[2].Value);

					return match.Groups[1].Value + domainName;
				}
			}
			catch (RegexMatchTimeoutException e)
			{
				return false;
			}
			catch (ArgumentException e)
			{
				return false;
			}

			try
			{
				return Regex.IsMatch(email,
					@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
					RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
			}
			catch (RegexMatchTimeoutException)
			{
				return false;
			}
		}

		private bool IsValidPhone(string phone)
		{
			// evaluates as valid if contains 9 numbers + 2 whitespaces from input mask at the index 3 and 7		
			if (phone.Length == 11 && phone[3].Equals(' ') && phone[7].Equals(' '))
			{
				string phoneWithoutMask = phone.Remove(3, 1).Remove(6, 1);
				if (phoneWithoutMask.All(char.IsNumber))
				{
					return true;
				}
			}
			return false;
		}

		private bool IsValidPostalCode(string postalCode)
		{
			// evaluates as valid if contains 5 numbers + 1 whitespace from input mask at the index 3
			if (postalCode.Length == 6 && postalCode[3].Equals(' '))
			{
				string postalCodeWithoutMask = postalCode.Remove(3, 1);
				if (postalCodeWithoutMask.All(char.IsNumber))
				{
					return true;
				}
			}
			return false;
		}

        private void RunServerValidations(Person person)
        {
			if (!IsValidEmail(person.Email))
			{
				ModelState.AddModelError("Email", "Neplatný formát e-mailu");
			}
			if (!IsValidPhone(person.Phone))
			{
				ModelState.AddModelError("Phone", "Neplatný formát telefonu");
			}
			if (!IsValidPostalCode(person.PostalCode))
			{
				ModelState.AddModelError("PostalCode", "Neplatný formát PSČ");
			}
		}
	}
}
