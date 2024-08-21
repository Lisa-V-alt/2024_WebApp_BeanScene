using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeanScene.Data;
using BeanScene.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;

namespace BeanScene.Controllers
{
    [Authorize(Roles = "Manager, Staff, Member")]
    public class SittingsController : Controller
    {
        private readonly BSDBContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public SittingsController(BSDBContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Sittings
        public async Task<IActionResult> Index()
        {
            ViewBag.BreakfastImage = GetImage("Breakfast");
            ViewBag.LunchImage = GetImage("Lunch");
            ViewBag.DinnerImage = GetImage("Dinner");
            return View(await _context.Sitting.ToListAsync());
        }

        private string GetImage(string sitting)
        {
            var path = Path.Combine(_hostEnvironment.WebRootPath, "uploads", $"{sitting}.jpg");
            if (System.IO.File.Exists(path))
            {
                return $"/uploads/{sitting}.jpg";
            }
            return null;
        }

        // GET: Sittings/Details/5
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sitting = await _context.Sitting
                .FirstOrDefaultAsync(m => m.SittingId == id);
            if (sitting == null)
            {
                return NotFound();
            }

            return View(sitting);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UploadImage(string sitting, IFormFile file)
        {
            if (file != null && (file.ContentType == "image/jpeg" || file.ContentType == "image/png"))
            {
                var fileName = $"{sitting}.jpg";
                var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, fileName);

                // Delete the existing file if it exists
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Save the new file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            return RedirectToAction(nameof(Index));
        }


        // GET: Sittings/Create
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Sittings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create([Bind("SittingId,SittingType,StartTime,EndTime,TotalCapacity,SittingStatus")] Sitting sitting)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sitting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sitting);
        }

        // GET: Sittings/Edit/5
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sitting = await _context.Sitting.FindAsync(id);
            if (sitting == null)
            {
                return NotFound();
            }
            return View(sitting);
        }

        // POST: Sittings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("SittingId,SittingType,StartTime,EndTime,TotalCapacity,SittingStatus")] Sitting sitting)
        {
            if (id != sitting.SittingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sitting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SittingExists(sitting.SittingId))
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
            return View(sitting);
        }

        // GET: Sittings/Delete/5
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sitting = await _context.Sitting
                .FirstOrDefaultAsync(m => m.SittingId == id);
            if (sitting == null)
            {
                return NotFound();
            }

            return View(sitting);
        }

        // POST: Sittings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sitting = await _context.Sitting.FindAsync(id);
            if (sitting != null)
            {
                _context.Sitting.Remove(sitting);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SittingExists(int id)
        {
            return _context.Sitting.Any(e => e.SittingId == id);
        }
    }
}
