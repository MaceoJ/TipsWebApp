using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TipsTricksWebApp.Data;
using TipsTricksWebApp.Models;
using PagedList;

namespace TipsTricksWebApp.Controllers
{
    public class TipsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TipsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tips
        public async Task<IActionResult> Index(string currentFilter, string searchString, int? pageNumber, string sortOrder)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "date_asc" : "";
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;
            var tips = from t in _context.Tip select t;

            switch (sortOrder)
            {
                case "date_asc":
                    tips = tips.OrderBy(t => t.CreatedTime);
                    break;
                default:
                    tips = tips.OrderByDescending(t => t.CreatedTime);
                    break;
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                tips = tips.Where(t => t.Title.Contains(searchString)
                                       || t.Game.Contains(searchString));
            }

            int pageSize = 5;
            return View(await PaginatedList<Tip>.CreateAsync(tips.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Tips
        [Authorize]
        public async Task<IActionResult> MyTips(string currentFilter, string searchString, int? pageNumber, string sortOrder)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "date_asc" : "";
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;
            var tips = from t in _context.Tip select t;
            tips = tips.Where(t => t.User == User.FindFirstValue(ClaimTypes.NameIdentifier));
           
            switch (sortOrder)
            {
                case "date_asc":
                    tips = tips.OrderBy(t => t.CreatedTime);
                    break;
                default:
                    tips = tips.OrderByDescending(t => t.CreatedTime);
                    break;
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                tips = tips.Where(t => (t.Title.Contains(searchString)
                                       || t.Game.Contains(searchString)) && t.User == User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            int pageSize = 5;
            return View(await PaginatedList<Tip>.CreateAsync(tips.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // POST: Tips/ShowSearchResult
        //Currently searches both the title and game for the search query
        public async Task<IActionResult> ShowSearchResult(string SearchPhrase)
        {
            if (SearchPhrase == null) 
            {
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Name = SearchPhrase;
            return View("ShowSearchResult", await _context.Tip.Where(t => t.Title.Contains(SearchPhrase) || t.Game.Contains(SearchPhrase)).ToListAsync());
        }

        // GET: Tips/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tip = await _context.Tip
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tip == null)
            {
                return NotFound();
            }

            return View(tip);
        }

        // GET: Tips/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tips/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Game,Description,User,Username,CreatedTime")] Tip tip)
        {
            if (ModelState.IsValid)
            {
                tip.User = User.FindFirstValue(ClaimTypes.NameIdentifier);
                //The usernamer that will actually be displayed
                tip.Username = User.FindFirstValue(ClaimTypes.Name);
                tip.CreatedTime = DateTime.Now;
                _context.Add(tip);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tip);
        }

        // GET: Tips/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tip = await _context.Tip.FindAsync(id);
            if (tip == null)
            {
                return NotFound();
            }
            if (tip.User != User.FindFirstValue(ClaimTypes.NameIdentifier)) 
            {
                return RedirectToAction(nameof(Index));
            }
            return View(tip);
        }

        // POST: Tips/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Game,Description,User,Username,CreatedTime")] Tip tip)
        {
            if (id != tip.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    tip.User = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    //The usernamer that will actually be displayed
                    tip.Username = User.FindFirstValue(ClaimTypes.Name);
                    tip.CreatedTime = DateTime.Now;
                    _context.Update(tip);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipExists(tip.Id))
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
            return View(tip);
        }

        // GET: Tips/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tip = await _context.Tip
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tip == null)
            {
                return NotFound();
            }
            if (tip.User != User.FindFirstValue(ClaimTypes.NameIdentifier)) 
            {
                return RedirectToAction(nameof(Index));
            }
            return View(tip);
        }

        // POST: Tips/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tip = await _context.Tip.FindAsync(id);
            _context.Tip.Remove(tip);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipExists(int id)
        {
            return _context.Tip.Any(e => e.Id == id);
        }
    }
}
