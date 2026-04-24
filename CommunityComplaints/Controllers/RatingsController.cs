using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommunityComplaints.Models;

namespace CommunityComplaints.Controllers
{
    public class RatingsController : Controller
    {
        private readonly AppDbContext _context;

        public RatingsController(AppDbContext context)
        {
            _context = context;
        }

        // ================= INDEX (ADMIN VIEW OPTIONAL) =================
        public async Task<IActionResult> Index()
        {
            var ratings = _context.Ratings
                .Include(r => r.Complaint)
                .Include(r => r.Resident);

            return View(await ratings.ToListAsync());
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var rating = await _context.Ratings
                .Include(r => r.Complaint)
                .Include(r => r.Resident)
                .FirstOrDefaultAsync(r => r.RatingId == id);

            if (rating == null)
                return NotFound();

            return View(rating);
        }

        // ================= GET: CREATE =================
        public async Task<IActionResult> Create(int complaintId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var complaint = await _context.Complaints.FindAsync(complaintId);

            if (complaint == null)
                return NotFound();

            if (complaint.Status != "Resolved")
                return Content("Rating allowed only after complaint is resolved.");

            // Prevent duplicate rating view access
            var alreadyRated = await _context.Ratings
                .AnyAsync(r => r.ComplaintId == complaintId && r.ResidentId == userId.Value);

            if (alreadyRated)
                return Content("You have already rated this complaint.");

            ViewBag.ComplaintId = complaintId;

            return View();
        }

        // ================= POST: CREATE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int ComplaintId, int Stars, string Feedback)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var complaint = await _context.Complaints.FindAsync(ComplaintId);

            if (complaint == null || complaint.Status != "Resolved")
                return Content("Not allowed");

            var exists = await _context.Ratings
                .AnyAsync(r => r.ComplaintId == ComplaintId && r.ResidentId == userId.Value);

            if (exists)
                return Content("Already rated");

            var rating = new Rating
            {
                ComplaintId = ComplaintId,
                Stars = Stars,
                Feedback = Feedback,
                ResidentId = userId.Value,
                CreatedAt = DateTime.Now
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyComplaints", "Complaints");
        }

        // ================= EDIT (OPTIONAL - RESTRICT IF YOU WANT) =================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var rating = await _context.Ratings.FindAsync(id);

            if (rating == null)
                return NotFound();

            return View(rating);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Rating rating)
        {
            if (id != rating.RatingId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(rating);

            try
            {
                _context.Update(rating);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Ratings.Any(r => r.RatingId == rating.RatingId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE (OPTIONAL - YOU CAN REMOVE IN PROJECT) =================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var rating = await _context.Ratings
                .Include(r => r.Complaint)
                .Include(r => r.Resident)
                .FirstOrDefaultAsync(r => r.RatingId == id);

            if (rating == null)
                return NotFound();

            return View(rating);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rating = await _context.Ratings.FindAsync(id);

            if (rating != null)
            {
                _context.Ratings.Remove(rating);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}