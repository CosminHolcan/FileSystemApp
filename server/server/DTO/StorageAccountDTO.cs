using server.Enums;

namespace server.DTO
{
    public class StorageAccountDTO
    {
        public Guid Id { get; set; }
        public Location Location { get; set; }
        public Redundancy Redundancy { get; set; }
        public bool Versioning { get; set; }
    }
}
