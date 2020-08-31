﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class TablesController : Controller
    {
        private StarkitContext _db;
        private UserManager<User> _userManager;
        private IHostEnvironment _environment;
        private UploadService _uploadService;

        public TablesController(StarkitContext db, UserManager<User> userManager, IHostEnvironment environment, UploadService uploadService)
        {
            _db = db;
            _userManager = userManager;
            _environment = environment;
            _uploadService = uploadService;
        }
        [Authorize(Roles = "SuperAdmin,Registrant,ContentManager")]
        private async Task<string> Load(int id, IFormFile file)
        {
            User user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            if (User.IsInRole(Convert.ToString(Roles.SuperAdmin)))
            {
                string userId = user.IdOfTheSelectedRestaurateur;
                user = await _userManager.FindByIdAsync(userId);
            }
            string path = Path.Combine(_environment.ContentRootPath + $"\\wwwroot\\images\\restaurants\\{user.RestaurantId}\\Tables\\{id}");
            string photoPath = $"images/restaurants/{user.RestaurantId}/Tables/{id}/{file.FileName}";
            if (!Directory.Exists($"wwwroot/images/restaurants/{user.RestaurantId}/Tables/{id}"))
            {
                Directory.CreateDirectory($"wwwroot/images/restaurants/{user.RestaurantId}/Tables/{id}");
            }
            await _uploadService.Upload(path, file.FileName, file);
            return photoPath;
        }
        
        [Authorize(Roles = "SuperAdmin,Registrant,ContentManager")]
        private async Task DeleteTableIcon(Table table)
        {
            User user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            if (User.IsInRole(Convert.ToString(Roles.SuperAdmin)))
            {
                string userId = user.IdOfTheSelectedRestaurateur;
                user = await _userManager.FindByIdAsync(userId);
            }
            string filePath = _environment.ContentRootPath + $"\\wwwroot\\images\\restaurants\\{user.RestaurantId}\\Tables\\" + table.Id; 
            if (Directory.Exists(filePath))
            {
                if (table.IconUrl == null)
                    Directory.Delete(filePath, true);
                else
                    System.IO.File.Delete("wwwroot/" + table.IconUrl);
            }
        }
        // GET
        public async Task<IActionResult> Index()
        {
            User user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            if (User.IsInRole(Convert.ToString(Roles.SuperAdmin)))
            {
                string userId = user.IdOfTheSelectedRestaurateur;
                user = await _userManager.FindByIdAsync(userId);
            }

            List<Table> tables = _db.Tables.Where(t => t.RestaurantId == user.RestaurantId).ToList();
            return View(tables);
        }

        [Authorize(Roles = "SuperAdmin,Registrant")]

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            User user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            if (User.IsInRole(Convert.ToString(Roles.SuperAdmin)))
            {
                string userId = user.IdOfTheSelectedRestaurateur;
                user = await _userManager.FindByIdAsync(userId);
            }
            if (user.RestaurantId == null)
                return RedirectToAction("Register", "Restaurants");
            return View();
        }
        
        [Authorize(Roles = "SuperAdmin,Registrant")]
        [HttpPost]
        public async Task<IActionResult> Create(Table table)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
                if (User.IsInRole(Convert.ToString(Roles.SuperAdmin)))
                    user = await _userManager.FindByIdAsync(user.IdOfTheSelectedRestaurateur);
                table.IconUrl = await Load(table.Id, table.File);
                table.RestaurantId = user.RestaurantId;
                table.CreatorId = user.Id;
                table.RestaurantId = user.RestaurantId;
                _db.Entry(table).State = EntityState.Added;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(table);
        }
        
        [Authorize(Roles = "SuperAdmin,Registrant")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            Table table = new Table{Id = id};
            _db.Entry(table).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
            await DeleteTableIcon(table);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Registrant")]
        public IActionResult Edit(int id)
        {
            Table table = _db.Tables.FirstOrDefault(t => t.Id == id);
            if (table != null)
            {
                EditTableViewModel model = new EditTableViewModel
                {
                    Id = table.Id,
                    Capacity = table.Capacity,
                    IconUrl = table.IconUrl,
                    State = table.State,
                    Desc = table.Desc,
                    Location = table.Location,
                    IsSmoking = table.IsSmoking,
                    IsQuiet = table.IsQuiet,
                    Floor = table.Floor,
                };
                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Registrant")]
        public async Task<IActionResult> Edit(EditTableViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
                if (User.IsInRole(Convert.ToString(Roles.SuperAdmin)))
                    user = await _userManager.FindByIdAsync(user.IdOfTheSelectedRestaurateur);
                var table = _db.Tables.FirstOrDefault(t => t.Id == model.Id);
                if (table != null)
                {
                    table.Capacity = model.Capacity;
                    table.State = model.State;
                    table.Desc = model.Desc;
                    table.Location = model.Location;
                    table.IsSmoking = model.IsSmoking;
                    table.IsQuiet = model.IsQuiet;
                    table.Floor = model.Floor;
                    table.EditedDate = DateTime.Now;
                    table.EditorId = user.Id;
                    if (model.File != null)
                    {
                        await DeleteTableIcon(table);
                        table.IconUrl = await Load(model.Id, model.File);
                    }

                    _db.Entry(table).State = EntityState.Modified;
                }

                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View();
        }
        
    }
}