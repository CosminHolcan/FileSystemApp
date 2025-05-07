namespace server.DTO
{
    public class FileWithVersionsDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<FileVersionDTO> Versions { get; set; }
    }
}
