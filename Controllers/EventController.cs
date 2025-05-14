using Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketBooking.Controllers
{

    [AllowAnonymous]
    public class EventController : Controller
    {
            private readonly IUnitOfWork _unitOfWork;

            public EventController(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }

        public async Task<IActionResult> Index()
        {
            var data = await _unitOfWork.Events.GetEventsWithTimeSlotsAsync();
            return View(data);
        }
    }

}
