using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Security.Data;
using Security.Models;

namespace Security.Controllers
{
    public class CommentsController : Controller
    {
        private readonly SecurityDbContext DB;
        public List<string> allowedTags { get; set; }

        public CommentsController(SecurityDbContext context)
        {
            DB = context;
            allowedTags = new List<string>() { "<b>", "</b>", "<em>","</em>", "<strong>" , "</strong>", "<i>", "</i>" , "<del>", "</del>" };
        }

        // GET: Comments1
        public async Task<IActionResult> Index()
        {
            return View(await DB.Comment.ToListAsync());
        }

        // GET: Comments1/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await DB.Comment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Comments1/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Comments1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.Id = Guid.NewGuid();
                comment.TimeStamp = DateTime.Now;

                //security encoding -- tillåta saker
                string encodedContent = HttpUtility.HtmlEncode(comment.Content); 
                foreach (var tag in allowedTags)
                {
                    string encodedtag = HttpUtility.HtmlEncode(tag);
                    encodedContent = encodedContent.Replace(encodedtag, tag); //spara i variable ändring
                }
                comment.Content = encodedContent;


                DB.Add(comment);
                await DB.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(comment);
        }

        // GET: Comments1/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await DB.Comment.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            return View(comment);
        }

        // POST: Comments1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,TimeStamp,Content")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    DB.Update(comment);
                    await DB.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
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
            return View(comment);
        }

        // GET: Comments1/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await DB.Comment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var comment = await DB.Comment.FindAsync(id);
            DB.Comment.Remove(comment);
            await DB.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(Guid id)
        {
            return DB.Comment.Any(e => e.Id == id);
        }
    }
}
