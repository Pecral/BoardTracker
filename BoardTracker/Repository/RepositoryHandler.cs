using System;
using BoardTracker.Repository.MsSql;
using BoardTracker.Repository.MySql;

namespace BoardTracker.Repository
{
    public class RepositoryHandler
    {
        /// <summary>
        /// Returns the repository for a specific type of database
        /// </summary>
        /// <param name="type">The repository type of the database</param>
        /// <param name="connectionString">The connection string to the database</param>
        /// <returns></returns>
        public static ITrackingRepository GetTrackingRepository(RepositoryType type, string connectionString)
        {
            switch (type)
            {
                case RepositoryType.MSSQL:
                    return new TrackingSqlLinq(connectionString);

                case RepositoryType.MYSQL:
                    return new TrackingMySQL(connectionString);

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
