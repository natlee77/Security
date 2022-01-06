using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Security.Data;
using Security.Models;
using Security.Utilities;

namespace Security.Controllers
{
    public class AddFilesController : Controller
    {
        private readonly SecurityDbContext _context;
        private readonly long fileSizeLimit = 10 * 1048576;
        private readonly string[] permittedExtensions = { ".jpg",".png" };

        public AddFilesController(SecurityDbContext context)
        {
            _context = context;
        }

        // GET: AddFiles
        public async Task<IActionResult> Index()
        {
            return View(await _context.AddFile.ToListAsync());
        }

        [HttpPost]
        [Route(nameof(UploadFile))]
        public async Task<IActionResult> UploadFile()  //anropas i index(addfiles )
        {
            var theWebRequest = HttpContext.Request;

            // validation of Content-Type
            // 1. first, it must be a form-data request
            // 2. a boundary should be found in the Content-Type
            if (!theWebRequest.HasFormContentType ||
                !MediaTypeHeaderValue.TryParse(theWebRequest.ContentType, out var theMediaTypeHeader) ||
                string.IsNullOrEmpty(theMediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }

            var reader = new MultipartReader(theMediaTypeHeader.Boundary.Value, theWebRequest.Body);
            var section = await reader.ReadNextSectionAsync();

            // This sample try to get the first file from request and save it
            // Make changes according to your needs in actual use
            while (section != null)
            {
                var DoesItHaveContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out var theContentDisposition);

                if (DoesItHaveContentDispositionHeader && theContentDisposition.DispositionType.Equals("form-data") &&
                    !string.IsNullOrEmpty(theContentDisposition.FileName.Value))
                {
                    // Don't trust any file name, file extension, and file data from the request unless you trust them completely
                    // Otherwise, it is very likely to cause problems such as virus uploading, disk filling, etc
                    // In short, it is necessary to restrict and verify the upload
                    // Here, we just use the temporary folder and a random file name

                    AddFile addFile = new AddFile();
                    addFile.NotrustedName = HttpUtility.HtmlEncode(theContentDisposition.FileName.Value);//encoda säkra filnamn
                    addFile.TimeStamp = DateTime.UtcNow;

                    addFile.Content =
                            await FileHelpers.ProcessStreamedFile(section, theContentDisposition,
                                ModelState, permittedExtensions, fileSizeLimit); //  verifiera  ändelser -- permittedExtensions
                    if (addFile.Content.Length == 0)
                    {
                        return RedirectToAction("Index", "AddFiles");
                    }
                    addFile.Size = addFile.Content.Length;

                    await _context.AddFile.AddAsync(addFile);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "AddFiles");

                }

                section = await reader.ReadNextSectionAsync();
            }

            // If the code runs to this location, it means that no files have been saved
            return BadRequest("No files data in the request.");
        }

        // GET: ApplicationFiles/Download/5
        public async Task<IActionResult> Download(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var addFile = await _context.AddFile //sök i DB filen
                .FirstOrDefaultAsync(m => m.Id == id);
            if (addFile == null)
            {
                return NotFound();
                //  return RedirecToAction("Index", "AddFiles");
            }

            return File(addFile.Content, MediaTypeNames.Application.Octet, addFile.NotrustedName);
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
