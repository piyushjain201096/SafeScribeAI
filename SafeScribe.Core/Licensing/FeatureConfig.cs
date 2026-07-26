using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SafeScribe.Core.Licensing
{
    public class FeatureConfig
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; } = 0.00m;

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "USD";

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("checkoutUrl")]
        public string CheckoutUrl { get; set; } = string.Empty;

        [JsonPropertyName("isUnlocked")]
        public bool IsUnlocked { get; set; } = false;
    }

    public class FeatureCatalogConfig
    {
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "USD";

        [JsonPropertyName("features")]
        public List<FeatureConfig> Features { get; set; } = new();
    }
}
