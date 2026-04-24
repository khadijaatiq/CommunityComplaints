using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommunityComplaints.Models;

namespace CommunityComplaints.Controllers
{
    public class ResolutionStagesController : Controller
    {
        private readonly AppDbContext _context;

        public ResolutionStagesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int complaintId, string title, string description)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1 || userId == null) return RedirectToAction("Login", "Account");

            _context.ResolutionStages.Add(new ResolutionStage
            {
                ComplaintId = complaintId,
                StaffId = userId.Value,
                Title = title,
                Description = description,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("Manage", "Complaints", new { id = complaintId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int complaintId)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1) return RedirectToAction("Login", "Account");

            var stage = await _context.ResolutionStages.FindAsync(id);
            if (stage != null)
                _context.ResolutionStages.Remove(stage);

            await _context.SaveChangesAsync();
            return RedirectToAction("Manage", "Complaints", new { id = complaintId });
        }
    }
}