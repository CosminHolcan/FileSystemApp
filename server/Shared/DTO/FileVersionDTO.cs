namespace Shared.DTO
{
    public class FileVersionDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AzureId { get; set; }
        public Guid OriginalFileId { get; set; }
        public string CreationTime { get; set; }
        public string TokenSAS { get; set; }
    }
}
