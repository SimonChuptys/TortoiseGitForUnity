using System.Diagnostics;
namespace Vintecc.TortoiseGitForUnity
{
    public static class TortoiseGitRunner
    {
        // .. FIELDS

        private static string TortoiseGitProcPath = @"C:\Program Files\TortoiseGit\bin\TortoiseGitProc.exe";

        // .. OPERATIONS

        /// <summary>
        /// Execute a git command on specified repository
        /// </summary>
        /// <param name="cmd">The command to execute</param>
        /// <param name="path">The repository path</param>
        public static void Do(Command cmd, string path)
        {
            RunExecutable(TortoiseGitProcPath, "/command:" + cmd.ToString().ToLower() + " /path:\"" + path);
        }

        private static void RunExecutable(string filename, string arguments)
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = filename,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }

        public enum Command
        {
            None,
            Commit,
            Log,
            Fetch,
            Push
        }
    }
}