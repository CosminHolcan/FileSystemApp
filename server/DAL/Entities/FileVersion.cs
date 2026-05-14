namespace DAL.Entities
{
    public class FileVersion
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string AzureId { get; set; }

        public Guid OriginalFileId { get; set; }
        public AppFile OriginalFile { get; set; }

        public DateTime CreationTime { get; set; }
    }
}
