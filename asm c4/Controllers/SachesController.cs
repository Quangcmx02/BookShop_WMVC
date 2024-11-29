using asm_c4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace asm_c4.Controllers
{
    public class SachesController : Controller
    {
        private readonly QuanLySachContext _context;

        public SachesController(QuanLySachContext context)
        {
            _context = context;
        }

        // GET: Saches
        public async Task<IActionResult> Index()
        {
            var quanLySachContext = _context.Saches.Include(s => s.DanhMuc);
            return View(await quanLySachContext.ToListAsync());
        }

        // GET: Saches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sach = await _context.Saches
                .Include(s => s.DanhMuc)
                .FirstOrDefaultAsync(m => m.SachId == id);
            if (sach == null)
            {
                return NotFound();
            }

            return View(sach);
        }

        // GET: Saches/Create
        public IActionResult Create()
        {
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "DanhMucId", "DanhMucId");
            return View();
        }

        // POST: Saches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SachId,TenSach,MoTa,HinhAnh,GiaTien,SoLuong,TrangThai,DanhMucId")] Sach sach)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sach);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "DanhMucId", "DanhMucId", sach.DanhMucId);
            return View(sach);
        }

        // GET: Saches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sach = await _context.Saches.FindAsync(id);
            if (sach == null)
            {
                return NotFound();
            }
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "DanhMucId", "DanhMucId", sach.DanhMucId);
            return View(sach);
        }

        // POST: Saches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SachId,TenSach,MoTa,HinhAnh,GiaTien,SoLuong,TrangThai,DanhMucId")] Sach sach)
        {
            if (id != sach.SachId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sach);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SachExists(sach.SachId))
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
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "DanhMucId", "DanhMucId", sach.DanhMucId);
            return View(sach);
        }

        // GET: Saches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sach = await _context.Saches
                .Include(s => s.DanhMuc)
                .FirstOrDefaultAsync(m => m.SachId == id);
            if (sach == null)
            {
                return NotFound();
            }

            return View(sach);
        }

        // POST: Saches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sach = await _context.Saches.FindAsync(id);
            if (sach != null)
            {
                _context.Saches.Remove(sach);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SachExists(int id)
        {
            return _context.Saches.Any(e => e.SachId == id);
        }
    }
}
