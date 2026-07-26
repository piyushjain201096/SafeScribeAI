using System.Threading.Tasks;

namespace SafeScribe.Core.Licensing
{
    public interface ILicenseService
    {
        LicenseInfo GetCurrentLicense();
        bool IsFeatureUnlocked(ProFeature feature);
        bool CanGenerateSummary();
        Task ConsumeTrialSummaryCreditAsync();
        Task<LicenseInfo> ActivateLicenseKeyAsync(string licenseKey);
        void DeactivateLicense();
    }
}
