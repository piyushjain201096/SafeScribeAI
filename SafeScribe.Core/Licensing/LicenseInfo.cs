using System;

namespace SafeScribe.Core.Licensing
{
    public enum LicenseTier
    {
        Free,
        ProLifetime,
        Enterprise
    }

    public class LicenseInfo
    {
        public string LicenseKey { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public LicenseTier Tier { get; set; } = LicenseTier.Free;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public bool IsValid { get; set; } = false;
        public string StatusMessage { get; set; } = string.Empty;
    }
}
