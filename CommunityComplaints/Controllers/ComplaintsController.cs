using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CommunityComplaints.Models;

namespace CommunityComplaints.Controllers
{
    public class ComplaintsController : Controller
    {
        private readonly AppDbContext _context;

        private static readonly List<string> ValidStatuses = new() { "Open", "InProgress", "Resolved", "Cancelled" };
        private static readonly List<string> ValidUrgencies = new() { "Low", "Medium", "High", "Critical" };
        private static readonly List<string> ValidCategories = new()
        {
            "Plumbing","Electrical","Carpentry","HVAC",
            "Lock Issue","Gate Problem","Patrol Request",
            "Trash Collection","Pest Control","Recycling",
            "Water Supply","Electricity Outage","Gas Leak"
        };

        public ComplaintsController(AppDbContext context)
        {
            _context = context;
        }

        // ================= RESIDENT: MY COMPLAINTS =================
        public async Task<IActionResult> MyComplaints()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var complaints = await _context.Complaints
                .Include(c => c.Department)
                .Where(c => c.ResidentId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var ratedComplaints = await _context.Ratings
                .Where(r => r.ResidentId == userId)
                .Select(r => r.ComplaintId)
                .ToListAsync();

            ViewBag.RatedComplaints = ratedComplaints;
            return View(complaints);
        }

        // ================= STAFF: ALL COMPLAINTS =================
        public async Task<IActionResult> Index(string status, string urgency, string category, int? departmentId)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            // FIX 4: unauthenticated → Login; authenticated but wrong role → Forbid (403)
            if (roleId == null) return RedirectToAction("Login", "Account");
            if (roleId != 1) return Forbid();

            var query = _context.Complaints
                .Include(c => c.Department)
                .Include(c => c.Resident)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && ValidStatuses.Contains(status))
                query = query.Where(c => c.Status == status);
            if (!string.IsNullOrEmpty(urgency) && ValidUrgencies.Contains(urgency))
                query = query.Where(c => c.Urgency == urgency);
            if (!string.IsNullOrEmpty(category) && ValidCategories.Contains(category))
                query = query.Where(c => c.Category == category);
            if (departmentId.HasValue && departmentId > 0)
                query = query.Where(c => c.DepartmentId == departmentId);

            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "Name");
            ViewBag.ValidStatuses = ValidStatuses;
            ViewBag.ValidUrgencies = ValidUrgencies;
            ViewBag.ValidCategories = ValidCategories;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedUrgency = urgency;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedDepartment = departmentId;

            return View(await query.OrderByDescending(c => c.CreatedAt).ToListAsync());
        }

        // ================= CREATE (RESIDENT ONLY) =================
        public IActionResult Create()
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 2) return RedirectToAction("Login", "Account");

            ViewBag.DepartmentId = new SelectList(_context.Departments, "DepartmentId", "Name");
            ViewBag.Categories = ValidCategories;
            ViewBag.Urgencies = ValidUrgencies;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Complaint complaint)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (userId == null) return RedirectToAction("Login", "Account");
            if (roleId != 2) return Forbid();

            ModelState.Remove(nameof(complaint.Resident));
            ModelState.Remove(nameof(complaint.Department));
            ModelState.Remove(nameof(complaint.CreatedAt));
            ModelState.Remove(nameof(complaint.UpdatedAt));
            ModelState.Remove(nameof(complaint.ResolvedAt));
            ModelState.Remove(nameof(complaint.Status));

            // Validate title length
            if (string.IsNullOrWhiteSpace(complaint.Title) || complaint.Title.Trim().Length < 5)
                ModelState.AddModelError("Title", "Title must be at least 5 characters");

            // Validate description length
            if (string.IsNullOrWhiteSpace(complaint.Description) || complaint.Description.Trim().Length < 20)
                ModelState.AddModelError("Description", "Description must be at least 20 characters");

            // FIX 9: Also enforce max length in the custom validation path
            // (the [StringLength(1000)] model annotation covers EF but not this path)
            if (!string.IsNullOrWhiteSpace(complaint.Description) && complaint.Description.Trim().Length > 1000)
                ModelState.AddModelError("Description", "Description cannot exceed 1000 characters");

            // Validate category is from allowed list
            if (string.IsNullOrWhiteSpace(complaint.Category) || !ValidCategories.Contains(complaint.Category))
                ModelState.AddModelError("Category", "Please select a valid category");

            // Validate urgency is from allowed list
            if (string.IsNullOrWhiteSpace(complaint.Urgency) || !ValidUrgencies.Contains(complaint.Urgency))
                ModelState.AddModelError("Urgency", "Please select a valid urgency level");

            if (!ModelState.IsValid)
            {
                ViewBag.DepartmentId = new SelectList(_context.Departments, "DepartmentId", "Name");
                ViewBag.Categories = ValidCategories;
                ViewBag.Urgencies = ValidUrgencies;
                return View(complaint);
            }

            complaint.ResidentId = userId.Value;
            complaint.Status = "Open";
            complaint.Title = complaint.Title.Trim();
            complaint.Description = complaint.Description.Trim();
            complaint.CreatedAt = DateTime.Now;
            complaint.UpdatedAt = DateTime.Now;

            _context.Add(complaint);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Complaint submitted successfully!";
            return RedirectToAction(nameof(MyComplaints));
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (userId == null) return RedirectToAction("Login", "Account");

            var complaint = await _context.Complaints
                .Include(c => c.Department)
                .Include(c => c.Resident)
                .Include(c => c.Comments).ThenInclude(c => c.User)
                .Include(c => c.ResolutionStages).ThenInclude(s => s.Staff)
                .Include(c => c.Assignments).ThenInclude(a => a.Staff)
                .FirstOrDefaultAsync(c => c.ComplaintId == id);

            if (complaint == null) return NotFound();

            // Residents can only view their own complaints
            if (roleId == 2 && complaint.ResidentId != userId)
                return Forbid();

            return View(complaint);
        }

        // ================= STAFF: MANAGE COMPLAINT =================
        public async Task<IActionResult> Manage(int id)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId == null) return RedirectToAction("Login", "Account");
            if (roleId != 1) return Forbid();

            var complaint = await _context.Complaints
                .Include(c => c.Department)
                .Include(c => c.Resident)
                .Include(c => c.Comments).ThenInclude(c => c.User)
                .Include(c => c.ResolutionStages).ThenInclude(s => s.Staff)
                .Include(c => c.Assignments).ThenInclude(a => a.Staff)
                .FirstOrDefaultAsync(c => c.ComplaintId == id);

            if (complaint == null) return NotFound();

            var staffRoleId = await _context.Roles
                .Where(r => r.Name == "Staff")
                .Select(r => r.RoleId)
                .FirstOrDefaultAsync();

            ViewBag.StaffList = new SelectList(
                _context.Users.Where(u => u.RoleId == staffRoleId && u.IsActive),
                "UserId", "FullName"
            );
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "Name", complaint.DepartmentId);
            ViewBag.ValidStatuses = ValidStatuses;
            ViewBag.ValidUrgencies = ValidUrgencies;

            return View(complaint);
        }

        // ================= STAFF: UPDATE STATUS =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId == null) return RedirectToAction("Login", "Account");
            if (roleId != 1) return Forbid();

            if (!ValidStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid status value";
                return RedirectToAction(nameof(Manage), new { id });
            }

            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            // Cannot change status of already cancelled complaint
            if (complaint.Status == "Cancelled")
            {
                TempData["Error"] = "Cannot update a cancelled complaint";
                return RedirectToAction(nameof(Manage), new { id });
            }

            complaint.Status = status;
            complaint.UpdatedAt = DateTime.Now;
            if (status == "Resolved") complaint.ResolvedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Status updated successfully";
            return RedirectToAction(nameof(Manage), new { id });
        }

        // ================= STAFF: UPDATE URGENCY =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUrgency(int id, string urgency)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId == null) return RedirectToAction("Login", "Account");
            if (roleId != 1) return Forbid();

            if (!ValidUrgencies.Contains(urgency))
            {
                TempData["Error"] = "Invalid urgency value";
                return RedirectToAction(nameof(Manage), new { id });
            }

            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            complaint.Urgency = urgency;
            complaint.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Urgency updated successfully";
            return RedirectToAction(nameof(Manage), new { id });
        }

        // ================= STAFF: UPDATE DEPARTMENT =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDepartment(int id, int? departmentId)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId == null) return RedirectToAction("Login", "Account");
            if (roleId != 1) return Forbid();

            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            // Validate department exists if provided
            if (departmentId.HasValue)
            {
                bool deptExists = await _context.Departments.AnyAsync(d => d.DepartmentId == departmentId);
                if (!deptExists)
                {
                    TempData["Error"] = "Selected department does not exist";
                    return RedirectToAction(nameof(Manage), new { id });
                }
            }

            complaint.DepartmentId = departmentId;
            complaint.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Department updated successfully";
            return RedirectToAction(nameof(Manage), new { id });
        }

        // ================= ADD COMMENT =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int complaintId, string message, bool isInternal = false)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (userId == null) return RedirectToAction("Login", "Account");

            // Validate message not empty
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Comment cannot be empty";
                // FIX 3: Residents must go to Details (not Manage which is staff-only)
                return roleId == 1
                    ? RedirectToAction(nameof(Manage), new { id = complaintId })
                    : RedirectToAction(nameof(Details), new { id = complaintId });
            }

            // Validate message length
            if (message.Trim().Length > 1000)
            {
                TempData["Error"] = "Comment cannot exceed 1000 characters";
                return roleId == 1
                    ? RedirectToAction(nameof(Manage), new { id = complaintId })
                    : RedirectToAction(nameof(Details), new { id = complaintId });
            }

            // Validate complaint exists
            var complaint = await _context.Complaints.FindAsync(complaintId);
            if (complaint == null) return NotFound();

            // Residents can only comment on their own complaints
            if (roleId == 2 && complaint.ResidentId != userId)
                return Forbid();

            // Residents cannot add internal notes
            if (roleId != 1) isInternal = false;

            var comment = new Comment
            {
                ComplaintId = complaintId,
                UserId = userId.Value,
                Message = message.Trim(),
                IsInternal = isInternal,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Comment added";

            if (roleId == 1)
                return RedirectToAction(nameof(Manage), new { id = complaintId });
            else
                return RedirectToAction(nameof(Details), new { id = complaintId });
        }

        // ================= STAFF: ASSIGN STAFF =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int complaintId, int staffId)
        {
            int? assignedBy = HttpContext.Session.GetInt32("UserId");
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1 || assignedBy == null) return RedirectToAction("Login", "Account");

            // Validate staff user exists and is active
            var staffUser = await _context.Users.FindAsync(staffId);
            if (staffUser == null || !staffUser.IsActive)
            {
                TempData["Error"] = "Selected staff member is not valid";
                return RedirectToAction(nameof(Manage), new { id = complaintId });
            }

            // Validate complaint exists
            var complaint = await _context.Complaints.FindAsync(complaintId);
            if (complaint == null) return NotFound();

            // Cannot assign resolved or cancelled complaints
            if (complaint.Status == "Resolved" || complaint.Status == "Cancelled")
            {
                TempData["Error"] = "Cannot assign staff to a resolved or cancelled complaint";
                return RedirectToAction(nameof(Manage), new { id = complaintId });
            }

            // Deactivate old assignments
            var existing = await _context.Assignments
                .Where(a => a.ComplaintId == complaintId && a.IsActive)
                .ToListAsync();
            foreach (var a in existing) a.IsActive = false;

            _context.Assignments.Add(new Assignment
            {
                ComplaintId = complaintId,
                StaffId = staffId,
                AssignedBy = assignedBy.Value,
                IsActive = true,
                AssignedAt = DateTime.Now
            });

            if (complaint.Status == "Open")
            {
                complaint.Status = "InProgress";
                complaint.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Staff assigned successfully";
            return RedirectToAction(nameof(Manage), new { id = complaintId });
        }

        // ================= CANCEL (RESIDENT ONLY) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            // Only owner can cancel
            if (complaint.ResidentId != userId)
                return Forbid();

            // Can only cancel Open complaints
            if (complaint.Status != "Open")
            {
                TempData["Error"] = "Only open complaints can be cancelled";
                return RedirectToAction(nameof(MyComplaints));
            }

            complaint.Status = "Cancelled";
            complaint.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Complaint cancelled";
            return RedirectToAction(nameof(MyComplaints));
        }
    }
}