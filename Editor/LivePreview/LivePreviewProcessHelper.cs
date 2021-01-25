using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages.Excursion360_Builder.Editor.LivePreview
{
    class LivePreviewProcessHelper
    {
        public static Process StartLivePreviewBackend(string executablePath)
        {
            if (FindExistingProcess(executablePath, out var process))
            {
                return process;
            }
            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    RedirectStandardOutput = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            return process;
        }
        private static bool FindExistingProcess(string executablePath, out Process process)
        {
            var targetProcess = Process.GetProcesses()
                .Where(p => p.ProcessName.Contains("Web"))
                .Select(p =>
                {
                    try { return new { process = p, module = p.MainModule }; } catch { return null; }
                })
                .Where(m => m != null)
                .FirstOrDefault(m => m.module.FileName == executablePath);
            if (targetProcess != null)
            {
                process = targetProcess.process;
                return true;
            }
            else
            {
                process = null;
                return false;
            }
        }
    }
}
