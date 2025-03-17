namespace server.DAL.Entities
{
    public class FileVersion
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid AzureId { get; set; }

        public Guid OriginalFileId { get; set; }
        public AppFile OriginalFile { get; set; }

        public DateOnly CreationTime { get; set; }
    }
}
