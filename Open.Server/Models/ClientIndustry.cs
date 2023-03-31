using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;
using System.Text;

namespace Catalyzr.Engine.Models.Client
{
    public class ClientIndustry
    {
        public int ClientIndustryId { get; set; }
        [Required]
        public string ClientIndustryName { get; set; }
        public bool IsDelete { get; set; }
    }
}
