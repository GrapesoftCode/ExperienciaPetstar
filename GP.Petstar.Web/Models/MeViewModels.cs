using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GP.Petstar.Web.Models
{
    // Models returned by MeController actions.
    public class GetViewModel
    {
        public string Email { get; set; }
        public string Nombres { get; set; }
        public string ApPaterno { get; set; }
        public string ApMaterno { get; set; }
    }
}