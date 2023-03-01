using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace NEMS.Models
{
    public class TimeTable
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string uid { get; set; }
        [Required]
        public DateTime date {  get; set; }
        [AllowNull]
        public DateTime clockin { get; set; }
        [AllowNull]
        public DateTime clockout { get; set; }
        [AllowNull]
        public DateTime rClockin { get; set; }
        [AllowNull]
        public DateTime rClockout { get; set; }
        [AllowNull]
        public double worktime { get; set; }
        [AllowNull]
        public double ot { get; set; }
        [AllowNull]
        public double et { get; set; }
    }
}
