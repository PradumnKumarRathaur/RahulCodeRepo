using Azure.Core;
using Core.Entities;
using Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace TicketBooking.Controllers
{
    [Authorize]
    public class MyBookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MyBookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> MyBooking()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var seatList = await _unitOfWork.MyBooking.GetBookedSeat(userId);
            return View(seatList);  

        }



        [HttpPost]
        public async Task<IActionResult> CancelSelectedSeats(List<int> SelectedSeatIds)
        {
            if (SelectedSeatIds == null || !SelectedSeatIds.Any())
            {
                TempData["Error"] = "No seats were selected for cancellation.";
                return RedirectToAction(nameof(MyBooking));
            }

            await _unitOfWork.MyBooking.CancelSeats(SelectedSeatIds);
            await _unitOfWork.CommitAsync();
            TempData["Success"] = "Selected seats have been successfully canceled.";

            return RedirectToAction(nameof(MyBooking));
        }
    }
}
