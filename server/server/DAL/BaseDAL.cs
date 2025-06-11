namespace server.DAL
{
    public class BaseDAL
    {
        #region Members
        protected FileSystemAppDbContext _dbContext;
        #endregion

        #region Constructors
        public BaseDAL(FileSystemAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        #endregion
    }
}