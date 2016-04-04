namespace BoardTracker.Configuration.Model
{
    public class RequestRateConfiguration
    {
        /// <summary>
        /// The amount of minutes the process will sleep after each track-rotation
        /// </summary>
        public int RequestRateInMinutes;

        /// <summary>
        /// Unfortunately we don't want to DDOS the server, we have to limit the amount of requests and take a break for a specific amount of milliseconds
        /// </summary>
        public int RequestsTillSleep;

        /// <summary>
        /// The amount of milliseconds we will sleep at each timeout
        /// </summary>
        public int RequestSleepInMilliseconds;

        /// <summary>
        /// The amount of milliseconds we will sleep after each request
        /// </summary>
        public int RequestDelayInMilliseconds;
    }
}
