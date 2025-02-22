using DataAbstractionLayer;
using Microsoft.EntityFrameworkCore;
using server.DAL.Entities;

namespace server.DAL
{
    public class ReviewsDAL : BaseDAL
    {
        public ReviewsDAL(MovieReviewDbContext dbContext) : base(dbContext) { }

        public async Task<Review> AddReview(Review review)
        {
            Movie existingMovie = await this._dbContext.Movies.FirstOrDefaultAsync((Movie movie) => movie.MovieId == review.MovieId);
            if (existingMovie == null)
                throw new Exception("There is no movie with this id.");

            this._dbContext.Reviews.Add(review);
            await this._dbContext.SaveChangesAsync();

            return review;
        }

        public async Task UpdateReview(Review review)
        {
            Review existingReview = await this._dbContext.Reviews.FirstOrDefaultAsync((Review r) => r.ReviewId == review.ReviewId);
            if (existingReview == null)
                throw new Exception("There is no review with this id.");

            existingReview.Text = review.Text;
            existingReview.WasEdited = true;

            await this._dbContext.SaveChangesAsync();
        }

        public async Task DeleteReview(Guid id)
        {
            Review existingReview = await this._dbContext.Reviews.FirstOrDefaultAsync((Review r) => r.ReviewId == id);
            if (existingReview == null)
                throw new Exception("There is no review with this id.");

            this._dbContext.Reviews.Remove(existingReview);
            await this._dbContext.SaveChangesAsync();
        }

        public async Task<List<Review>> GetAllReviews()
        {
            return await this._dbContext.Reviews.Include(m => m.User).ToListAsync();
        }

        public async Task<List<Review>> GetReviewsByMovie(Guid movieId)
        {
            return await this._dbContext.Reviews.Include(m => m.User).Where((Review review) => review.MovieId == movieId).ToListAsync();
        }

        public async Task<List<Review>> GetReviewsByUserId(Guid userId)
        {
            return await this._dbContext.Reviews.Include(r => r.User).Include(r => r.Movie).Where((Review review) => review.UserId == userId).ToListAsync();
        }

        public async Task<Review> GetReviewById(Guid id)
        {
            return await this._dbContext.Reviews.Include(m => m.User).FirstOrDefaultAsync((Review review) => review.ReviewId == id);
        }
    }
}