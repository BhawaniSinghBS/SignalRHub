using System.IO;

namespace TCP.UDP.Constants
{
    public class Constants
    {
        public static string AppWebRootPath { get; set; } = "D:\\";
        public static string AppContentRootPath { get; set; } = "D:\\";

        public static string LogDirectoryPath
        {
            get => Path.Combine(AppContentRootPath, @"Logs");
        }
        public static string LogFilePath
        {
            get => Path.Combine(LogDirectoryPath, @"FileLogs.txt");
        }
        public static readonly object fileLockObject = new object();
    }
}
