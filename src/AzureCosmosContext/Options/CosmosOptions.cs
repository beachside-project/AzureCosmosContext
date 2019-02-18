using System;

namespace AzureCosmosContext.Options
{
    public class CosmosOptions
    {
        public string CurrentRegion { get; set; }

        public string DatabaseId { get; set; }

        public int DefaultThroughput { get; set; }

        public ThrottlingRetryOptions ThrottlingRetryOptions { get; set; }

        public CosmosContainerOptions[] CosmosContainerOptions { get; set; }

        public bool NeedCreateIfExist { get; set; } = true;

        /// <summary>
        /// プロパティの値の validation。異常があればExceptionを発生させてアプリを止めます。
        /// </summary>
        public void Validate()
        {
            Validate(DatabaseId, nameof(DatabaseId));
            ValidateDefaultThroughput();
            ValidateThrottlingRetryOptions(ThrottlingRetryOptions);
        }

        private static void Validate(string property, string propertyName)
        {
            if (string.IsNullOrEmpty(property)) throw new ArgumentNullException(propertyName);
        }

        private const int CosmosMinThroughput = 400;

        private void ValidateDefaultThroughput()
        {
            if (DefaultThroughput < CosmosMinThroughput)
                throw new ArgumentOutOfRangeException(nameof(DefaultThroughput));
            // 最大値のチェックは今はしてない。
        }

        private static void ValidateThrottlingRetryOptions(ThrottlingRetryOptions throttlingRetryOptions)
        {
            if (throttlingRetryOptions == null) return; // 定義なしはOK
            if (throttlingRetryOptions.NumberOfRetries <= 0)
                throw new ArgumentOutOfRangeException(nameof(throttlingRetryOptions.NumberOfRetries));
            if (throttlingRetryOptions.MaxRetryWaitTimeInSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(throttlingRetryOptions.MaxRetryWaitTimeInSeconds));
        }
    }
}