﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QLKhachSan.Models;
using QLKhachSan.Models.DTO;
using QLKhachSan.Repository.IRepository;
using QLKhachSan.Utility;
using System.Net;

namespace QLKhachSan.Controllers
{
    [Route("api/BookingService")]
    [ApiController]
    public class BookingServiceController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        protected APIResponse _response;
        private readonly UserManager<Person> _userManager;
        public BookingServiceController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<Person> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _response = new APIResponse();
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetBookServices()
        {
            IEnumerable<BookingService> bookingServices = await _unitOfWork.BookingService.GetAllAsync(includeProperties: "BookingServiceDetails");
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<IEnumerable<BookingServiceDTO>>(bookingServices);
            return Ok(_response);
        }
        [HttpGet("{id:int}", Name = "GetBookingService")]
        public async Task<ActionResult<APIResponse>> GetBookService(int id)
        {
            if(id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Invalid BookingService ID");
                return BadRequest(_response);
            }
            BookingService bookingService = await _unitOfWork.BookingService.GetAsync(u => u.Id == id, includeProperties: "BookingServiceDetails");
            if(bookingService == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Not Found");
                return NotFound(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<BookingServiceDTO>(bookingService);
            return Ok(_response);
        }
        [HttpPost]

        public async Task<ActionResult<APIResponse>> CreateBookingService(int serviceId, int quanlity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Invalid User ID");
                return BadRequest(_response);
            }
            Service service = await _unitOfWork.Service.GetAsync(u => u.Id == serviceId);
            if (service == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Service item is not valid");
                return BadRequest(_response);
            }
            Booking booking = await _unitOfWork.Booking.GetAsync(u => u.PersonId == user.Id, includeProperties: "Room");
            if (booking == null)
            {
                // chua co booking
                
                BookingService newBookingService = new BookingService()
                {
                    ServiceCount = 0,
                    ToTalPrice = 0,
                    BookingServiceDetails = new List<BookingServiceDetail>()
                };
                await _unitOfWork.BookingService.CreateAsync(newBookingService);
                Booking newBooking = new Booking()
                {
                    PersonId = user.Id,
                    BookingDate = DateTime.Now,
                    TotalPrice = 0,
                    UpdateBookingDate = DateTime.Now,
                    BookingServiceId = newBookingService.Id,
                };
                await _unitOfWork.Booking.CreateAsync(newBooking);
                BookingServiceDetail newBookingServiceDetail = new BookingServiceDetail()
                {
                    ServiceId = serviceId,
                    Quantity = quanlity,
                    BookingServiceId = newBookingService.Id,
                };
                await _unitOfWork.BookingServiceDetail.CreateAsync(newBookingServiceDetail);
                newBookingService.BookingServiceDetails = await _unitOfWork.BookingServiceDetail.GetAllAsync(u => u.BookingServiceId == newBookingService.Id, includeProperties: "Service");
                // tinh tong tien
                newBookingService.ToTalPrice = newBookingService.BookingServiceDetails.Sum(d => d.Quantity * d.Service.Price);
                newBookingService.ServiceCount = newBookingService.BookingServiceDetails.Count();
                await _unitOfWork.BookingService.UpdateAsync(newBookingService);
                newBooking.TotalPrice = newBookingService.ToTalPrice;
                await _unitOfWork.Booking.UpdateAsync(newBooking);
                _response.Result = _mapper.Map<BookingServiceDTO>(newBookingService);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            else
            {
                // da co booking
                BookingService bookingService = await _unitOfWork.BookingService.GetAsync(u => u.Id == booking.BookingServiceId);
                if (bookingService == null)
                {
                    //chua co BookingService
                    BookingService newBookingService = new BookingService()
                    {
                        ServiceCount = 0,
                        ToTalPrice = 0,
                        BookingServiceDetails = new List<BookingServiceDetail>()
                    };
                    await _unitOfWork.BookingService.CreateAsync(newBookingService);
                    BookingServiceDetail bookingServiceDetail = await _unitOfWork.BookingServiceDetail.GetAsync(u => u.ServiceId == serviceId && u.BookingServiceId == newBookingService.Id);
                    if (bookingServiceDetail == null)
                    {
                        //chua co booking service detail
                        BookingServiceDetail newBookingServiceDetail = new BookingServiceDetail()
                        {
                            ServiceId = serviceId,
                            Quantity = quanlity,
                            BookingServiceId = newBookingService.Id,
                        };
                        await _unitOfWork.BookingServiceDetail.CreateAsync(newBookingServiceDetail);
                        newBookingService.BookingServiceDetails = await _unitOfWork.BookingServiceDetail.GetAllAsync(u => u.BookingServiceId == newBookingService.Id, includeProperties: "Service");
                        // tinh tong tien
                        newBookingService.ToTalPrice = newBookingService.BookingServiceDetails.Sum(d => d.Quantity * d.Service.Price);
                        newBookingService.ServiceCount = newBookingService.BookingServiceDetails.Count();
                    }
                    else
                    {
                        // da co booking service detail
                        bookingServiceDetail.Quantity += quanlity;
                        await _unitOfWork.BookingServiceDetail.UpdateAsync(bookingServiceDetail);
                        newBookingService.BookingServiceDetails = await _unitOfWork.BookingServiceDetail.GetAllAsync(u => u.BookingServiceId == newBookingService.Id, includeProperties: "Service");
                        // tinh tong tien
                        newBookingService.ToTalPrice = newBookingService.BookingServiceDetails.Sum(d => d.Quantity * d.Service.Price);
                        newBookingService.ServiceCount = newBookingService.BookingServiceDetails.Count();
                    }
                    
                    await _unitOfWork.BookingService.UpdateAsync(newBookingService);
                    booking.BookingServiceId = newBookingService.Id;
                    booking.TotalPrice += newBookingService.ToTalPrice;
                    booking.UpdateBookingDate = DateTime.Now;
                    await _unitOfWork.Booking.UpdateAsync(booking);
                    _response.Result = _mapper.Map<BookingServiceDTO>(newBookingService);
                    _response.StatusCode = HttpStatusCode.OK;
                    return Ok(_response);
                }
                else
                {
                    //co BookingService
                    BookingServiceDetail bookingServiceDetail = await _unitOfWork.BookingServiceDetail.GetAsync(u => u.ServiceId == serviceId && u.BookingServiceId == bookingService.Id);
                    if (bookingServiceDetail == null)
                    {
                        //chua co booking service detail
                        BookingServiceDetail newBookingServiceDetail = new BookingServiceDetail()
                        {
                            ServiceId = serviceId,
                            Quantity = quanlity,
                            BookingServiceId = bookingService.Id,
                        };
                        await _unitOfWork.BookingServiceDetail.CreateAsync(newBookingServiceDetail);
                        bookingService.BookingServiceDetails = await _unitOfWork.BookingServiceDetail.GetAllAsync(u => u.BookingServiceId == bookingService.Id, includeProperties: "Service");
                        // tinh tong tien
                        bookingService.ToTalPrice = bookingService.BookingServiceDetails.Sum(d => d.Quantity * d.Service.Price);
                        bookingService.ServiceCount = bookingService.BookingServiceDetails.Count();
                    }
                    else
                    {
                        // da co booking service detail
                        bookingServiceDetail.Quantity += quanlity;
                        await _unitOfWork.BookingServiceDetail.UpdateAsync(bookingServiceDetail);
                        bookingService.BookingServiceDetails = await _unitOfWork.BookingServiceDetail.GetAllAsync(u => u.BookingServiceId == bookingService.Id, includeProperties: "Service");
                        // tinh tong tien
                        bookingService.ToTalPrice = bookingService.BookingServiceDetails.Sum(d => d.Quantity * d.Service.Price);
                        bookingService.ServiceCount = bookingService.BookingServiceDetails.Count();
                    }
                    await _unitOfWork.BookingService.UpdateAsync(bookingService);
                    var days = (booking.CheckOutDate - booking.CheckInDate);
                    booking.TotalPrice = 0;
                    if (days.HasValue)
                    {
                        int day = days.Value.Days;
                        if (day >= 1 && day < 7)
                        {
                            booking.TotalPrice = booking.Room.PriceDay * day;
                        }
                        else
                        {
                            booking.TotalPrice = ((day / 7) * booking.Room.PriceWeek) + ((day % 7) * booking.Room.PriceDay);
                        }
                    }
                    booking.TotalPrice += bookingService.ToTalPrice;
                    booking.UpdateBookingDate = DateTime.Now;
                    await _unitOfWork.Booking.UpdateAsync(booking);
                    _response.Result = _mapper.Map<BookingServiceDTO>(bookingService);
                    _response.StatusCode = HttpStatusCode.OK;
                    return Ok(_response);
                }

                
            }
        }
        [HttpPut]

        public async Task<ActionResult<APIResponse>> UpdateBookingService(string userId, int serviceId, int quanlity)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Invalid User ID");
                return BadRequest(_response);
            }
            Service service = await _unitOfWork.Service.GetAsync(u => u.Id == serviceId);
            if (service == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Service item is not valid");
                return BadRequest(_response);
            }
            Booking booking = await _unitOfWork.Booking.GetAsync(u => u.PersonId == user.Id, includeProperties: "Room");
            if (booking == null)
            {
                // chua co booking
                
                BookingService newBookingService = new BookingService()
                {
                    ServiceCount = 0,
                    ToTalPrice = 0,
                    BookingServiceDetails = new List<BookingServiceDetail>()
                };
                await _unitOfWork.BookingService.CreateAsync(newBookingService);
                Booking newBooking = new Booking()
                {
                    PersonId = user.Id,
                    BookingDate = DateTime.Now,
                    TotalPrice = 0,
                    UpdateBookingDate = DateTime.Now,
                    BookingServiceId = newBookingService.Id,
                };
                await _unitOfWork.Booking.CreateAsync(newBooking);
                BookingServiceDetail newBookingServiceDetail = new BookingServiceDetail()
                {
                    ServiceId = serviceId,
                    Quantity = quanlity,
                    BookingServiceId = newBookingService.Id,
                };
                await _unitOfWork.BookingServiceDetail.CreateAsync(newBookingServiceDetail);
                newBookingService.BookingServiceDetails = await _unitOfWork.BookingServiceDetail.GetAllAsync(u => u.BookingServiceId == newBookingService.Id, includeProperties: "Service");
                // tinh tong tien
                newBookingService.ToTalPrice = newBookingService.BookingServiceDetails.Sum(d => d.Quantity * d.Service.Price);
                newBookingService.ServiceCount = newBookingService.BookingServiceDetails.Count();
                await _unitOfWork.BookingService.UpdateAsync(newBookingService);

                newBooking.TotalPrice = newBookingService.ToTalPrice;
                await _unitOfWork.Booking.UpdateAsync(newBooking);
                _response.Result = _mapper.Map<BookingServiceDTO>(newBookingService);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            else
            {
                // da co booking
                BookingService bookingService = await _unitOfWork.BookingService.GetAsync(u => u.Id == booking.BookingServiceId);
                if (bookingService == null)
                {
                    //chua co BookingService
                    BookingService newBookingService = new BookingService()
                    {
                        ServiceCount = 0,
                        ToTalPrice = 0,
                        BookingServiceDetails = new List<BookingServiceDetail>()
                    };
                    await _unitOfWork.BookingService.CreateAsync(newBookingService);
                    BookingServiceDetail bookingServiceDetail = await _unitOfWork.BookingServiceDetail.GetAsync(u => u.ServiceId == serviceId && u.BookingServiceId == newBookingService.Id);
                    //if (bookingServiceDetail == null)
                    //{
                        //chua co booking service detail
                        BookingServiceDetail newBookingServiceDetail = new BookingServiceDetail()
                        {
                            ServiceId = serviceId,
                            Quantity = quanlity,
                            BookingServiceId = newBookingService.Id,
                        };
                        await _unitOfWork.BookingServiceDetail.CreateAsync(newBookingServiceDetail);
                        newBookingService.BookingServiceDetails = await _unitOfWork.BookingServiceDetail.GetAllAsync(u => u.BookingServiceId == newBookingService.Id, includeProperties: "Service");
                        // tinh tong tien
                        newBookingService.ToTalPrice = newBookingService.BookingServiceDetails.Sum(d => d.Quantity * d.Service.Price);
                        newBookingService.ServiceCount = newBookingService.BookingServiceDetails.Count();
                    //}
                    //else
                    //{
                    //    // da co booking service detail
                    //    bookingServiceDetail.Quantity += quanlity;
                    //    await _unitOfWork.BookingServiceDetail.UpdateAsync(bookingServiceDetail);
                    //    newBookingService.BookingServiceDetails = await _unitOfWork.BookingServiceDetail.GetAllAsync(u => u.BookingServiceId == newBookingService.Id, includeProperties: "Service");
                    //    // tinh tong tien
                    //    newBookingService.ToTalPrice = newBookingService.BookingServiceDetails.Sum(d => d.Quantity * d.Service.Price);
                    //    newBookingService.ServiceCount = newBookingService.BookingServiceDetails.Count();
                    //}
                    await _unitOfWork.BookingService.UpdateAsync(newBookingService);
                    booking.BookingServiceId = newBookingService.Id;
                    booking.TotalPrice += newBookingService.ToTalPrice;
                    booking.UpdateBookingDate = DateTime.Now;
                    await _unitOfWork.Booking.UpdateAsync(booking);
                    _response.Result = _mapper.Map<BookingServiceDTO>(newBookingService);
                    _response.StatusCode = HttpStatusCode.OK;
                    return Ok(_response);
                }
                else
                {
                    //co BookingService
                    BookingServiceDetail bookingServiceDetail = await _unitOfWork.BookingServiceDetail.GetAsync(u => u.ServiceId == serviceId && u.BookingServiceId == bookingService.Id);
                    if (bookingServiceDetail == null)
                    {
                        //chua co booking service detail
                        BookingServiceDetail newBookingServiceDetail = new BookingServiceDetail()
                        {
                            ServiceId = serviceId,
                            Quantity = quanlity,
                            BookingServiceId = bookingService.Id,
                        };
                        await _unitOfWork.BookingServiceDetail.CreateAsync(newBookingServiceDetail);
                        bookingService.BookingServiceDetails = await _unitOfWork.BookingServiceDetail.GetAllAsync(u => u.BookingServiceId == bookingService.Id, includeProperties: "Service");
                        // tinh tong tien
                        bookingService.ToTalPrice = bookingService.BookingServiceDetails.Sum(d => d.Quantity * d.Service.Price);
                        bookingService.ServiceCount = bookingService.BookingServiceDetails.Count();
                    }
                    else
                    {
                        // da co booking service detail
                        bookingServiceDetail.Quantity = quanlity;
                        await _unitOfWork.BookingServiceDetail.UpdateAsync(bookingServiceDetail);
                        bookingService.BookingServiceDetails = await _unitOfWork.BookingServiceDetail.GetAllAsync(u => u.BookingServiceId == bookingService.Id, includeProperties: "Service");
                        // tinh tong tien
                        bookingService.ToTalPrice = bookingService.BookingServiceDetails.Sum(d => d.Quantity * d.Service.Price);
                        bookingService.ServiceCount = bookingService.BookingServiceDetails.Count();
                    }
                    await _unitOfWork.BookingService.UpdateAsync(bookingService);
                    var days = (booking.CheckOutDate - booking.CheckInDate);
                    booking.TotalPrice = 0;
                    if (days.HasValue)
                    {
                        int day = days.Value.Days;
                        if (day >= 1 && day < 7)
                        {
                            booking.TotalPrice = booking.Room.PriceDay * day;
                        }
                        else
                        {
                            booking.TotalPrice = ((day / 7) * booking.Room.PriceWeek) + ((day % 7) * booking.Room.PriceDay);
                        }
                    }
                    booking.TotalPrice += bookingService.ToTalPrice;
                    booking.UpdateBookingDate = DateTime.Now;
                    await _unitOfWork.Booking.UpdateAsync(booking);
                    _response.Result = _mapper.Map<BookingServiceDTO>(bookingService);
                    _response.StatusCode = HttpStatusCode.OK;
                    return Ok(_response);
                }
            }
        }
        [HttpDelete]
        public async Task<ActionResult<APIResponse>> DeleteBookingService(string userId, int bookingServiceDetailId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Invalid User ID");
                return BadRequest(_response);
            }
            Booking booking = await _unitOfWork.Booking.GetAsync(u => u.PersonId == userId);
            BookingService bookingService = await _unitOfWork.BookingService.GetAsync(u => u.Id == booking.Id);
            if (bookingService == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("BookingService does not exist");
                return BadRequest(_response);
            }
            BookingServiceDetail item = await _unitOfWork.BookingServiceDetail.GetAsync(u => u.BookingServiceId == bookingService.Id && u.Id == bookingServiceDetailId);
            if (item == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("item does not exist");
                return BadRequest(_response);
            }
            else
            {
                await _unitOfWork.BookingServiceDetail.RemoveAsync(item);
                //update lai ShoppingCart
                bookingService.BookingServiceDetails = await _unitOfWork.BookingServiceDetail.GetAllAsync(u => u.BookingServiceId == bookingService.Id, includeProperties: "Service");
                bookingService.ServiceCount = bookingService.BookingServiceDetails.Count();
                bookingService.ToTalPrice = bookingService.BookingServiceDetails.Sum(d => d.Quantity * d.Service.Price);
                await _unitOfWork.BookingService.UpdateAsync(bookingService);

                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<BookingServiceDTO>(bookingService);
                return Ok(_response);
            }
        }
    }
}
