using server.Enums;

namespace server.DTO
{
    public class AppFileDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid StorageAccountId { get; set; }

        public Location? Location { get; set; }

        public Redundancy? Redundancy { get; set; }

        public bool? Versionning { get; set; }

        public string? CreationDate { get; set; }
    }
}
