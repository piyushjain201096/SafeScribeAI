using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SafeScribe.Core.Licensing
{
    public class FeatureCatalogManager
    {
        private readonly string _catalogFilePath;
        private FeatureCatalogConfig _catalogConfig;

        public FeatureCatalogManager(string? customFilePath = null)
        {
            if (string.IsNullOrEmpty(customFilePath))
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string folder = Path.Combine(appData, "SafeScribeAI");
                Directory.CreateDirectory(folder);
                _catalogFilePath = Path.Combine(folder, "features_catalog.json");
            }
            else
            {
                _catalogFilePath = customFilePath;
            }

            _catalogConfig = LoadCatalog();
        }

        public FeatureCatalogConfig GetCatalog() => _catalogConfig;

        public FeatureConfig? GetFeature(string featureId)
        {
            return _catalogConfig.Features.FirstOrDefault(f => f.Id.Equals(featureId, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsFeatureUnlocked(string featureId)
        {
            var f = GetFeature(featureId);
            return f?.IsUnlocked ?? false;
        }

        public async Task UnlockFeatureAsync(string featureId)
        {
            var f = GetFeature(featureId);
            if (f != null)
            {
                f.IsUnlocked = true;
                await SaveCatalogAsync();
            }
        }

        public async Task UpdateFeaturePriceAsync(string featureId, decimal newPrice, string? checkoutUrl = null)
        {
            var f = GetFeature(featureId);
            if (f != null)
            {
                f.Price = newPrice;
                if (!string.IsNullOrEmpty(checkoutUrl)) f.CheckoutUrl = checkoutUrl;
                await SaveCatalogAsync();
            }
        }

        private FeatureCatalogConfig LoadCatalog()
        {
            try
            {
                if (File.Exists(_catalogFilePath))
                {
                    string json = File.ReadAllText(_catalogFilePath);
                    var config = JsonSerializer.Deserialize<FeatureCatalogConfig>(json);
                    if (config != null && config.Features.Count > 0) return config;
                }
            }
            catch { }

            // Default Configurable Feature Catalog
            return GetDefaultCatalog();
        }

        private async Task SaveCatalogAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(_catalogConfig, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_catalogFilePath, json);
            }
            catch { }
        }

        public static FeatureCatalogConfig GetDefaultCatalog()
        {
            return new FeatureCatalogConfig
            {
                Currency = "USD",
                Features = new List<FeatureConfig>
                {
                    new FeatureConfig
                    {
                        Id = "diarization",
                        Name = "Speaker Diarization Engine",
                        Price = 9.99m,
                        Description = "Voice fingerprinting & speaker identification (Who said what)",
                        CheckoutUrl = "https://safescribe.ai/buy/diarization",
                        IsUnlocked = false
                    },
                    new FeatureConfig
                    {
                        Id = "enterprise_exporters",
                        Name = "Enterprise Exporters Pack",
                        Price = 9.99m,
                        Description = "Obsidian Vault Auto-Sync, Notion Sync, Slack/Teams Webhooks, ICS Calendar",
                        CheckoutUrl = "https://safescribe.ai/buy/exporters",
                        IsUnlocked = false
                    },
                    new FeatureConfig
                    {
                        Id = "model_sales_crm",
                        Name = "Sales & CRM Fine-Tuned Model Pack",
                        Price = 14.99m,
                        Description = "Fine-tuned Ollama model for BANT qualification, deal risk analysis & CRM notes",
                        CheckoutUrl = "https://safescribe.ai/buy/sales-pack",
                        IsUnlocked = false
                    },
                    new FeatureConfig
                    {
                        Id = "model_legal_depo",
                        Name = "Legal Deposition Model Pack",
                        Price = 24.99m,
                        Description = "Fine-tuned Ollama model for legal testimony & cross-examination summaries",
                        CheckoutUrl = "https://safescribe.ai/buy/legal-pack",
                        IsUnlocked = false
                    }
                }
            };
        }
    }
}
