using System;
using BoardTracker.BusinessLogic.DataProvider.Universal;

namespace BoardTracker.BusinessLogic.DataProvider
{
    public class DataProviderHandler
    {

        /// <summary>
        /// Create a data provider instance for a specfic provider type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IDataProvider GetProvider(DataProviderType type)
        {
            switch (type)
            {
                case DataProviderType.Universal:
                    return new UniversalDataProvider();

                case DataProviderType.Reddit:
                    throw new NotImplementedException();

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
