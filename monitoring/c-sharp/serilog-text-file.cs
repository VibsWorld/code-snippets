string folderPath = string.Empty;
Ilogger _logger = null;
folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
if (!Directory.Exists(folderPath))
{
    Directory.CreateDirectory(folderPath);
}
var filePath = Path.Combine(folderPath, "log.txt");
_logger = Log.Logger =
    new LoggerConfiguration().WriteTo.File(filePath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

// OR

 public static class SerilogHelper
    {
        /// <summary>
        /// By Default Log Writers will be created in /logs folder in root app
        /// </summary>
        /// <param name="absoluteFolderPath">Leave this null if not sure</param>
        /// <returns></returns>
        public static ILogger GetSerilogTextWriter(string absoluteFolderPath = null)
        {
            if (Log.Logger != null && Log.Logger.GetType().Name != "SilentLogger")
            {                
                return Log.Logger;                
            }

            string filePath;
            if (string.IsNullOrEmpty(absoluteFolderPath) == false)
            {
                filePath = Path.Combine(absoluteFolderPath, "log.txt");
            }
            else
            {
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "log.txt");
            }

            return new LoggerConfiguration().WriteTo.File(filePath, rollingInterval: RollingInterval.Day).CreateLogger();            
        }
    }
