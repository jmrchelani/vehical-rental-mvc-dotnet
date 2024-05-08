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
using Stripe.Checkout;
using Stripe;

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
        [Authorize]
        public async Task<IActionResult> Index()
        {

            var reviews = await _context.Review.ToListAsync();
            if (reviews == null)
            {
                Debug.WriteLine("Reviews is null from DB");
                reviews = new List<Models.Review>();
            }

            var rentals = await _context.Rental.ToListAsync();
            if (rentals == null)
            {
                Debug.WriteLine("Rentals is null from DB");
                rentals = new List<Rental>();
            }

            rentals = rentals.Where(x => x.User == _userManager.GetUserAsync(User).Result.Id).ToList();

            // Create a map of rental id to boolean value of whether the user has reviewed the rental or not
            Dictionary<int, bool> reviewedRentals = new Dictionary<int, bool>();

            foreach (var rental in rentals)
            {
                var review = reviews.Find(x =>(x.Rental == rental.Id.ToString()) && (x.User == rental.User));
                if (review != null)
                {
                    reviewedRentals[rental.Id] = true;
                }
                else
                {
                    reviewedRentals[rental.Id] = false;
                }
            }
            

            ViewData["Reviews"] = reviewedRentals;

            if (Request.Query.ContainsKey("checkout"))
            {
                ViewData["checkout"] = true;
            }

            return View(rentals);
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

                String veh = vehicle.Make + " " + vehicle.Model + " " + vehicle.Year;
                // Contoller: Rentals, Action: Index
                var successUrl = Request.Scheme + "://" + Request.Host + "/Rentals/PaymentDone?startDate=" + start.ToString() + "&endDate=" + end.ToString() + "&vehicleId=" + vehicle.Id + "&checkout=true";                                                                                                                      
                var cancelUrl = Request.Scheme + "://" + Request.Host + "/Rentals/CancelledCheckout";

                
                StripeConfiguration.ApiKey = "sk_test_4eC39HqLyjWDarjtT1zdp7dc";
                

                var prodoptions = new ProductCreateOptions { Name = "Vehicle Rent: "+veh };
                var prodservice = new ProductService();
                var product = prodservice.Create(prodoptions);
                var options = new PriceCreateOptions
                {
                    Product = product.Id,
                    UnitAmount = (long?)rental.TotalPrice * 100,
                    Currency = "usd",
                };
                var service = new PriceService();
                Price priceObj = service.Create(options);

                var sessionoptions = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    Price = priceObj.Id,
                    Quantity = 1,
                  },
                },
                    Mode = "payment",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                };

                var serviceObj = new SessionService();
                Session session = serviceObj.Create(sessionoptions);

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);

            }
            else
            {
                ModelState.AddModelError("Error", "Error renting vehicle");
                Console.WriteLine("Model state count is not 5");
                Debug.WriteLine("Model state count is not 5");
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> PaymentDone([FromQuery] String startDate, [FromQuery] String endDate, [FromQuery] String vehicleId)
        {
            Rental rental = new Rental
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalPrice = 0,
                Vehicle = vehicleId,
                User = _userManager.GetUserAsync(User).Result.Id
            };

            _context.Add(rental);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index), new { checkout = true });
        }

        
        [Authorize]
        private bool RentalExists(int id)
        {
            return _context.Rental.Any(e => e.Id == id);
        }

        [Authorize]
        public async Task<IActionResult> AlreadyRented()
        {
            return View();
        }

        [Authorize]                             
        public async Task<IActionResult> CancelledCheckout()
        {
            return View();
        }
    }
}
