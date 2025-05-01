namespace server.DAL.Entities
{
    public class AppFile
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid StorageAccountId { get; set; }
        public StorageAccount StorageAccount { get; set; }

        public DateOnly LastInteraction { get; set; }

        public DateOnly CreationDate { get; set; }

        public virtual ICollection<FileVersion> Versions { get; set; }
    }
}
