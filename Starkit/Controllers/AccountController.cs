﻿using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Starkit.Models;
using Starkit.Models.Data;
using Starkit.Services;
using Starkit.ViewModels;

namespace Starkit.Controllers
{
    public class AccountController : Controller
    {
        public UserManager<User> _userManager { get; set; }
        public RoleManager<IdentityRole> _roleManager { get; set; }
        public SignInManager<User> _signInManager { get; set; }
        public StarkitContext _db { get; set; }
        public IHostEnvironment _environment { get; set; }

        public AccountController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager, StarkitContext db, IHostEnvironment environment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _db = db;
            _environment = environment;
        }

        // GET

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login model)
        {
            if (ModelState.IsValid)
            {
                User user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                        return RedirectToAction("Index", "Starkit");
                    ModelState.AddModelError("","Неверный пароль пользователя");
                }
                else
                    ModelState.AddModelError("","E-mail не зарегистрирован.");
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Register(Register model)
        {
            if(ModelState.IsValid)
            {
                User newUser = new User
                {
                    UserName =  model.Email,
                    Name = model.Name,
                    SurName = model.SurName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    CityPhone = model.CityPhone,
                    CompanyName = model.CompanyName,
                    IIN = model.IIN
                };
                model.PostalAddress.UserId = newUser.Id;
                model.LegalAddress.UserId = newUser.Id;
                
                var result = await _userManager.CreateAsync(newUser, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, "restaurateur");
                    await _signInManager.SignInAsync(newUser, false);
                    await _db.LegalAddresses.AddAsync(model.LegalAddress);
                    await _db.PostalAddresses.AddAsync(model.PostalAddress);
                    // await SendConfirmationEmailAsync(model.Email);      // Функционал подтверждение электронного адреса. Осталось доработать шаблон письма добавив информацию о пользователе. 
                    return RedirectToAction("Index", "Starkit");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(String.Empty, error.Description);
            }
            return View(model);
        }

        
        // Требует доработки, незаконченный action
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmailAsync(string email)
        {
            if (email != null)
            {
                User user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    user.EmailConfirmed = true;
                    _db.Users.Update(user);
                    await _db.SaveChangesAsync();
                    return null; // не готов 
                }
            }
            return NotFound();
        }
        
        
        
        [NonAction]
        private async Task SendConfirmationEmailAsync(string email)
        {
            string filePath = Path.Combine(_environment.ContentRootPath, "wwwroot/HTML_Template/ConfirmationEmail.txt");
            string message = await System.IO.File.ReadAllTextAsync(filePath);
            MailService emailServices = new MailService();
            await emailServices.SendEmailAsync(
                email,
                "Приветственное письмо!",
                message
            );
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login","Account");
        }

       
    }
}