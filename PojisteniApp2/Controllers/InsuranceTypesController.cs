using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PojisteniApp2.Data;
using PojisteniApp2.Models;

namespace PojisteniApp2.Controllers
{
    public class InsuranceTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InsuranceTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: InsuranceTypes
        public async Task<IActionResult> Index()
        {
              return _context.InsuranceType != null ? 
                          View(await _context.InsuranceType.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.InsuranceType'  is null.");
        }

        // GET: InsuranceTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.InsuranceType == null)
            {
                return NotFound();
            }

            var insuranceType = await _context.InsuranceType
                .FirstOrDefaultAsync(m => m.InsuranceTypeId == id);
            if (insuranceType == null)
            {
                return NotFound();
            }

            return View(insuranceType);
        }

        // GET: InsuranceTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: InsuranceTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InsuranceTypeId,InsuranceTypeName")] InsuranceType insuranceType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(insuranceType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(insuranceType);
        }

        // GET: InsuranceTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.InsuranceType == null)
            {
                return NotFound();
            }

            var insuranceType = await _context.InsuranceType.FindAsync(id);
            if (insuranceType == null)
            {
                return NotFound();
            }
            return View(insuranceType);
        }

        // POST: InsuranceTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InsuranceTypeId,InsuranceTypeName")] InsuranceType insuranceType)
        {
            if (id != insuranceType.InsuranceTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(insuranceType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InsuranceTypeExists(insuranceType.InsuranceTypeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(insuranceType);
        }

        // GET: InsuranceTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.InsuranceType == null)
            {
                return NotFound();
            }

            var insuranceType = await _context.InsuranceType
                .FirstOrDefaultAsync(m => m.InsuranceTypeId == id);
            if (insuranceType == null)
            {
                return NotFound();
            }

            return View(insuranceType);
        }

        // POST: InsuranceTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.InsuranceType == null)
            {
                return Problem("Entity set 'ApplicationDbContext.InsuranceType'  is null.");
            }
            var insuranceType = await _context.InsuranceType.FindAsync(id);
            if (insuranceType != null)
            {
                _context.InsuranceType.Remove(insuranceType);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InsuranceTypeExists(int id)
        {
          return (_context.InsuranceType?.Any(e => e.InsuranceTypeId == id)).GetValueOrDefault();
        }
    }
}
