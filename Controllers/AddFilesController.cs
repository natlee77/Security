using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Security.Data;
using Security.Models;

namespace Security.Controllers
{
    public class AddFilesController : Controller
    {
        private readonly SecurityDbContext _context;
        private readonly long fileSizeLimit = 10 * 1048576;
        private readonly string[] permittedExtensions = { ".jpg" };

        public AddFilesController(SecurityDbContext context)
        {
            _context = context;
        }

        // GET: AddFiles
        public async Task<IActionResult> Index()
        {
            return View(await _context.AddFile.ToListAsync());
        }

        // GET: AddFiles/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var addFile = await _context.AddFile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (addFile == null)
            {
                return NotFound();
            }

            return View(addFile);
        }

        // GET: AddFiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AddFiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NotrustedName,Size,Content")] AddFile addFile)
        {
            if (ModelState.IsValid)
            {
                addFile.Id = Guid.NewGuid();
                addFile.TimeStamp = DateTime.Now;
                _context.Add(addFile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(addFile);
        }

        // GET: AddFiles/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var addFile = await _context.AddFile.FindAsync(id);
            if (addFile == null)
            {
                return NotFound();
            }
            return View(addFile);
        }

        // POST: AddFiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,NotrustedName,TimeStamp,Size,Content")] AddFile addFile)
        {
            if (id != addFile.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(addFile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AddFileExists(addFile.Id))
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
            return View(addFile);
        }

        // GET: AddFiles/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var addFile = await _context.AddFile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (addFile == null)
            {
                return NotFound();
            }

            return View(addFile);
        }

        // POST: AddFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var addFile = await _context.AddFile.FindAsync(id);
            _context.AddFile.Remove(addFile);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AddFileExists(Guid id)
        {
            return _context.AddFile.Any(e => e.Id == id);
        }
    }
}
