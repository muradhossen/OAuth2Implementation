using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Catalyzr.Engine.Models.Client
{
    public class Client
    {
        public Client()
        {
            
        }
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }       
    }
}
