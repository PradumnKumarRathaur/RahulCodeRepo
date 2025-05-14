using Core.Entities;
using Core.Interface;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Data.Common;

namespace TicketBooking.Controllers
{
    [AllowAnonymous]
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<SignalR> _seatHub;
        public BookingController(IUnitOfWork unitOfWork, IHubContext<SignalR> seathub)
        {
            _unitOfWork = unitOfWork;
            _seatHub = seathub;
        }

      
        [HttpPost]
        public async Task<IActionResult> ContinueToBooking(int timeSlotId, List<int> SelectedSeatIds, string tabSessionId)
        {
            if (SelectedSeatIds == null || !SelectedSeatIds.Any())
            {
                TempData["Message"] = "Please select atleast 1 seat";
                return RedirectToAction("Seats", "Seat", new { timeSlotId = timeSlotId });
            }

            var selectedSeats = await _unitOfWork.SlotSeats.GetSeatsByIdsAsync(SelectedSeatIds);

            

            var viewModel = new BookingViewModel
            {
                TimeSlotId = timeSlotId,
                SelectedSeatIds = selectedSeats.Select(s => s.SlotSeatId).ToList(),
                SelectedSeatNumbers = selectedSeats.Select(s => s.SeatNumber).ToList()
            };

            await _unitOfWork.Bookings.MarkSeatsBlockedAsync(SelectedSeatIds, tabSessionId);

            foreach (var seat in selectedSeats)
            {
                await _seatHub.Clients.All.SendAsync("ReceiveSeatUpdate", seat.SlotSeatId, seat.SeatNumber);
            }
            await _unitOfWork.CommitAsync();

            return View("BookingPage", viewModel);
        }











        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(BookingViewModel model, string tabSessionId)
        {
            if (!ModelState.IsValid)
            {
                return View("BookingPage", model);
            }


            var user = new User
            {
                Username = model.Name,
                Email = model.Email,
                MobileNumber = model.Mobile,
                Password = model.Password
            };

            var booking = new Booking
            {
                CustomerName = model.Name,
                Email = model.Email,
                MobileNumber = model.Mobile,
                Password = model.Password,
                TimeSlotId = model.TimeSlotId,
                BookingDate = DateTime.Now
            };

            var result = await _unitOfWork.Users.CreateUser(user);

            if(result == -1)
            {
                ViewData["Message"] = "Username or Email already exists. Please use different ones.";
                return View("BookingPage", model);
            }

            var selectedSlotSeatIds = model.SelectedSeatIds;

            try
            {

                var bookingId = await _unitOfWork.Bookings.CreateAsync(booking);

                foreach (var slotSeatId in selectedSlotSeatIds)
                {
                    await _unitOfWork.Bookings.AddBookingSeatAsync(new BookingSeat
                    {
                        BookingId = bookingId,
                        SlotSeatId = slotSeatId,
                        UserId = result
                    });
                }

                await _unitOfWork.CommitAsync();
                await _unitOfWork.Bookings.MarkSeatsAsBookedAsync(selectedSlotSeatIds, tabSessionId);

                TempData["SuccessMessage"] = "Tickets booked successfully!";
                return RedirectToAction("Index", "Event");
            }
            catch (InvalidOperationException ex)
            {

                ViewData["Message"] = "These seats are booked, please select different ones.";
                return View("BookingPage", model);
            }
        }

        public IActionResult BookingSuccess()
        {
            return View();
        }
    }
}
