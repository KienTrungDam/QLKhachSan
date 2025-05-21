using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QLKhachSan.Models.DTO;
using QLKhachSan.Models;
using QLKhachSan.Utility;
using System.Net;
using AutoMapper;
using QLKhachSan.Repository.IRepository;

namespace QLKhachSan.Controllers
{
    [Route("api/Booking")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        protected APIResponse _response;
        private readonly UserManager<Person> _userManager;
        public BookingController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<Person> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _response = new APIResponse();
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetBookings()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                IEnumerable<Booking> bookings = await _unitOfWork.Booking.GetAllAsync();
                bookings = bookings.OrderByDescending(item => item.Id);

                bool isAdmin = await _userManager.IsInRoleAsync(user, SD.Role_Admin);
                bool isEmployee = await _userManager.IsInRoleAsync(user, SD.Role_Employee);

                if (isAdmin || isEmployee)
                {
                    _response.Result = _mapper.Map<IEnumerable<BookingDTO>>(bookings);
                }
                else
                {
                    _response.Result = _mapper.Map<IEnumerable<BookingDTO>>(bookings.Where(u => u.PersonId == user.Id));
                }
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add(ex.Message);
                return BadRequest(_response);
            }
        }
        [HttpGet("{id:int}", Name = "GetBooking")]
        public async Task<ActionResult<APIResponse>> GetBooking(int id)
        {
            try
            {
                if (id == 0)
                {
                    throw new Exception("Id is not valid");
                }
                Booking booking = await _unitOfWork.Booking.GetAsync(u => u.Id == id);
                if (booking == null)
                {
                    throw new Exception("Not found");
                }
                var user = await _userManager.GetUserAsync(User);
                bool isAdmin = await _userManager.IsInRoleAsync(user, SD.Role_Admin);
                bool isStaff = await _userManager.IsInRoleAsync(user, SD.Role_Employee);

                if (isAdmin || isStaff || booking.PersonId == user.Id)
                {
                    _response.Result = _mapper.Map<BookingDTO>(booking);
                }
                else
                {
                    throw new Exception("User is unauthorized");
                }
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add(ex.Message);
                return BadRequest(_response);
            }
        }
        [HttpPost]
        public async Task<ActionResult<APIResponse>> CreateBooking([FromForm] BookingCreateDTO bookingDTO)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                Booking booking = await _unitOfWork.Booking.GetAsync(u => u.PersonId == user.Id && u.BookingStatus == SD.Status_Booking_Pending, includeProperties: "BookingService");
                if(booking != null)
                {
                    // da tao booking tu them service truoc
                    var room = await _unitOfWork.Room.GetAsync(u => u.Id == bookingDTO.RoomId);
                    if (room == null)
                    {
                        _response.StatusCode = HttpStatusCode.NotFound;
                        _response.IsSuccess = false;
                        _response.ErrorMessages.Add("Not Found Room");
                        return NotFound(_response);
                    }
                    var days = (bookingDTO.CheckOutDate - bookingDTO.CheckInDate);
                    if (days.HasValue)
                    {
                        int day = days.Value.Days;
                        if (day >= 1 && day < 7)
                        {
                            booking.TotalPrice = (room.PriceDay * day) + booking.BookingService.ToTalPrice;
                        }
                        else
                        {
                            booking.TotalPrice = (((day / 7) * room.PriceWeek) + ((day % 7) * room.PriceDay)) + booking.BookingService.ToTalPrice;
                        }
                    }
                    booking.CheckInDate = bookingDTO.CheckInDate;
                    booking.CheckOutDate = bookingDTO.CheckOutDate;
                    booking.BookingDate = DateTime.Now;
                    booking.UpdateBookingDate = DateTime.Now;
                    booking.NumberOfGuests = bookingDTO.NumberOfGuests;
                    booking.RoomId = bookingDTO.RoomId;
                    await _unitOfWork.Booking.UpdateAsync(booking);
                    _response.Result = _mapper.Map<BookingDTO>(booking);
                    _response.StatusCode = HttpStatusCode.Created;
                    return CreatedAtRoute("GetBooking", new { id = booking.Id }, _response);
                }
                else
                {
                    Booking newBooking = _mapper.Map<Booking>(bookingDTO);
                    var room = await _unitOfWork.Room.GetAsync(u => u.Id == bookingDTO.RoomId);
                    if (room == null)
                    {
                        _response.StatusCode = HttpStatusCode.NotFound;
                        _response.IsSuccess = false;
                        _response.ErrorMessages.Add("Not Found Room");
                        return NotFound(_response);
                    }
                    newBooking.PersonId = user.Id;
                    var days = (bookingDTO.CheckOutDate - bookingDTO.CheckInDate);
                    if (days.HasValue)
                    {
                        int day = days.Value.Days;
                        if (day >= 1 && day < 7)
                        {
                            newBooking.TotalPrice = room.PriceDay * day;
                        }
                        else
                        {
                            newBooking.TotalPrice = ((day / 7) * room.PriceWeek) + ((day % 7) * room.PriceDay);
                        }
                    }
                    newBooking.UpdateBookingDate = DateTime.Now;
                    await _unitOfWork.Booking.CreateAsync(newBooking);
                    _response.Result = _mapper.Map<BookingDTO>(newBooking);
                    _response.StatusCode = HttpStatusCode.Created;
                    return CreatedAtRoute("GetBooking", new { id = newBooking.Id }, _response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
        }
    }
}
