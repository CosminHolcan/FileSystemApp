using server.Enums;

namespace server.DAL.Entities
{
    public class StorageAccount
    {
        public Guid Id { get; set; }

        public string ConnectionString { get; set; }

        public Location Location { get; set; }

        public Redundancy Redundancy { get; set; }

        public bool Versioning { get; set; }
    }
}
