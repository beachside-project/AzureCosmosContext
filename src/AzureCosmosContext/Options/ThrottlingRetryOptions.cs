using System;

namespace AzureCosmosContext.Options
{
    public class ThrottlingRetryOptions
    {
        public int MaxRetryWaitTimeInSeconds { get; set; }
        public TimeSpan MaxRetryWaitTimeOnThrottledRequests => new TimeSpan(0, 0, MaxRetryWaitTimeInSeconds);

        public int NumberOfRetries { get; set; }

        public int MaxRetryAttemptsOnThrottledRequests => NumberOfRetries;
    }
}