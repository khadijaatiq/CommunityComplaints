using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CommunityComplaints.Models;

namespace CommunityComplaints.Controllers
{
    public class ComplaintsController : Controller
    {
        private readonly AppDbContext _context;

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
            if (roleId != 1) return RedirectToAction("Login", "Account");

            var query = _context.Complaints
                .Include(c => c.Department)
                .Include(c => c.Resident)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);
            if (!string.IsNullOrEmpty(urgency))
                query = query.Where(c => c.Urgency == urgency);
            if (!string.IsNullOrEmpty(category))
                query = query.Where(c => c.Category == category);
            if (departmentId.HasValue)
                query = query.Where(c => c.DepartmentId == departmentId);

            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "Name");
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
            ViewBag.Categories = new SelectList(new List<string>
            {
                "Plumbing","Electrical","Carpentry","HVAC",
                "Lock Issue","Gate Problem","Patrol Request",
                "Trash Collection","Pest Control","Recycling",
                "Water Supply","Electricity Outage","Gas Leak"
            });
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Complaint complaint)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            ModelState.Remove(nameof(complaint.Resident));
            ModelState.Remove(nameof(complaint.Department));
            ModelState.Remove(nameof(complaint.CreatedAt));
            ModelState.Remove(nameof(complaint.UpdatedAt));
            ModelState.Remove(nameof(complaint.ResolvedAt));
            ModelState.Remove(nameof(complaint.Urgency));
            ModelState.Remove(nameof(complaint.Status));

            if (ModelState.IsValid)
            {
                complaint.ResidentId = userId.Value;
                complaint.Status = "Open";
                complaint.Urgency = "Medium";
                complaint.CreatedAt = DateTime.Now;
                complaint.UpdatedAt = DateTime.Now;

                _context.Add(complaint);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyComplaints));
            }

            ViewBag.DepartmentId = new SelectList(_context.Departments, "DepartmentId", "Name");
            ViewBag.Categories = new SelectList(new List<string>
            {
                "Plumbing","Electrical","Carpentry","HVAC",
                "Lock Issue","Gate Problem","Patrol Request",
                "Trash Collection","Pest Control","Recycling",
                "Water Supply","Electricity Outage","Gas Leak"
            });
            return View(complaint);
        }

        // ================= DETAILS (RESIDENT) =================
        public async Task<IActionResult> Details(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var complaint = await _context.Complaints
                .Include(c => c.Department)
                .Include(c => c.Resident)
                .Include(c => c.Comments).ThenInclude(c => c.User)
                .Include(c => c.ResolutionStages).ThenInclude(s => s.Staff)
                .Include(c => c.Assignments).ThenInclude(a => a.Staff)
                .FirstOrDefaultAsync(c => c.ComplaintId == id);

            if (complaint == null) return NotFound();

            return View(complaint);
        }

        // ================= STAFF: MANAGE COMPLAINT =================
        public async Task<IActionResult> Manage(int id)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1) return RedirectToAction("Login", "Account");

            var complaint = await _context.Complaints
                .Include(c => c.Department)
                .Include(c => c.Resident)
                .Include(c => c.Comments).ThenInclude(c => c.User)
                .Include(c => c.ResolutionStages).ThenInclude(s => s.Staff)
                .Include(c => c.Assignments).ThenInclude(a => a.Staff)
                .FirstOrDefaultAsync(c => c.ComplaintId == id);

            if (complaint == null) return NotFound();

            // Staff users only for assignment
            var staffRoleId = await _context.Roles
                .Where(r => r.Name == "Staff")
                .Select(r => r.RoleId)
                .FirstOrDefaultAsync();

            ViewBag.StaffList = new SelectList(
                _context.Users.Where(u => u.RoleId == staffRoleId && u.IsActive),
                "UserId", "FullName"
            );
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "Name", complaint.DepartmentId);

            return View(complaint);
        }

        // ================= STAFF: UPDATE STATUS =================
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1) return RedirectToAction("Login", "Account");

            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            complaint.Status = status;
            complaint.UpdatedAt = DateTime.Now;
            if (status == "Resolved") complaint.ResolvedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage), new { id });
        }

        // ================= STAFF: UPDATE URGENCY =================
        [HttpPost]
        public async Task<IActionResult> UpdateUrgency(int id, string urgency)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1) return RedirectToAction("Login", "Account");

            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            complaint.Urgency = urgency;
            complaint.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage), new { id });
        }

        // ================= STAFF: UPDATE DEPARTMENT =================
        [HttpPost]
        public async Task<IActionResult> UpdateDepartment(int id, int? departmentId)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1) return RedirectToAction("Login", "Account");

            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            complaint.DepartmentId = departmentId;
            complaint.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage), new { id });
        }

        // ================= ADD COMMENTS =================

        [HttpPost]
        public async Task<IActionResult> AddComment(int complaintId, string message, bool isInternal = false)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (userId == null) return RedirectToAction("Login", "Account");

            // Residents CANNOT add internal notes
            if (roleId != 1)
            {
                isInternal = false;
            }

            var comment = new Comment
            {
                ComplaintId = complaintId,
                UserId = userId.Value,
                Message = message,
                IsInternal = isInternal,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage), new { id = complaintId });
        }

        // ================= STAFF: ASSIGN STAFF =================
        [HttpPost]
        public async Task<IActionResult> Assign(int complaintId, int staffId)
        {
            int? assignedBy = HttpContext.Session.GetInt32("UserId");
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1 || assignedBy == null) return RedirectToAction("Login", "Account");

            // Deactivate old assignment
            var existing = await _context.Assignments
                .Where(a => a.ComplaintId == complaintId && a.IsActive)
                .ToListAsync();

            foreach (var a in existing)
                a.IsActive = false;

            // Add new assignment
            _context.Assignments.Add(new Assignment
            {
                ComplaintId = complaintId,
                StaffId = staffId,
                AssignedBy = assignedBy.Value,
                IsActive = true,
                AssignedAt = DateTime.Now
            });

            // Move status to InProgress if still Open
            var complaint = await _context.Complaints.FindAsync(complaintId);
            if (complaint != null && complaint.Status == "Open")
            {
                complaint.Status = "InProgress";
                complaint.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage), new { id = complaintId });
        }

        // ================= CANCEL (RESIDENT) =================
        public async Task<IActionResult> Cancel(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            if (complaint.Status == "Open" && complaint.ResidentId == userId)
            {
                complaint.Status = "Cancelled";
                complaint.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyComplaints));
        }
    }
}