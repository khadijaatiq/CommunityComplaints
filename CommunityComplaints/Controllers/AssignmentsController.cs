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
    public class AssignmentsController : Controller
    {
        private readonly AppDbContext _context;

        public AssignmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Assignments
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Assignments.Include(a => a.AssignedByNavigation).Include(a => a.Complaint).Include(a => a.Staff);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Assignments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments
                .Include(a => a.AssignedByNavigation)
                .Include(a => a.Complaint)
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(m => m.AssignmentId == id);
            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // GET: Assignments/Create
        public IActionResult Create()
        {
            ViewData["AssignedBy"] = new SelectList(_context.Users, "UserId", "UserId");
            ViewData["ComplaintId"] = new SelectList(_context.Complaints, "ComplaintId", "ComplaintId");
            ViewData["StaffId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Assignments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AssignmentId,ComplaintId,StaffId,AssignedBy,IsActive,AssignedAt")] Assignment assignment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(assignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssignedBy"] = new SelectList(_context.Users, "UserId", "UserId", assignment.AssignedBy);
            ViewData["ComplaintId"] = new SelectList(_context.Complaints, "ComplaintId", "ComplaintId", assignment.ComplaintId);
            ViewData["StaffId"] = new SelectList(_context.Users, "UserId", "UserId", assignment.StaffId);
            return View(assignment);
        }

        // GET: Assignments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }
            ViewData["AssignedBy"] = new SelectList(_context.Users, "UserId", "UserId", assignment.AssignedBy);
            ViewData["ComplaintId"] = new SelectList(_context.Complaints, "ComplaintId", "ComplaintId", assignment.ComplaintId);
            ViewData["StaffId"] = new SelectList(_context.Users, "UserId", "UserId", assignment.StaffId);
            return View(assignment);
        }

        // POST: Assignments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AssignmentId,ComplaintId,StaffId,AssignedBy,IsActive,AssignedAt")] Assignment assignment)
        {
            if (id != assignment.AssignmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssignmentExists(assignment.AssignmentId))
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
            ViewData["AssignedBy"] = new SelectList(_context.Users, "UserId", "UserId", assignment.AssignedBy);
            ViewData["ComplaintId"] = new SelectList(_context.Complaints, "ComplaintId", "ComplaintId", assignment.ComplaintId);
            ViewData["StaffId"] = new SelectList(_context.Users, "UserId", "UserId", assignment.StaffId);
            return View(assignment);
        }

        // GET: Assignments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments
                .Include(a => a.AssignedByNavigation)
                .Include(a => a.Complaint)
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(m => m.AssignmentId == id);
            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // POST: Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment != null)
            {
                _context.Assignments.Remove(assignment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AssignmentExists(int id)
        {
            return _context.Assignments.Any(e => e.AssignmentId == id);
        }
    }
}
