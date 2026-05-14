using Shared.Enums;

namespace DAL.Entities
{
    public class StorageAccount
    {
        public Guid Id { get; set; }

        public string BlobServicePath { get; set; }

        public Location Location { get; set; }

        public Redundancy Redundancy { get; set; }

        public bool Versioning { get; set; }
    }
}
