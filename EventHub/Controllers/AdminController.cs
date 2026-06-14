using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventHub.Data;
using EventHub.ViewModels;

namespace EventHub.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var model = new AdminDashboardViewModel
            {
                // Basic Statistics
                TotalEvents = await _context.Events.CountAsync(),
                ActiveEvents = await _context.Events.CountAsync(e => e.IsActive && e.DateTime >= now),
                TotalUsers = await _context.Users.CountAsync(),
                TotalRegistrations = await _context.EventRegistrations.CountAsync(),
                EventsThisMonth = await _context.Events.CountAsync(e => e.CreatedAt >= startOfMonth && e.CreatedAt <= endOfMonth),
                RegistrationsThisMonth = await _context.EventRegistrations.CountAsync(r => r.RegisteredAt >= startOfMonth && r.RegisteredAt <= endOfMonth),
                TotalRevenue = await _context.EventRegistrations
                    .Include(r => r.Event)
                    .Where(r => !r.Event.IsFree)
                    .SumAsync(r => r.Event.Price),

                // Recent Events
                RecentEvents = await _context.Events
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(5)
                    .Select(e => new RecentEventDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        DateTime = e.DateTime,
                        RegistrationsCount = e.Registrations!.Count,
                        Capacity = e.Capacity,
                        IsActive = e.IsActive
                    })
                    .ToListAsync(),

                // Recent Registrations
                RecentRegistrations = await _context.EventRegistrations
                    .Include(r => r.Event)
                    .Include(r => r.User)
                    .OrderByDescending(r => r.RegisteredAt)
                    .Take(10)
                    .Select(r => new RecentRegistrationDto
                    {
                        Id = r.Id,
                        EventName = r.Event != null ? r.Event.Name : "Unknown",
                        UserName = r.User != null ? r.User.UserName ?? "Unknown" : "Unknown",
                        RegisteredAt = r.RegisteredAt,
                        EventDateTime = r.Event != null ? r.Event.DateTime : DateTime.Now
                    })
                    .ToListAsync(),

                // Popular Categories
                PopularCategories = await _context.Events
                    .Where(e => e.IsActive)
                    .GroupBy(e => e.Category)
                    .Select(g => new PopularCategoryDto
                    {
                        Category = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(c => c.Count)
                    .Take(5)
                    .ToListAsync()
            };

            // Calculate percentages for categories
            var totalEventsWithCategory = model.PopularCategories.Sum(c => c.Count);
            foreach (var category in model.PopularCategories)
            {
                category.Percentage = totalEventsWithCategory > 0
                    ? (int)((double)category.Count / totalEventsWithCategory * 100)
                    : 0;
            }

            // Prepare chart data for registrations by month (last 6 months)
            var months = new List<string>();
            var registrationsData = new List<int>();

            for (int i = 5; i >= 0; i--)
            {
                var monthDate = now.AddMonths(-i);
                var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                months.Add(monthDate.ToString("MMM yyyy"));
                var count = await _context.EventRegistrations
                    .CountAsync(r => r.RegisteredAt >= monthStart && r.RegisteredAt <= monthEnd);
                registrationsData.Add(count);
            }

            model.RegistrationsByMonth = new ChartDataDto
            {
                Labels = months,
                Data = registrationsData
            };

            // Prepare chart data for events by category
            var categoryLabels = model.PopularCategories.Take(5).Select(c => c.Category).ToList();
            var categoryData = model.PopularCategories.Take(5).Select(c => c.Count).ToList();

            model.EventsByCategory = new ChartDataDto
            {
                Labels = categoryLabels,
                Data = categoryData
            };

            return View(model);
        }

        // GET: Admin/ManageEvents
        public async Task<IActionResult> ManageEvents(string search, string status)
        {
            var query = _context.Events.Include(e => e.Registrations).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => e.Name.Contains(search) || e.Location.Contains(search));
            }

            if (status == "active")
            {
                query = query.Where(e => e.IsActive);
            }
            else if (status == "inactive")
            {
                query = query.Where(e => !e.IsActive);
            }

            var events = await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
            ViewBag.SearchTerm = search;
            ViewBag.Status = status;

            return View(events);
        }

        // POST: Admin/ToggleEventStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleEventStatus(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem != null)
            {
                eventItem.IsActive = !eventItem.IsActive;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Event {(eventItem.IsActive ? "activated" : "deactivated")} successfully!";
            }
            return RedirectToAction(nameof(ManageEvents));
        }

        // GET: Admin/ManageUsers
        public async Task<IActionResult> ManageUsers(string search, string role)
        {
            var users = await _context.Users.ToListAsync();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.UserName?.Contains(search) == true ||
                                         u.Email?.Contains(search) == true).ToList();
            }

            ViewBag.SearchTerm = search;
            ViewBag.Role = role;

            return View(users);
        }

        // POST: Admin/ToggleUserStatus
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully!";
            }
            return RedirectToAction(nameof(ManageUsers));
        }
    }
}