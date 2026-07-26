using System;
using System.Runtime.InteropServices;

namespace SafeScribe.Core.Hardware
{
    public class HardwareProfile
    {
        public ulong TotalPhysicalMemoryGb { get; set; }
        public ulong AvailableMemoryGb { get; set; }
        public int CpuCores { get; set; }
        public bool Is8GbRamOrLess => TotalPhysicalMemoryGb <= 8;
        public string RecommendedOllamaModel { get; set; } = "llama3.2:3b";
        public string RecommendedModelQuantization { get; set; } = "fp16";
    }

    public static class HardwareDetector
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        public static HardwareProfile Detect()
        {
            var profile = new HardwareProfile
            {
                CpuCores = Environment.ProcessorCount
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var memStatus = new MEMORYSTATUSEX();
                memStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));

                if (GlobalMemoryStatusEx(ref memStatus))
                {
                    profile.TotalPhysicalMemoryGb = (ulong)Math.Round(memStatus.ullTotalPhys / (1024.0 * 1024.0 * 1024.0));
                    profile.AvailableMemoryGb = (ulong)Math.Round(memStatus.ullAvailPhys / (1024.0 * 1024.0 * 1024.0));
                }
            }
            else
            {
                profile.TotalPhysicalMemoryGb = 8;
                profile.AvailableMemoryGb = 4;
            }

            if (profile.TotalPhysicalMemoryGb <= 8)
            {
                profile.RecommendedOllamaModel = "llama3.2:3b";
                profile.RecommendedModelQuantization = "fp16";
            }
            else if (profile.TotalPhysicalMemoryGb <= 16)
            {
                profile.RecommendedOllamaModel = "llama3.1:8b";
                profile.RecommendedModelQuantization = "q4_K_M";
            }
            else
            {
                profile.RecommendedOllamaModel = "qwen2.5:14b";
                profile.RecommendedModelQuantization = "q4_K_M";
            }

            return profile;
        }
    }
}
