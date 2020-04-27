﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BookStore_API.Contracts;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILoggerService _logger;
        private IConfiguration _config;

        public UsersController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager,
            ILoggerService loggerService, IConfiguration config) 
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = loggerService;
            _config = config;
        }

        /// <summary>
        /// User Login Endpoint
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Login([FromBody] UserDTO userDTO) 
        {
            var location = GetCotrollerActionNames();
            try
            {
                var username = userDTO.Username;
                var password = userDTO.Password;
                _logger.LogInfo($"{location}: Login attempt from user: {username}");
                var result = await _signInManager.PasswordSignInAsync(username, password, false, false);

                if (result.Succeeded)
                {
                    _logger.LogInfo($"{location}: {username} successfully authenticated");
                    var user = await _userManager.FindByNameAsync(username);
                    var tokenString = await GenerateJSONWebToken(user);
                    return Ok(new { token = tokenString });
                }
                _logger.LogInfo($"{location}: {username} not authenticated");

                return Unauthorized(userDTO);
            }
            catch (Exception e) 
            {
                return InternalServerError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        private async Task<string> GenerateJSONWebToken(IdentityUser user) 
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(x => new Claim(ClaimsIdentity.DefaultRoleClaimType, x)));
            var token = new JwtSecurityToken(_config["Jwt:Issuer"], 
                _config["Jwt:Issuer"],
                claims, 
                null,
                expires: DateTime.Now.AddMinutes(5), 
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GetCotrollerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} - {action}";
        }

        private ObjectResult InternalServerError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the System Administrator.");
        }
    }
}