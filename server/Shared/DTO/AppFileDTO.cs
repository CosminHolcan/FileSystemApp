using Shared.Enums;

namespace Shared.DTO
{
    public class AppFileDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid StorageAccountId { get; set; }

        public Location? Location { get; set; }

        public Location? SecondaryLocation { get; set; }

        public Redundancy? Redundancy { get; set; }

        public bool? Versioning { get; set; }

        public string? CreationDate { get; set; }

        public string? VersionName { get; set; }

        public Guid? ReplicaId { get; set; }

        public bool? IsReplica { get; set; }

        public string? TokenSAS { get; set; }
    }
}
