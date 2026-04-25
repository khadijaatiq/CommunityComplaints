using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommunityComplaints.Models;

namespace CommunityComplaints.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly AppDbContext _context;

        public AnnouncementsController(AppDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public async Task<IActionResult> Index()
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (roleId == null)
                return RedirectToAction("Login", "Account");

            var announcements = await _context.Announcements
                .Include(a => a.CreatedByNavigation)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            if (roleId == 1)
            {
                var readCounts = await _context.AnnouncementReads
                    .GroupBy(r => r.AnnouncementId)
                    .Select(g => new { AnnouncementId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.AnnouncementId, x => x.Count);

                ViewBag.ReadCounts = readCounts;
                return View(announcements);
            }

            if (roleId == 2)
            {
                var readIds = await _context.AnnouncementReads
                    .Where(r => r.UserId == userId)
                    .Select(r => r.AnnouncementId)
                    .ToListAsync();

                ViewBag.ReadIds = readIds;
                return View(announcements);
            }

            return RedirectToAction("Login", "Account");
        }

        // ================= CREATE (STAFF ONLY) =================
        public IActionResult Create()
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement model)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (roleId != 1 || userId == null)
                return RedirectToAction("Login", "Account");

            // Validate title
            if (string.IsNullOrWhiteSpace(model.Title) || model.Title.Trim().Length < 5)
                ModelState.AddModelError("Title", "Title must be at least 5 characters");

            // Validate body
            if (string.IsNullOrWhiteSpace(model.Body) || model.Body.Trim().Length < 10)
                ModelState.AddModelError("Body", "Announcement body must be at least 10 characters");

            // Validate expiry date is in the future if provided
            if (model.ExpiresAt.HasValue && model.ExpiresAt <= DateTime.Now)
                ModelState.AddModelError("ExpiresAt", "Expiry date must be in the future");

            // Remove navigation/server-set properties so ModelState doesn't
            // fail on fields that aren't part of the form submission
            ModelState.Remove(nameof(model.CreatedByNavigation));
            ModelState.Remove(nameof(model.AnnouncementReads));
            ModelState.Remove(nameof(model.CreatedBy));
            ModelState.Remove(nameof(model.CreatedAt));
            ModelState.Remove(nameof(model.IsActive));

            if (!ModelState.IsValid)
                return View(model);

            model.Title = model.Title.Trim();
            model.Body = model.Body.Trim();
            model.CreatedBy = userId.Value;
            model.CreatedAt = DateTime.Now;
            model.IsActive = true;

            _context.Announcements.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Announcement created successfully";
            return RedirectToAction(nameof(Index));
        }

        // ================= MARK AS READ (RESIDENT) =================
        public async Task<IActionResult> MarkRead(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Validate announcement exists
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();

            bool exists = await _context.AnnouncementReads
                .AnyAsync(x => x.AnnouncementId == id && x.UserId == userId.Value);

            if (!exists)
            {
                _context.AnnouncementReads.Add(new AnnouncementRead
                {
                    AnnouncementId = id,
                    UserId = userId.Value,
                    ReadAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ================= DEACTIVATE (STAFF ONLY) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1) return Unauthorized();

            var ann = await _context.Announcements.FirstOrDefaultAsync(x => x.AnnouncementId == id);
            if (ann == null) return NotFound();

            if (!ann.IsActive)
            {
                TempData["Error"] = "Announcement is already deactivated";
                return RedirectToAction(nameof(Index));
            }

            ann.IsActive = false;
            ann.ExpiresAt = DateTime.Now;

            _context.Announcements.Update(ann);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Announcement deactivated";
            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT =================
        public async Task<IActionResult> Edit(int id)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Login", "Account");

            var ann = await _context.Announcements.FindAsync(id);
            if (ann == null) return NotFound();

            return View(ann);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Announcement model)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(model.Title) || model.Title.Trim().Length < 5)
                ModelState.AddModelError("Title", "Title must be at least 5 characters");

            if (string.IsNullOrWhiteSpace(model.Body) || model.Body.Trim().Length < 10)
                ModelState.AddModelError("Body", "Body must be at least 10 characters");

            if (model.ExpiresAt.HasValue && model.ExpiresAt <= DateTime.Now)
                ModelState.AddModelError("ExpiresAt", "Expiry date must be in the future");

            ModelState.Remove(nameof(model.CreatedByNavigation));
            ModelState.Remove(nameof(model.AnnouncementReads));
            ModelState.Remove(nameof(model.CreatedBy));
            ModelState.Remove(nameof(model.CreatedAt));
            ModelState.Remove(nameof(model.IsActive));

            if (!ModelState.IsValid)
                return View(model);

            var ann = await _context.Announcements.FindAsync(model.AnnouncementId);
            if (ann == null) return NotFound();

            ann.Title = model.Title.Trim();
            ann.Body = model.Body.Trim();
            ann.ExpiresAt = model.ExpiresAt;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Announcement updated successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}