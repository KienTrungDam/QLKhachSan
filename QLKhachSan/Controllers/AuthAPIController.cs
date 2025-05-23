﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QLKhachSan.Data;
using QLKhachSan.Models;
using QLKhachSan.Models.DTO;
using QLKhachSan.Repository.IRepository;
using QLKhachSan.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace QLKhachSan.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly APIResponse _response;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Person> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthAPIController(IMapper mapper, IUnitOfWork unitOfWork, UserManager<Person> userManager, RoleManager<IdentityRole> roleManager)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _response = new();
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            LoginResponseDTO loginResponseDTO = await _unitOfWork.User.LoginAsync(loginRequestDTO);
            
            var user = _mapper.Map<Person>(loginResponseDTO.user);
            var role = await _userManager.GetRolesAsync(user);

            loginResponseDTO.user.Role = role.FirstOrDefault();
            try
            {
                if (loginResponseDTO == null)
                {
                    throw new Exception("UserName or Password is incorrect");
                }
                _response.Result = loginResponseDTO;
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
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Register([FromBody] RegisterRequestDTO registerRequestDTO)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(registerRequestDTO.UserName);
                //bool valid = _unitOfWork.User.UniqueUserName(registerRequestDTO.UserName);   
                if (user == null)
                {
                    registerRequestDTO.Role = SD.Role_Customer;
                    PersonDTO userDTO = await _unitOfWork.User.RegisterAsync(registerRequestDTO);
                    if (userDTO == null)
                    {
                        throw new Exception("Error while registering");
                    }
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Result = userDTO;
                    return Ok(_response);
                }
                else
                {
                    throw new Exception("UserName is already exist");
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add(ex.Message);
                return BadRequest(_response);
            }
        }
    }
}
