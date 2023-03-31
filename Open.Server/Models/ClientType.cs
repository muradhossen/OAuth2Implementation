using System;
using System.Collections.Generic;
using System.Text;

namespace Catalyzr.Engine.Models.Client
{
    public class ClientType
    {
        public ClientType()
        {
            Clients=new List<Client>();
        }
        public int ClientTypeId { get; set; }
        public string ClientTypeName { get; set; }
        public List<Client> Clients { get; set; }
    }
}
