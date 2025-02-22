namespace server.DAL.Entities
{
    public class User
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public virtual ICollection<Movie> Movies { get; set; }

        public virtual ICollection<Review> Reviews { get; set; }
    }
}
