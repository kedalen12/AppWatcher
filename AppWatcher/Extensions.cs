using System.Diagnostics;
using System.Management;

namespace AppWatcher;

public static class Extensions
{
    public static IList<Process> GetChildProcesses(this Process process) 
        => new ManagementObjectSearcher(
                $"Select * From Win32_Process Where ParentProcessID={process.Id}")
            .Get()
            .Cast<ManagementObject>()
            .Select(mo =>
                Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])))
            .ToList();
}