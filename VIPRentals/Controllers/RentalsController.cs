using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VIPRentals.Data;
using VIPRentals.Models;
using System.Net.Http;
using Microsoft.IdentityModel.Tokens;

namespace VIPRentals.Controllers
{
    public class RentalsController : Controller
    {
        private readonly VIPRentalsContext _context;
        private readonly UserManager<UserModel> _userManager;

        public RentalsController(VIPRentalsContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Rentals
        public async Task<IActionResult> Index()
        {
            return View(await _context.Rental.ToListAsync());
        }

        // GET: Rentals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rental
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // GET: Rentals/Create
        public IActionResult Create()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RentNow([FromServices] IWebHostEnvironment env)
        {
            Debug.WriteLine("Model state count: " + ModelState.Count);
            if (ModelState.IsValid)
            {

                var data = HttpContext.Request.Form;

                DateTime start = data.SingleOrDefault(x => x.Key == "startDateTime").Value.Count > 0 ? DateTime.Parse(data.SingleOrDefault(x => x.Key == "startDateTime").Value[0]) : DateTime.Now;

                DateTime end = data.SingleOrDefault(x => x.Key == "endDateTime").Value.Count > 0 ? DateTime.Parse(data.SingleOrDefault(x => x.Key == "endDateTime").Value[0]) : DateTime.Now.AddHours(1);

                Vehicle? vehicle = _context.Vehicle.Find(int.Parse(data.SingleOrDefault(x => x.Key == "VehicleId").Value[0]));

                if (vehicle == null)
                {
                    ModelState.AddModelError("Error", "Vehicle not found");
                    return RedirectToAction(nameof(Index));
                }

                double price = (end - start).TotalHours * (double)vehicle.Price;

                UserModel? _user = _userManager.GetUserAsync(User).Result;

                if (_user == null)
                {
                    ModelState.AddModelError("Error", "User not found");
                    return RedirectToAction(nameof(Index));
                }

                // If the vehicle is already rented in between the date time range [format:5/15/2024 12:00:00 AM	], return to AlreadyRented
                
                var rentals = _context.Rental.Where(x => x.Vehicle == vehicle.Id.ToString()).ToList();
                foreach (var _rental in rentals)
                {
                    DateTime rentalStart = DateTime.Parse(_rental.StartDate);
                    DateTime rentalEnd = DateTime.Parse(_rental.EndDate);

                    Debug.WriteLine("Rental Start: " + rentalStart);
                    Debug.WriteLine("Rental End: " + rentalEnd);

                    // If the start date is in between the rental start and end date 
                    // or the end date is in between the rental start and end date
                    // or the start date is before the rental start date and the end date is after the rental end date
                    // But if the start date is the same as the rental end date, it is not considered as in between
                    
                    if ((start >= rentalStart && start <= rentalEnd) || (end >= rentalStart && end <= rentalEnd) || (start <= rentalStart && end >= rentalEnd))
                    {
                        if (start == rentalEnd)
                        {
                            continue;
                        }
                        return RedirectToAction(nameof(AlreadyRented));
                    }

                }

                Rental rental = new Rental
                {
                    StartDate = start.ToString(),
                    EndDate = end.ToString(),
                    TotalPrice = (decimal)price,
                    Vehicle = vehicle.Id.ToString(),
                    User = _user.Id
                };

                _context.Add(rental);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("Error", "Error renting vehicle");
                Console.WriteLine("Model state count is not 5");
                Debug.WriteLine("Model state count is not 5");
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Rentals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartDate,EndDate,TotalPrice")] Rental rental)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rental);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rental);
        }

        // GET: Rentals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rental.FindAsync(id);
            if (rental == null)
            {
                return NotFound();
            }
            return View(rental);
        }

        // POST: Rentals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StartDate,EndDate,TotalPrice")] Rental rental)
        {
            if (id != rental.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rental);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RentalExists(rental.Id))
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
            return View(rental);
        }

        // GET: Rentals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rental
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // POST: Rentals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rental = await _context.Rental.FindAsync(id);
            if (rental != null)
            {
                _context.Rental.Remove(rental);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RentalExists(int id)
        {
            return _context.Rental.Any(e => e.Id == id);
        }

        public async Task<IActionResult> AlreadyRented()
        {
            return View();
        }
    }
}
