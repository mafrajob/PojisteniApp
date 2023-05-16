﻿using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PojisteniApp2.Data;
using PojisteniApp2.Helpers;
using PojisteniApp2.Models;

namespace PojisteniApp2.Controllers
{
    public class InsurancesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotyfService _notyf;
        private readonly string _genericErrorMessage = "Opravte zadaná data";

        public InsurancesController(ApplicationDbContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        // GET: Insurances
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            // Can be assigned whatever string you want, but the keyword needs to be consistent throughout this method
            string personCustomSortKeyword = "Person";

            // Sort based on tutorial: https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/advanced?view=aspnetcore-7.0#dynamic-linq
            ViewData["CurrentSort"] = sortOrder;
            ViewData["PersonSortParm"] = string.IsNullOrEmpty(sortOrder) ? $"{personCustomSortKeyword}_desc" : "";
            ViewData["AmountSortParm"] = sortOrder == "InsuranceAmount" ? "InsuranceAmount_desc" : "InsuranceAmount";
            ViewData["ValidFromSortParm"] = sortOrder == "ValidFrom" ? "ValidFrom_desc" : "ValidFrom";
            ViewData["ValidToSortParm"] = sortOrder == "ValidTo" ? "ValidTo_desc" : "ValidTo";
            // Paging based on tutorial: https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/sort-filter-page?view=aspnetcore-7.0#add-paging-to-students-index
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            // Search based on tutorial: https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/sort-filter-page?view=aspnetcore-7.0#add-filtering-functionality-to-the-index-method
            ViewData["CurrentFilter"] = searchString;

            var insurances = (IQueryable<Insurance>)_context.Insurance.Include(i => i.InsuranceType).Include(i => i.Person);

            if (!String.IsNullOrEmpty(searchString))
            {
                insurances = insurances.Where(e => e.Person.LastName.Contains(searchString) || e.Person.FirstName.Contains(searchString));
            }

            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = personCustomSortKeyword;
            }

            bool descending = false;
            if (sortOrder.EndsWith("_desc"))
            {
                // Trim _desc suffix
                sortOrder = sortOrder.Substring(0, sortOrder.Length - 5);
                descending = true;
            }

            if (descending)
            {
                // Custom sort by Person properties
                if (sortOrder == personCustomSortKeyword)
                {
                    insurances = insurances.OrderByDescending(e => e.Person.LastName)
                                            .ThenByDescending(e => e.Person.FirstName)
                                            .ThenByDescending(e => e.Person.City)
                                            .ThenByDescending(e => e.Person.Street);
                }
                // Generic sort by all other Insurance properties
                else
                {
                    insurances = insurances.OrderByDescending(e => EF.Property<object>(e, sortOrder));
                }
            }
            else
            {
                if (sortOrder == personCustomSortKeyword)
                {
                    insurances = insurances.OrderBy(e => e.Person.LastName)
                                            .ThenBy(e => e.Person.FirstName)
                                            .ThenBy(e => e.Person.City)
                                            .ThenBy(e => e.Person.Street);
                }
                else
                {
                    insurances = insurances.OrderBy(e => EF.Property<object>(e, sortOrder));
                }
            }

            int pageSize = 3;
            return View(await PaginatedList<Insurance>.CreateAsync(insurances.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Insurances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Insurance == null)
            {
                return NotFound();
            }

            var insurance = await _context.Insurance
                .Include(i => i.InsuranceType)
                .Include(i => i.Person)
                .FirstOrDefaultAsync(m => m.InsuranceId == id);
            if (insurance == null)
            {
                return NotFound();
            }

            return View(insurance);
        }

        // GET: Insurances/Create
        public IActionResult Create(int? id)
        {
            ViewData["InsuranceTypeId"] = CreateInsuranceTypeSelectList();
            ViewData["PersonId"] = CreatePersonSelectList(id); // Custom list of persons created by current user or all persons if current user is admin
            ViewData["IsDefinedPerson"] = false;
            ViewData["CustomTitle"] = "Nové pojištění";
            TempData["PreviousUrl"] = Request.Headers["Referer"].ToString(); // Saves URL user is coming from to be used in POST Create action
            // Create insurance instance and assign default values
            Insurance insurance = new Insurance();
            insurance.ValidFrom = DateTime.Now.Date;
            insurance.ValidTo = insurance.ValidFrom.AddYears(1);
            if (id != null) // PersonId was routed to this action
            {
                // Insurance will be created for person defined by PersonId
                ViewData["IsDefinedPerson"] = true;
                insurance.PersonId = (int)id;

                // Person name for the view title
                var person = _context.Person.Find(id);
                if (person != null)
                {
                    ViewData["CustomTitle"] = ViewData["CustomTitle"] + string.Format($" {person.FirstName.Substring(0, 1).ToUpper()}. {person.LastName}");
                }
            }

            // Assign string "disabled" if no user logged in
            ViewBag.Disabled = UserHelper.IsLoggedUser(User) ? string.Empty : "disabled";

            return View(insurance);
        }

        // POST: Insurances/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Create([Bind("InsuranceId,InsuranceTypeId,InsuranceAmount,InsuranceSubject,ValidFrom,ValidTo,PersonId")] Insurance insurance)
        {
            if (ModelState.IsValid)
            {
                // Set currently logged in user's id as AuthorId
                insurance.AuthorId = UserHelper.GetUserId(User);
                _context.Add(insurance);
                await _context.SaveChangesAsync();
                _notyf.Success("Nové pojištění přidáno");

                // Redirect to previous URL if available
                if (TryGetPreviousUrl(out string previousUrl))
                {
                    return Redirect(previousUrl);
                }

                // Redirect to defaul action People -> Details if condition above is not true
                return RedirectToAction("Details", "People", new { id = insurance.PersonId });
            }
            ViewData["InsuranceTypeId"] = CreateInsuranceTypeSelectList(insurance.InsuranceTypeId);
            ViewData["PersonId"] = CreatePersonSelectList(insurance.PersonId); // Custom list of persons created by current user or all persons if current user is admin
            ViewData["IsDefinedPerson"] = false;
            ViewData["CustomTitle"] = "Nové pojištění";
            // Person name for the view title
            var person = _context.Person.Find(insurance.PersonId);
            if (person != null)
            {
                ViewData["IsDefinedPerson"] = true;
                ViewData["CustomTitle"] = ViewData["CustomTitle"] + string.Format($" {person.FirstName.Substring(0, 1).ToUpper()}. {person.LastName}");
            }

            // No disabled functionality for POST method of Create action needed
            ViewBag.Disabled = string.Empty;

            _notyf.Error(_genericErrorMessage);
            return View(insurance);
        }

        // GET: Insurances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Insurance == null)
            {
                return NotFound();
            }

            var insurance = await _context.Insurance.FindAsync(id);
            if (insurance == null)
            {
                return NotFound();
            }
            ViewData["InsuranceTypeId"] = CreateInsuranceTypeSelectList(insurance.InsuranceTypeId);
            ViewData["PersonId"] = CreatePersonSelectList(insurance.PersonId); // Custom list of persons created by current user or all persons if current user is admin
            TempData["PreviousUrl"] = Request.Headers["Referer"].ToString(); // Saves URL user is coming from to be used in POST Edit action
            // Assign string "disabled" if current user is neither author nor admin
            ViewBag.Disabled = UserHelper.IsAuthorOrAdmin(User, insurance) ? string.Empty : "disabled";
            return View(insurance);
        }

        // POST: Insurances/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Edit(int id, [Bind("InsuranceId,InsuranceTypeId,InsuranceAmount,InsuranceSubject,ValidFrom,ValidTo,PersonId")] Insurance insurance)
        {
            if (id != insurance.InsuranceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set AuthorId from existing record, otherwise it will be set to NULL by following update
                    SetExistingAuthorId(insurance);
                    _context.Update(insurance);
                    await _context.SaveChangesAsync();
                    _notyf.Success($"Změněno pojištění ID {id}");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InsuranceExists(insurance.InsuranceId))
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

                // Redirect to defaul action Insurances -> Index if condition above is not true
                return RedirectToAction(nameof(Index));
            }
            ViewData["InsuranceTypeId"] = CreateInsuranceTypeSelectList(insurance.InsuranceTypeId);
            ViewData["PersonId"] = CreatePersonSelectList(insurance.PersonId); // Custom list of persons created by current user or all persons if current user is admin
            // No disabled functionality for POST method of Edit action needed
            ViewBag.Disabled = string.Empty;
            _notyf.Error(_genericErrorMessage);
            return View(insurance);
        }

        // GET: Insurances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Insurance == null)
            {
                return NotFound();
            }

            var insurance = await _context.Insurance
                .Include(i => i.InsuranceType)
                .Include(i => i.Person)
                .FirstOrDefaultAsync(m => m.InsuranceId == id);
            if (insurance == null)
            {
                return NotFound();
            }

            TempData["PreviousUrl"] = Request.Headers["Referer"].ToString(); // Saves URL user is coming from to be used in POST Delete action

            // Assign string "disabled" if current user is neither author nor admin
            ViewBag.Disabled = UserHelper.IsAuthorOrAdmin(User, insurance) ? string.Empty : "disabled";
            
            return View(insurance);
        }

        // POST: Insurances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Insurance == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Insurance'  is null.");
            }
            var insurance = await _context.Insurance.FindAsync(id);
            if (insurance != null)
            {
                _context.Insurance.Remove(insurance);
                _notyf.Success($"Smazáno pojištění ID {id}");
            }
            
            await _context.SaveChangesAsync();

            // Redirect to previous URL if available
            if (TryGetPreviousUrl(out string previousUrl))
            {
                return Redirect(previousUrl);
            }

            // Redirect to defaul action Insurances -> Index if condition above is not true
            return RedirectToAction(nameof(Index));
        }

        private bool InsuranceExists(int id)
        {
          return (_context.Insurance?.Any(e => e.InsuranceId == id)).GetValueOrDefault();
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

        private void SetExistingAuthorId(Insurance insurance)
        {
            // Find existing record in DB
            Insurance? existingInsurance = _context.Insurance.Find(insurance.InsuranceId);
            if (existingInsurance != null) // Insurance found
            {
                // Detach the existing insurance from the context to prevent error:
                // The instance of entity type 'Insurance' cannot be tracked because another instance
                // with the same key value for { 'InsuranceId'} is already being tracked.
                _context.Entry(existingInsurance).State = EntityState.Detached;
                insurance.AuthorId = existingInsurance.AuthorId;
            }
        }

        private List<SelectListItem> CreatePersonSelectList(int? selectedPersonId = null)
        {
            // Create a list of persons
            var persons = _context.Person;

            // Create a new list of SelectListItems that includes a dummy value
            List<SelectListItem> personList = new() { new SelectListItem { Value = "Vyber pojištěnce", Text = "-- Vyber pojištěnce --" } };

            // Add each person to the list of SelectListItems
            foreach (var person in persons)
            {
                // Add to select list only Persons created by current user or all Persons if current user is admin
                if (UserHelper.IsAdmin(User) || UserHelper.IsAuthor(User, person))
                {
                    // Create a new SelectListItem object for the person
                    SelectListItem item = new()
                    {
                        Value = person.PersonId.ToString(),
                        Text = person.FullNameWithAddress,
                        Selected = selectedPersonId == person.PersonId
                    };

                    // Add the SelectListItem to the list
                    personList.Add(item);
                }
            }

            return personList;
        }

        private List<SelectListItem> CreateInsuranceTypeSelectList(int? selectedInsuranceTypeId = null)
        {
            // Create a list of insurance types
            var insuranceTypes = _context.InsuranceType;

            // Create a new list of SelectListItems that includes a dummy value
            List<SelectListItem> insuranceTypeList = new() { new SelectListItem { Value = "Vyber typ pojištění", Text = "-- Vyber typ pojištění --" } };

            // Add each insurance type to the list of SelectListItems
            foreach (var insuranceType in insuranceTypes)
            {
                // Create a new SelectListItem object for the insurance type
                SelectListItem item = new()
                {
                    Value = insuranceType.InsuranceTypeId.ToString(),
                    Text = insuranceType.InsuranceTypeName,
                    Selected = selectedInsuranceTypeId == insuranceType.InsuranceTypeId
                };

                // Add the SelectListItem to the list
                insuranceTypeList.Add(item);
            }

            return insuranceTypeList;
        }
    }
}
