namespace server.DAL.Entities
{
    public class Review
    {
        public Guid ReviewId { get; set; }

        public string Text { get; set; }

        public bool WasEdited { get; set; }

        public DateTime CreationTime { get; set; }

        public Guid MovieId { get; set; }
        public Movie Movie { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
