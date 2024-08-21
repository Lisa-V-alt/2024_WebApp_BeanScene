using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeanScene.Data;
using BeanScene.Models;
using Microsoft.AspNetCore.Authorization;

namespace BeanScene.Controllers
{
    [Authorize(Roles = "Member, Manager, Staff")]
    public class TablesController : Controller
    {
        private readonly BSDBContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public TablesController(BSDBContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Tables
        public async Task<IActionResult> Index()
        {
            ViewBag.MainImage = GetImage("Main");
            ViewBag.OutsideImage = GetImage("Outside");
            ViewBag.BalconyImage = GetImage("Balcony");
            return View(await _context.Table.ToListAsync());
        }

        private string GetImage(string area)
        {
            var path = Path.Combine(_hostEnvironment.WebRootPath, "uploads", $"{area}.jpg");
            if (System.IO.File.Exists(path))
            {
                return $"/uploads/{area}.jpg";
            }
            return null;
        }

        // GET: Tables/Details/5
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _context.Table
                .FirstOrDefaultAsync(m => m.TableId == id);
            if (table == null)
            {
                return NotFound();
            }

            return View(table);
        }

        // GET: Tables/Create
        [Authorize(Roles = "Manager")]
        public IActionResult Create()
        {
            ViewBag.AreaList = new SelectList(Enum.GetValues(typeof(TableArea)).Cast<TableArea>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString()
            }).ToList(), "Value", "Text");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UploadImage(string area, IFormFile file)
        {
            if (file != null && (file.ContentType == "image/jpeg" || file.ContentType == "image/png"))
            {
                var fileName = $"{area}.jpg";
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

        // POST: Tables/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")] //this is extremely important: tableName is area and table merged for simplicity
        public async Task<IActionResult> Create([Bind("TableId,TableNo,Area,TableCapacity")] Table table)
        {
            ModelState.Remove("TableName");
            if (ModelState.IsValid)
            {
                table.UpdateTableName();  // Update TableName
                _context.Add(table);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(table);
        }

        // GET: Tables/Edit/5
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _context.Table.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }
            ViewBag.AreaList = new SelectList(Enum.GetValues(typeof(TableArea)).Cast<TableArea>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString()
            }).ToList(), "Value", "Text", table.Area); // Pass selected value as well

            return View(table);
        }

        // POST: Tables/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("TableId,TableNo,Area,TableCapacity")] Table table)
        {
            ModelState.Remove("TableName");

            if (id != table.TableId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    table.UpdateTableName();  // Update TableName
                    _context.Update(table);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TableExists(table.TableId))
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

            return View(table);
        }

        // GET: Tables/Delete/5
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _context.Table
                .FirstOrDefaultAsync(m => m.TableId == id);
            if (table == null)
            {
                return NotFound();
            }

            return View(table);
        }

        // POST: Tables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var table = await _context.Table.FindAsync(id);
            if (table != null)
            {
                _context.Table.Remove(table);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TableExists(int id)
        {
            return _context.Table.Any(e => e.TableId == id);
        }
    }
}
