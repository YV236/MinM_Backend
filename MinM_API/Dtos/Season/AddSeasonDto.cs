﻿using MinM_API.Models;

namespace MinM_API.Dtos.Season
{
    public class AddSeasonDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual List<string> ProductIds { get; set; } = [];
    }
}
