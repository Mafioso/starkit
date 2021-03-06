﻿using System.Collections.Generic;
using Starkit.Models;

namespace Starkit.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<Dish> Dishes { get; set; }
        public IEnumerable<Booking> Bookings { get; set; }
        public IEnumerable<Table> Tables { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterViewModel FilterViewModel { get; set; }
        public BookingTablesFilterViewModel BookingTablesFilterViewModel { get; set; }
        public SortViewModel SortViewModel { get; set; }
    }
}