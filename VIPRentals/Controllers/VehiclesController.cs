using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using VIPRentals.Data;
using VIPRentals.Models;

namespace VIPRentals.Controllers
{
    public class VehiclesController(VIPRentalsContext context) : Controller
    {
        private readonly VIPRentalsContext _context = context;

        // GET: Vehicles
        public async Task<IActionResult> Index(string sortOrder, string searchString, string showOwned)
        {
            ViewData["MakeSortParm"] = String.IsNullOrEmpty(sortOrder) ? "make_desc" : "";
            ViewData["ModelSortParm"] = sortOrder == "Model" ? "model_desc" : "Model";
            ViewData["YearSortParm"] = sortOrder == "Year" ? "year_desc" : "Year";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";
            ViewData["CurrentFilter"] = sortOrder;
            var vehicles = from s in _context.Vehicle
                           select s;
            if(!String.IsNullOrEmpty(searchString))
            {
                ViewData["CurrentSearch"] = searchString;
                vehicles = vehicles.Where(s => s.Make.Contains(searchString) || s.Model.Contains(searchString));
            }

            if(!String.IsNullOrEmpty(showOwned))
            {
                ViewData["CurrentShowOwned"] = showOwned;
                if(showOwned == "true")
                {
                    vehicles = vehicles.Where(s => s.Owner == User.Identity.Name);
                }
            }

            switch (sortOrder)
            {
                case "make_desc":
                    vehicles = vehicles.OrderByDescending(s => s.Make);
                    break;
                case "Model":
                    vehicles = vehicles.OrderBy(s => s.Model);
                    break;
                case "model_desc":
                    vehicles = vehicles.OrderByDescending(s => s.Model);
                    break;
                case "Year":
                    vehicles = vehicles.OrderBy(s => s.Year);
                    break;
                case "year_desc":
                    vehicles = vehicles.OrderByDescending(s => s.Year);
                    break;
                case "Price":
                    vehicles = vehicles.OrderBy(s => s.Price);
                    break;
                case "price_desc":
                    vehicles = vehicles.OrderByDescending(s => s.Price);
                    break;
                default:
                    vehicles = vehicles.OrderBy(s => s.Make);
                    break;
            }
            return View(await vehicles.AsNoTracking().ToListAsync());
        }

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicle
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            var reviews = _context.Review.Where(r => r.Vehicle == vehicle.Id.ToString()).ToList();

            // sort by date
            reviews.Sort((a, b) => DateTime.Compare(DateTime.Parse(b.Date), DateTime.Parse(a.Date)));

            ViewData["Reviews"] = reviews ?? [];

            return View(vehicle);
        }

        // GET: Vehicles/Create
        public IActionResult Create()
        {
            return View();
        }



        // POST: Vehicles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Model,Make,Year,Price")] Vehicle vehicle, [FromServices] IWebHostEnvironment env)
        {
            Debug.WriteLine("Model state count: " + ModelState.Count);
            if(ModelState.Count == 4)
            {
                string filePath = Utilities.UploadFile(HttpContext, env.WebRootPath);
                if(filePath == null)
                {
                    // Error
                    Console.WriteLine("Error uploading file");
                    Debug.WriteLine("Error uploading file");
                    return View(null);
                }
                vehicle.Image = filePath;
                vehicle.Owner = User.Identity.Name;
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            } else
            {
                Console.WriteLine("Model state count is not 5");
                Debug.WriteLine("Model state count is not 5");
            }
            return View(vehicle);
        }



        // GET: Vehicles/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicle.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            if (vehicle.Owner != User.Identity.Name)
            {
                ViewData["Error"] = "You do not have permission to edit this vehicle";
                return View("Error");
            }

            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, Price")] Vehicle vehModel)
        {
            Debug.WriteLine("ID: " + id + " And vehModel.ID="+vehModel.Id);
            if (id != vehModel.Id)
            {
                return NotFound();
            }
            Debug.WriteLine("Model state count: " + ModelState.Count);
            var vehicle = await _context.Vehicle.FindAsync(id);

            if (vehicle == null)
            {
                return NotFound();
            }

                try
                {
                    vehicle.Price = vehModel.Price;
                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(vehModel.Id))
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

        // GET: Vehicles/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicle
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            if (vehicle.Owner != User.Identity.Name)
            {
                ViewData["Error"] = "You do not have permission to delete this vehicle";
                return View("Error");
            }

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicle.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicle.Remove(vehicle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicle.Any(e => e.Id == id);
        }
    }
}
