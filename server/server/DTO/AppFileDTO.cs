using server.DAL.Entities;

namespace server.DTO
{
    public class AppFileDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string StorageAccount { get; set; }
    }
}
