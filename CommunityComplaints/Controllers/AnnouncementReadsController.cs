using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CommunityComplaints.Models;

namespace CommunityComplaints.Controllers
{
    public class AnnouncementReadsController : Controller
    {
        private readonly AppDbContext _context;

        public AnnouncementReadsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AnnouncementReads
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.AnnouncementReads.Include(a => a.Announcement).Include(a => a.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: AnnouncementReads/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcementRead = await _context.AnnouncementReads
                .Include(a => a.Announcement)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (announcementRead == null)
            {
                return NotFound();
            }

            return View(announcementRead);
        }

        // GET: AnnouncementReads/Create
        public IActionResult Create()
        {
            ViewData["AnnouncementId"] = new SelectList(_context.Announcements, "AnnouncementId", "AnnouncementId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: AnnouncementReads/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AnnouncementId,UserId,ReadAt")] AnnouncementRead announcementRead)
        {
            if (ModelState.IsValid)
            {
                _context.Add(announcementRead);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AnnouncementId"] = new SelectList(_context.Announcements, "AnnouncementId", "AnnouncementId", announcementRead.AnnouncementId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", announcementRead.UserId);
            return View(announcementRead);
        }

        // GET: AnnouncementReads/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcementRead = await _context.AnnouncementReads.FindAsync(id);
            if (announcementRead == null)
            {
                return NotFound();
            }
            ViewData["AnnouncementId"] = new SelectList(_context.Announcements, "AnnouncementId", "AnnouncementId", announcementRead.AnnouncementId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", announcementRead.UserId);
            return View(announcementRead);
        }

        // POST: AnnouncementReads/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AnnouncementId,UserId,ReadAt")] AnnouncementRead announcementRead)
        {
            if (id != announcementRead.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(announcementRead);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnnouncementReadExists(announcementRead.Id))
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
            ViewData["AnnouncementId"] = new SelectList(_context.Announcements, "AnnouncementId", "AnnouncementId", announcementRead.AnnouncementId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", announcementRead.UserId);
            return View(announcementRead);
        }

        // GET: AnnouncementReads/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcementRead = await _context.AnnouncementReads
                .Include(a => a.Announcement)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (announcementRead == null)
            {
                return NotFound();
            }

            return View(announcementRead);
        }

        // POST: AnnouncementReads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var announcementRead = await _context.AnnouncementReads.FindAsync(id);
            if (announcementRead != null)
            {
                _context.AnnouncementReads.Remove(announcementRead);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnnouncementReadExists(int id)
        {
            return _context.AnnouncementReads.Any(e => e.Id == id);
        }
    }
}
