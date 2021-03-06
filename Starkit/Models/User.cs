﻿using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Starkit.ViewModels;

namespace Starkit.Models
{
    public enum UserStatus
    {
        Locked,
        Unlocked
    }
    public enum EmployeePosition
    {
        Registrant,
        ContentManager,
        AdministratorRestaurant
    }
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public string SurName { get; set; }
        public string CompanyName { get; set; }
        public string IIN { get; set; }
        public string CityPhone { get; set; }
        public string AvatarPath { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Unlocked;
        public bool IsTermsAccepted { get; set; }
        public string RestaurantId { get; set; }
        public string IdOfTheSelectedRestaurateur { get; set; }
        public EmployeePosition Position { get; set; } = EmployeePosition.Registrant;
        [NotMapped]
        public Restaurant Restaurant { get; set; }
        [NotMapped]
        public LegalAddress LegalAddress { get; set; }
        [NotMapped]
        public PostalAddress PostalAddress { get; set; }
        public string GoogleMapsApi { get; set; }
    }
}