using GamesResults.Models;
using System.Diagnostics;

namespace PirAppBp.Utils
{
    public static class ProcessUtils
    {
        public static Task<int> RunAsync(string fileName, string args, string workingDirectory)
        {
            var source = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = {
                    FileName = fileName, 
                    Arguments = args,
                    UseShellExecute = false, 
                    CreateNoWindow = true,
                    RedirectStandardOutput = true, 
                    RedirectStandardError = true,
                    WorkingDirectory = workingDirectory
                },
                EnableRaisingEvents = true,
            };

            process.Exited += (sender, args) =>
            {
                source.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();

            return source.Task;
        }
    }
}
