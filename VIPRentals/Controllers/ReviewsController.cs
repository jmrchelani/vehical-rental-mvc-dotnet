using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VIPRentals.Data;
using VIPRentals.Models;

namespace VIPRentals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly VIPRentalsContext _context;

        public ReviewsController(VIPRentalsContext context)
        {
            _context = context;
        }

        // GET: api/Reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReview()
        {
            return await _context.Review.ToListAsync();
        }

        // GET: api/Reviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReview(int id)
        {
            var review = await _context.Review.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            return review;
        }


        private bool ReviewExists(int id)
        {
            return _context.Review.Any(e => e.Id == id);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LeaveReview()
        {
            var data = HttpContext.Request.Form;

            if (data == null)
            {
                ModelState.AddModelError("Error", "No data found");
                return RedirectToAction(nameof(Index));
            }

            string id = data.SingleOrDefault(x => x.Key == "rentalId").Value.Count > 0 ? data.SingleOrDefault(x => x.Key == "rentalId").Value[0] : null;

            if (id == null)
            {
                return NotFound();
            }

            int _rentalId = int.Parse(id);

            var rental = await _context.Rental.FindAsync(_rentalId);
            if (rental == null)
            {
                return NotFound();
            }



            string comment = data.SingleOrDefault(x => x.Key == "comment").Value.Count > 0 ? data.SingleOrDefault(x => x.Key == "comment").Value[0] : "No comment";
            int rating = data.SingleOrDefault(x => x.Key == "rating").Value.Count > 0 ? int.Parse(data.SingleOrDefault(x => x.Key == "rating").Value[0]) : 0;
            int total = data.SingleOrDefault(x => x.Key == "total").Value.Count > 0 ? int.Parse(data.SingleOrDefault(x => x.Key == "total").Value[0]) : 0;
            string userId = data.SingleOrDefault(x => x.Key == "userId").Value.Count > 0 ? data.SingleOrDefault(x => x.Key == "userId").Value[0] : "No user";
            string vehicleId = data.SingleOrDefault(x => x.Key == "vehicleId").Value.Count > 0 ? data.SingleOrDefault(x => x.Key == "vehicleId").Value[0] : null;
            string rentalId = data.SingleOrDefault(x => x.Key == "rentalId").Value.Count > 0 ? data.SingleOrDefault(x => x.Key == "rentalId").Value[0] : "No rental";

            if (vehicleId == null)
            {
                return NotFound();
            }

            Review review = new Review
            {
                Comment = comment,
                Rating = rating,
                Rental = rentalId,
                User = userId,
                Vehicle = vehicleId,
                Date = DateTime.Now.ToString()
            };

            var added = _context.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Vehicles", new { id = vehicleId });
        }
    }
}
