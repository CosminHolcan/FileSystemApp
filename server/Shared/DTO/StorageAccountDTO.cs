using Shared.Enums;

namespace Shared.DTO
{
    public class StorageAccountDTO
    {
        public Guid Id { get; set; }
        public Location Location { get; set; }
        public Redundancy Redundancy { get; set; }
        public bool Versioning { get; set; }
    }
}
