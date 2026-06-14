using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventHub.Data;
using EventHub.Models;
using System.Security.Claims;

namespace EventHub.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Event - Everyone can view events
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Where(e => e.IsActive && e.DateTime >= DateTime.Now)
                .OrderBy(e => e.DateTime)
                .ToListAsync();
            return View(events);
        }

        // GET: Event/Details/5 - Everyone can view details
        public async Task<IActionResult> Details(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
                return NotFound();

            // Check if current user is registered
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.IsRegistered = await _context.EventRegistrations
                    .AnyAsync(r => r.EventId == id && r.UserId == userId);
            }

            return View(eventItem);
        }

        // GET: Event/Create - Admin only
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Event/Create - Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventItem)
        {
            if (ModelState.IsValid)
            {
                // If event is free, set price to 0
                if (eventItem.IsFree)
                    eventItem.Price = 0;

                _context.Add(eventItem);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(eventItem);
        }

        // GET: Event/Edit/5 - Admin only
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
                return NotFound();

            return View(eventItem);
        }

        // POST: Event/Edit/5 - Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventItem)
        {
            if (id != eventItem.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (eventItem.IsFree)
                        eventItem.Price = 0;

                    _context.Update(eventItem);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Event updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventItem.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(eventItem);
        }

        // GET: Event/Delete/5 - Admin only
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
                return NotFound();

            return View(eventItem);
        }

        // POST: Event/Delete/5 - Admin only
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem != null)
            {
                eventItem.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Event/Register/5 - Any logged-in user can register
        [Authorize]
        public async Task<IActionResult> Register(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if already registered
            var existingRegistration = await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == id && r.UserId == userId);

            if (existingRegistration != null)
            {
                TempData["Error"] = "You are already registered for this event!";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Check capacity
            var registeredCount = await _context.EventRegistrations
                .CountAsync(r => r.EventId == id);

            if (registeredCount >= eventItem.Capacity)
            {
                TempData["Error"] = "Sorry, this event is full!";
                return RedirectToAction(nameof(Details), new { id });
            }

            var registration = new EventRegistration
            {
                EventId = id,
                UserId = userId
            };

            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Successfully registered for the event!";
            return RedirectToAction(nameof(Details), new { id });
        }

      
        // GET: Event/MyEvents - Regular users can see events they registered for
        [Authorize]
        public async Task<IActionResult> MyEvents()
        {
            // Redirect Admin users - they don't need a My Events page
            if (User.IsInRole("Admin"))
            {
                TempData["Info"] = "Admins don't have a My Events page. Use Create Event to manage your events.";
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var myEvents = await _context.EventRegistrations
                .Include(r => r.Event)
                .Where(r => r.UserId == userId && r.Event.IsActive)
                .OrderBy(r => r.Event.DateTime)
                .Select(r => r.Event)
                .ToListAsync();

            return View(myEvents);
        }        // POST: Event/CancelRegistration/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRegistration(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var registration = await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == id && r.UserId == userId);

            if (registration == null)
            {
                TempData["Error"] = "Registration not found.";
                return RedirectToAction(nameof(MyEvents));
            }

            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem != null && eventItem.DateTime <= DateTime.Now)
            {
                TempData["Error"] = "Cannot cancel registration for past events.";
                return RedirectToAction(nameof(MyEvents));
            }

            _context.EventRegistrations.Remove(registration);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Registration cancelled successfully.";
            return RedirectToAction(nameof(MyEvents));
        }


        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}