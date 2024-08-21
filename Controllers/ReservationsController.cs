using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeanScene.Data;
using BeanScene.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using BeanScene.Services;

namespace BeanScene.Controllers
{
    [Authorize(Roles = "Manager, Staff, Member")]
    public class ReservationsController : Controller
    {
        private readonly BSDBContext _context;
        private readonly IReservationService IReservation;
        public ReservationsController(BSDBContext context, IReservationService IRes)
        {
            _context = context;
            IReservation = IRes;
        }

        // GET: Reservations
        public async Task<IActionResult> Index(string searchString, string sortOrder, string statusSort, string tableSort)
        {
            //sorting
            ViewData["CurrentFilter"] = searchString;
            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["StatusSortParm"] = statusSort == "Status" ? "status_desc" : "Status";
            ViewData["TableSortParm"] = tableSort == "Table" ? "table_desc" : "Table";

            // Get the current user's email (this is the APPLICATIONUSER's email!!!!
            string userEmail = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            var reservationsQuery = IReservation.List()
                .Include(r => r.FKUser)
                .Include(r => r.FKTable)
                .Include(r => r.FKSitting)
                .AsQueryable(); // Ensure it's an IQueryable

            if (User.IsInRole("Member") && user != null)
            { //this is extremely important. when you log in or register, you log in or register as an applicationuser.
                //but user, not applicationusers, is used for booking. so, we copy applicationuser to a user profile when they register,
                //so that we can use anyone who registers as a member,
                //and then we compare the User Email to the Identity user email to filter what the 'member' sees when they've joined.
                reservationsQuery = reservationsQuery.Where(r => r.FKUser.Email == user.Email);
            }

            if (!String.IsNullOrEmpty(searchString))
            { //in this search string you can search any combination of lastname, firstname, or only lastname, firstname... etc
                var searchTerms = searchString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (searchTerms.Length == 1)
                {
                    var term = searchTerms[0];
                    reservationsQuery = reservationsQuery.Where(r => r.FKUser.FirstName.Contains(term)
                                                        || r.FKUser.LastName.Contains(term)
                                                        || r.FKTable.TableName.Contains(term));
                }
                else if (searchTerms.Length >= 2)
                {
                    var firstName = searchTerms[0];
                    var lastName = searchTerms[1];
                    reservationsQuery = reservationsQuery.Where(r => r.FKUser.FirstName.Contains(firstName)
                                                        && r.FKUser.LastName.Contains(lastName));
                }
            }

            // This is just to apply sorting
            switch (sortOrder)
            {
                case "date_desc":
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.Date);
                    break;
                default:
                    reservationsQuery = reservationsQuery.OrderBy(r => r.Date);
                    break;
            }

            switch (statusSort)
            {
                case "Status":
                    reservationsQuery = reservationsQuery.OrderBy(r => r.ResStatus);
                    break;
                case "status_desc":
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.ResStatus);
                    break;
            }

            switch (tableSort)
            {
                case "Table":
                    reservationsQuery = reservationsQuery.OrderBy(r => r.FKTable.TableName);
                    break;
                case "table_desc":
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.FKTable.TableName);
                    break;
            }

            var reservations = await reservationsQuery.ToListAsync();
            return View(reservations);
        }


        // GET: Reservations/Details/5
        [Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservation
                .Include(r => r.FKSitting)
                .Include(r => r.FKUser)
                .FirstOrDefaultAsync(m => m.ResId == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create

        [Authorize(Roles = "Manager, Staff, Member")]
        public async Task<IActionResult> Create()
        {
            // Get the current user's email (identity user, not guest user)
            string userEmail = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            List<SelectListItem> users;

            if (User.IsInRole("Member") && user != null)
            {
                // If the ApplicationUser is a Member, only show info linked to a User with their email address
                //remember, ApplicationUser =/= User
                users = _context.User
                                .Where(u => u.Email == user.Email)
                                .Select(u => new SelectListItem
                                { 
                                    Value = u.UserId.ToString(),
                                    Text = $"{u.UserId} - {u.FirstName} {u.LastName}"
                                })
                                .ToList();
            }
            else
            {
                // If the user is Manager or Staff, show all users
                users = _context.User
                                .Select(u => new SelectListItem
                                {
                                    Value = u.UserId.ToString(),
                                    Text = $"{u.UserId} - {u.FirstName} {u.LastName}"
                                })
                                .ToList();
            }

            // Get a list of sittings with their SittingId and SittingType
            var sittings = _context.Sitting
                                    .Select(s => new SelectListItem
                                    {
                                        Value = s.SittingId.ToString(),
                                        Text = $"{s.SittingType}"
                                    })
                                    .ToList();

            // Get a list of tables with their TableId and TableName
            var tables = _context.Table
                                 .Select(t => new SelectListItem
                                 {
                                     Value = t.TableId.ToString(),
                                     Text = t.TableName
                                 })
                                 .ToList();

            // Pass the lists to the view
            ViewData["UserId"] = users;
            ViewData["SittingId"] = sittings;
            ViewData["Tables"] = tables;

            return View();
        }


        private List<DateTime> CalculateAvailableTimeSlots(Reservation reservation)
        {
            // Get the selected sitting
            Sitting sitting = _context.Sitting.FirstOrDefault(s => s.SittingId == reservation.SittingId);

            // Get all reservations for the selected date and sitting
            List<Reservation> reservationsOnDate = _context.Reservation
                .Where(r => r.Date == reservation.Date &&
                            r.SittingId == reservation.SittingId &&
                            (r.ResStatus == resstatus.Pending ||
                             r.ResStatus == resstatus.Confirmed ||
                             r.ResStatus == resstatus.Seated ||
                             r.ResStatus == resstatus.Completed))
                .ToList();

            // This is to calculate the end time of the sitting
            DateTime sittingEndTime = new DateTime(
                reservation.Date.Year, reservation.Date.Month, reservation.Date.Day,
                sitting.EndTime.Hour, sitting.EndTime.Minute, 0);

            // Initialize the list of available time slots (this is linked to preventing double bookings)
            List<DateTime> availableTimeSlots = new List<DateTime>();

            // Calculate available time slots based on existing reservations (preventing double bookings)
            DateTime currentTime = new DateTime(
                reservation.Date.Year, reservation.Date.Month, reservation.Date.Day,
                sitting.StartTime.Hour, sitting.StartTime.Minute, 0);

            while (currentTime < sittingEndTime)
            {
                // Check if the current time slot is available (don't want double bookings)
                bool isAvailable = true;

                // Calculate end time of current time slot
                DateTime currentEndTime = currentTime.AddHours(reservation.Duration);

                foreach (var existingReservation in reservationsOnDate)
                {
                    // Skip cancelled reservations (you can book in a cancelled reservation's spot)
                    if (existingReservation.ResStatus == resstatus.Cancelled)
                        continue;

                    // Calculate end time of existing reservation
                    DateTime existingReservationEndTime = existingReservation.ResStartTime.AddHours(existingReservation.Duration);

                    // Check if there's any overlap with existing reservations
                    if ((currentTime >= existingReservation.ResStartTime && currentTime < existingReservationEndTime) ||
                        (currentEndTime > existingReservation.ResStartTime && currentEndTime <= existingReservationEndTime) ||
                        (currentTime <= existingReservation.ResStartTime && currentEndTime >= existingReservationEndTime))
                    {
                        isAvailable = false;
                        break;
                    }
                }

                // If the current time slot is available, add it to the list (so that the user can choose it)
                if (isAvailable)
                {
                    availableTimeSlots.Add(currentTime);
                }

                // Move to the next time slot
                currentTime = currentTime.AddHours(1);
            }

            return availableTimeSlots;
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Staff, Member")]
        public async Task<IActionResult> Create([Bind("ResId,UserId,SittingId,TableId,NumOfGuests,Date,ResStartTime,Duration,Source,SpecialReqs,ResStatus")] Reservation reservation)
        {
            if (User.IsInRole("Member"))
            {
                reservation.ResStatus = resstatus.Pending;
                reservation.Source = source.Online;
            }

            if (ModelState.IsValid)
            {
                // Check if the provided SittingId is valid
                if (!_context.Sitting.Any(s => s.SittingId == reservation.SittingId))
                {
                    ModelState.AddModelError("SittingId", "Invalid sitting selection.");
                    return View(reservation);
                }
                // Calculate available time slots
                List<DateTime> availableTimeSlots = CalculateAvailableTimeSlots(reservation);
                // Pass available time slots to the view
                ViewData["AvailableTimeSlots"] = availableTimeSlots;
                return View("SelectTimeSlot", reservation);

                //_context.Add(reservation);
                //await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
            }
            ViewData["TableId"] = new SelectList(_context.Table, "TableId", "TableId", reservation.TableId);
            ViewData["SittingId"] = new SelectList(_context.Sitting, "SittingId", "SittingId", reservation.SittingId);
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserId", reservation.UserId);
            return View(reservation);
        }

        public async Task<IActionResult> ConfirmReservation(Reservation reservation)
        {
            // Add the selected start time to the reservation
            reservation.Date = reservation.ResStartTime.Date;

            // Add the reservation to the database
            _context.Add(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservation.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            ViewData["TableId"] = new SelectList(_context.Table, "TableId", "TableId", reservation.TableId);
            ViewData["SittingId"] = new SelectList(_context.Sitting, "SittingId", "SittingId", reservation.SittingId);
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserId", reservation.UserId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> Edit(int id, [Bind("ResId,UserId,SittingId,TableId,NumOfGuests,Date,Duration,Source,SpecialReqs,ResStatus")] Reservation reservation)
        {
            if (id != reservation.ResId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.ResId))
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
            ViewData["TableId"] = new SelectList(_context.Table, "TableId", "TableId", reservation.TableId);
            ViewData["SittingId"] = new SelectList(_context.Sitting, "SittingId", "SittingId", reservation.SittingId);
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserId", reservation.UserId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        [Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservation
                .Include(r => r.FKSitting)
                .Include(r => r.FKUser)
                .FirstOrDefaultAsync(m => m.ResId == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservation.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservation.Remove(reservation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //i can't remember why i need this
        private bool ReservationExists(int id)
        {
            return _context.Reservation.Any(e => e.ResId == id);
        }
    }
}