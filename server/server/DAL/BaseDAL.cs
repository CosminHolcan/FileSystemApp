using server.DAL;

namespace DataAbstractionLayer
{
    public class BaseDAL
    {
        #region Members
        protected MovieReviewDbContext _dbContext;
        #endregion

        #region Constructors
        public BaseDAL(MovieReviewDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        #endregion
    }
}