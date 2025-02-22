using server.DAL.Enums;

namespace server.DAL.Entities
{
    public class Movie
    {
        public Guid MovieId { get; set; }

        public string Title { get; set; }

        public Genre Genre { get; set; }

        public DateTime ApparitionDate { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public virtual ICollection<Review> Reviews { get; set; }
    }
}
