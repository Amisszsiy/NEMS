using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace NEMS.Models
{
    public class Event
    {
        [Key]
        public int id {  get; set; }
        [Required]
        public string title { get; set; }
        [Required]
        public DateTime start { get; set; }
        [Required]
        public DateTime end { get; set; }
        [Required]
        public string user { get; set; }
        [Required]
        public bool isOwner { get; set; }
        [AllowNull]
        public string description { get; set; }
    }
}
