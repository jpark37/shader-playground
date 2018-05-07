using System;
using System.Diagnostics;

namespace ShaderPlayground.Core.Util
{
    internal static class ProcessHelper
    {
        public static void Run(string fileName, string arguments, out string stdOutput, out string stdError)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                var stdOutputTemp = string.Empty;
                process.OutputDataReceived += (sender, e) =>
                {
                    stdOutputTemp += e.Data + Environment.NewLine;
                };

                process.BeginOutputReadLine();

                stdError = process.StandardError.ReadToEnd();

                process.WaitForExit(4000);

                stdOutput = stdOutputTemp;
            }
        }
    }
}
