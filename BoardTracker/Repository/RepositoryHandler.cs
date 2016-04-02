﻿using System;
using BoardTracker.Repository.Sql;

namespace BoardTracker.Repository
{
    public class RepositoryHandler
    {
        /// <summary>
        /// Returns the repository for a specific type of database
        /// </summary>
        /// <param name="type"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static ITrackingRepository GetTrackingRepository(RepositoryType type, string connectionString)
        {
            switch (type)
            {
                case RepositoryType.XML:
                    throw new NotImplementedException($"{nameof(RepositoryType.XML)} Repository is not implemented at the moment");

                case RepositoryType.MSSQL:
                    return new TrackingSqlLinq(connectionString);

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
