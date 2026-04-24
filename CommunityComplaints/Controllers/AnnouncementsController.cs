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

            // ================= STAFF VIEW =================
            if (roleId == 1)
            {
                var readCounts = await _context.AnnouncementReads
                    .GroupBy(r => r.AnnouncementId)
                    .Select(g => new
                    {
                        AnnouncementId = g.Key,
                        Count = g.Count()
                    })
                    .ToDictionaryAsync(x => x.AnnouncementId, x => x.Count);

                ViewBag.ReadCounts = readCounts;
                return View(announcements);
            }

            // ================= RESIDENT VIEW =================
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
        public async Task<IActionResult> Create(Announcement model)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (roleId != 1 || userId == null)
                return RedirectToAction("Login", "Account");

            model.CreatedBy = userId.Value;
            model.CreatedAt = DateTime.Now;
            model.IsActive = true;

            _context.Announcements.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= MARK AS READ (RESIDENT) =================
        public async Task<IActionResult> MarkRead(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

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
        public async Task<IActionResult> Deactivate(int id)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1) return Unauthorized();

            var ann = await _context.Announcements.FirstOrDefaultAsync(x => x.AnnouncementId == id);

            if (ann == null)
                return NotFound();

            ann.IsActive = false;
            ann.ExpiresAt = DateTime.Now; // IMPORTANT (forces UI to show expired)

            _context.Announcements.Update(ann);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT =================
        public async Task<IActionResult> Edit(int id)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId != 1)
                return RedirectToAction("Login", "Account");

            var ann = await _context.Announcements.FindAsync(id);

            if (ann == null)
                return NotFound();

            return View(ann);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Announcement model)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId != 1)
                return RedirectToAction("Login", "Account");

            var ann = await _context.Announcements.FindAsync(model.AnnouncementId);

            if (ann == null)
                return NotFound();

            ann.Title = model.Title;
            ann.Body = model.Body;
            ann.ExpiresAt = model.ExpiresAt;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}