using DataAbstractionLayer;
using Microsoft.EntityFrameworkCore;
using server.DAL.Entities;

namespace server.DAL
{
    public class MoviesDAL : BaseDAL
    {
        public MoviesDAL(MovieReviewDbContext dbContext) : base(dbContext) { }

        public async Task<Movie> AddMovie(Movie movie)
        {
            this._dbContext.Movies.Add(movie);
            await this._dbContext.SaveChangesAsync();

            return movie;
        }

        public async Task UpdateMovie(Movie movie)
        {
            Movie existingMovie = await this._dbContext.Movies.FirstOrDefaultAsync(m => m.MovieId == movie.MovieId);
            if (existingMovie == null)
                throw new Exception("There is no movie with this id.");

            existingMovie.Title = movie.Title;
            existingMovie.Genre = movie.Genre;
            existingMovie.ApparitionDate = movie.ApparitionDate;

            await this._dbContext.SaveChangesAsync();
        }

        public async Task DeleteMovie(Guid id)
        {
            Movie existingMovie = await this._dbContext.Movies.FirstOrDefaultAsync(m => m.MovieId == id);
            if (existingMovie == null)
                throw new Exception("There is no movie with this id.");

            this._dbContext.Movies.Remove(existingMovie);
            await this._dbContext.SaveChangesAsync();
        }

        public async Task<List<Movie>> GetAllMovies()
        {
            return await this._dbContext.Movies.Include(m => m.User).ToListAsync();
        }

        public async Task<List<Movie>> GetMoviesByUserId(Guid userId)
        {
            return await this._dbContext.Movies.Include(m => m.User).Where(m => m.UserId == userId).ToListAsync();
        }

        public async Task<Movie> GetMovieByTitle(string title)
        {
            return await this._dbContext.Movies.Include(m => m.User).FirstOrDefaultAsync((Movie movie) => movie.Title == title);
        }

        public async Task<Movie> GetMovieById(Guid id)
        {
            return await this._dbContext.Movies.Include(m => m.User).FirstOrDefaultAsync((Movie movie) => movie.MovieId == id);
        }
    }
}
