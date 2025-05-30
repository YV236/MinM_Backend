﻿namespace MinM_API.Models
{
    public class Season
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual List<Product> Products { get; set; } = [];
    }
}
