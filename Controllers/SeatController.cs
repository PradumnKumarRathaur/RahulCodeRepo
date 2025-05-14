using Core.Entities;
using Core.Interface;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace TicketBooking.Controllers
{
    [AllowAnonymous]
    public class SeatController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<SignalR> _seatHub;
        public SeatController(IUnitOfWork unitOfWork, IHubContext<SignalR> seathub)
        {
            _unitOfWork = unitOfWork;
            _seatHub = seathub;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Seats(string tabSessionId, int timeSlotId = 1 )
        {
            var seats = await _unitOfWork.Seats.GetSeatsByTimeSlotIdAsync(timeSlotId);
            await _unitOfWork.Seats.ClearBlockedSeats(tabSessionId);
          


            var viewModel = new SeatSelectionViewModel
            {
                TimeSlotId = timeSlotId,
                Seats = seats.OrderBy(s => s.SeatId).ToList()
            };

            List<int> number = new List<int>() { 1, 2, 3 };


            foreach (var seat in number)
            {
                await _seatHub.Clients.All.SendAsync("ReceiveSeatUpdate", seat, seat);
            }



            await _unitOfWork.CommitAsync();


            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ContinueToBooking(int timeSlotId, List<int> SelectedSeatIds)
        {
            var seats = await _unitOfWork.SlotSeats.GetSeatsByIdsAsync(SelectedSeatIds);




            var viewModel = new BookingViewModel
            {
                TimeSlotId = timeSlotId,
                SelectedSeatIds = seats.Select(s => s.SlotSeatId).ToList(),
                SelectedSeatNumbers = seats.Select(s => s.SeatNumber).ToList()
            };

            return View("BookingPage", viewModel);
        }



      
           
           
          
           
        

    }
}
