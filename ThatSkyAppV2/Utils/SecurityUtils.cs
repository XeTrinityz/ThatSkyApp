using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace ThatSkyAppV2.Utils;

public static class SecurityUtils
{
    public static bool PatchDllHash(string dllPath)
    {
        try
        {
            var content = File.ReadAllBytes(dllPath);
            int hashOffset = FindHashOffset(content);
            if (hashOffset == -1) return false;

            byte[] hwid = GenerateHardwareId();
            byte[] originalBlock = new byte[72];
            Array.Copy(content, hashOffset, originalBlock, 0, 72);

            Array.Fill<byte>(content, 0, hashOffset, 72);

            using var sha256 = SHA256.Create();
            byte[] newHash = sha256.ComputeHash(content);

            Array.Copy(originalBlock, 0, content, hashOffset, 4);
            Array.Copy(newHash, 0, content, hashOffset + 4, 32);
            Array.Copy(hwid, 0, content, hashOffset + 36, 32);
            Array.Copy(originalBlock, 68, content, hashOffset + 68, 4);

            File.WriteAllBytes(dllPath, content);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static int FindHashOffset(byte[] content)
    {
        byte[] markerStart = BitConverter.GetBytes(0xDEADBEEF);
        byte[] markerEnd = BitConverter.GetBytes(0xCAFEBABE);

        for (int i = 0; i < content.Length - 72; i++)
        {
            if (content.Skip(i).Take(4).SequenceEqual(markerStart) &&
                content.Skip(i + 68).Take(4).SequenceEqual(markerEnd))
            {
                return i;
            }
        }
        return -1;
    }

    private static byte[] GenerateHardwareId()
    {
        var hwInfo = new StringBuilder();

        try
        {
            CollectCPUInfo(hwInfo);
            CollectVolumeInfo(hwInfo);
            CollectBIOSInfo(hwInfo);
        }
        catch (Exception)
        {
            // Fail silently and continue with whatever info we have
        }

        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.ASCII.GetBytes(hwInfo.ToString()));
    }

    private static void CollectCPUInfo(StringBuilder hwInfo)
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
        if (key?.GetValue("ProcessorNameString") is string processorName)
        {
            hwInfo.Append("CPU:").Append(processorName);
        }
    }

    private static void CollectVolumeInfo(StringBuilder hwInfo)
    {
        var systemDrive = Path.GetPathRoot(Environment.SystemDirectory);
        if (systemDrive != null)
        {
            if (GetVolumeInformation(systemDrive, null, 0, out uint serialNumber, out _, out _, null, 0))
            {
                hwInfo.AppendFormat("VOL:{0:X8}", serialNumber);
            }
        }
    }

    private static void CollectBIOSInfo(StringBuilder hwInfo)
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
        if (key?.GetValue("SystemManufacturer") is string manufacturer)
        {
            hwInfo.Append("BIOS:").Append(manufacturer);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetVolumeInformation(
        string rootPathName,
        StringBuilder? volumeNameBuffer,
        int volumeNameSize,
        out uint volumeSerialNumber,
        out uint maximumComponentLength,
        out uint fileSystemFlags,
        StringBuilder? fileSystemNameBuffer,
        int fileSystemNameSize);
}
