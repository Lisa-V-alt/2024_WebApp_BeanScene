using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeanScene.Data;
using BeanScene.Models;
using Microsoft.AspNetCore.Authorization;

//this is NOT accountcontroller. this controls 'User', not ApplicationUser
//meaning that this controller controls only 'guests' and 'members' used in reservations,
//it does NOT control manager or staff roles. it does not control roles at all, just membership/guest status

namespace BeanScene.Controllers
{
    [Authorize(Roles = "Manager, Staff")]
    public class UsersController : Controller
    {
        private readonly BSDBContext _context;

        public UsersController(BSDBContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var users = from u in _context.User
                        select u;

            if (!String.IsNullOrEmpty(searchString))
            {
                var searchTerms = searchString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (searchTerms.Length == 1)
                {
                    var term = searchTerms[0];
                    users = users.Where(u => u.FirstName.Contains(term)
                                          || u.LastName.Contains(term));
                }
                else if (searchTerms.Length >= 2)
                {
                    var firstName = searchTerms[0];
                    var lastName = searchTerms[1];
                    users = users.Where(u => u.FirstName.Contains(firstName)
                                          && u.LastName.Contains(lastName));
                }
            }

            return View(await users.ToListAsync());
        }

        // GET: Users/Details/5
        [Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        [Authorize(Roles = "Manager, Staff")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> Create([Bind("UserId,FirstName,LastName,Phone,Email,Membership")] User user)
        {
            if (ModelState.IsValid)
            {
                // Check if the email or phone number already exists (entries have to be unique)
                if (_context.User.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "This email is already in use.");
                    return View(user);
                }

                if (_context.User.Any(u => u.Phone == user.Phone))
                {
                    ModelState.AddModelError("Phone", "This phone number is already in use.");
                    return View(user);
                }
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
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
            return View(user);
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UserId == id);
        }
    }
}
