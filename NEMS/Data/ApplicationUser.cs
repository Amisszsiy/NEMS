using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace NEMS.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
    }
}
