
namespace Catalyzr.Engine.Models.Client
{
    public class ClientOrphanGroup
    {
        public long Id { get; set; }
        public int GroupId { get; set; }
        //public Group Group { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public string ErrorDescription { get; set; }
    }
}
