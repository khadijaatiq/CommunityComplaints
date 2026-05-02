using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CommunityComplaints.Models;
using BCrypt.Net;

namespace CommunityComplaints.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = _context.Users.Include(u => u.Role);
            return View(await users.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string Password)
        {
            // Remove server-set fields from validation
            ModelState.Remove(nameof(user.PasswordHash));
            ModelState.Remove(nameof(user.CreatedAt));
            ModelState.Remove(nameof(user.Role));  // navigation property causes issues

            // FIX 5: Validate password before hashing — previously an empty/null
            // password was silently hashed and stored.
            if (string.IsNullOrWhiteSpace(Password))
                ModelState.AddModelError("Password", "Password is required");
            else if (Password.Length < 8)
                ModelState.AddModelError("Password", "Password must be at least 8 characters");

            if (!ModelState.IsValid)
            {
                ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "Name", user.RoleId);
                return View(user);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
            user.CreatedAt = DateTime.Now;
            user.IsActive = true;

            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "Name", user.RoleId);
            return View(user);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "Name", user.RoleId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user, string NewPassword)
        {
            if (id != user.UserId) return NotFound();

            ModelState.Remove(nameof(user.PasswordHash));
            ModelState.Remove(nameof(user.CreatedAt));
            ModelState.Remove(nameof(user.Role));

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserId == id);

                    if (existingUser == null) return NotFound();

                    // Preserve fields not on the form
                    user.CreatedAt = existingUser.CreatedAt;

                    // FIX 5b: Validate new password length/complexity before hashing
                    if (!string.IsNullOrWhiteSpace(NewPassword))
                    {
                        if (NewPassword.Length < 8)
                        {
                            ModelState.AddModelError("NewPassword", "Password must be at least 8 characters");
                            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "Name", user.RoleId);
                            return View(user);
                        }
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                    }
                    else
                        user.PasswordHash = existingUser.PasswordHash;

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.UserId == user.UserId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "Name", user.RoleId);
            return View(user);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();

            var loggedInUser = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == HttpContext.Session.GetInt32("UserId"));

            if (loggedInUser?.Role?.Name == "Staff" && user.Role?.Name == "Staff")
            {
                TempData["Error"] = "Staff members cannot delete other staff members.";
                return RedirectToAction(nameof(Index));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null) return NotFound();

            return View(user);
        }

    }
}