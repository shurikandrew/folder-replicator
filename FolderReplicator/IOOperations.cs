namespace FolderReplicator;

public static class IOOperations
{
    public static string GetSourceFolderPathFromConsole()
    {
        Console.Write("Input path to the source folder: ");
        var path = Console.ReadLine();

        while (!Directory.Exists(path))
        {
            Console.Write("Sorry, the path is invalid or the program does not have access to it!\nProvide another path: ");
            path = Console.ReadLine();
        }

        return path;
    }
    
    public static string GetReplicaFolderPathFromConsole(string pathToCompare)
    {
        Console.Write("Input path to the replica folder: ");
        var path = Console.ReadLine();

        while (!Directory.Exists(path) || path == pathToCompare)
        {
            Console.WriteLine(path == pathToCompare
                ? "Sorry, the paths must be different!"
                : "Sorry, the path is invalid or the program does not have access to it!");

            Console.Write("Provide another path: ");
            path = Console.ReadLine();
        }

        return path;
    }
    
    public static string GetLogFilePathFromConsole(string sourcePath, string replicaPath)
    {
        Console.Write("Input path to log file (*.log): ");
        var path = Console.ReadLine();

        while (!File.Exists(path) || !Path.GetExtension(path).Equals(".log"))
        {
            if (File.Exists(path) && !Path.GetExtension(path).Equals(".log"))
            {
                Console.WriteLine("Sorry, the path must be to the file with .log extension!");
            }
            else if (path!.Contains(sourcePath) || path.Contains(replicaPath))
            {
                Console.WriteLine("Sorry, log file must be outside of source and replica folders!");
            }
            else
            {
                Console.WriteLine("Sorry, the path is invalid or the program does not have access to it!");
            }
            
            Console.Write("Provide another path: ");
            path = Console.ReadLine();
        }

        return path;
    }
    public static int GetIntervalFromConsole()
    {
        Console.Write("Input synchronization interval in milliseconds: ");
        var number = Console.ReadLine();
        int res;

        while (!int.TryParse(number, out res) || res <= 0)
        {
            Console.WriteLine(int.TryParse(number, out res)
                ? "Sorry, the number must be grater than 0!"
                : "Sorry, you did not input a number!");
            Console.Write("Provide another number: ");
            number = Console.ReadLine();
        }

        return res;
    }

    public static void LogAction(string obj, string logPath, LoggerMode mode)
    {
        var msg = 
            (mode == LoggerMode.Create ?
                "Added" :
                (mode == LoggerMode.Update ?
                    "Updated" :
                    "Removed"
                )) +  $" {obj} At: {DateTime.Now}";
        
        Console.WriteLine(msg);
                
        using var writeStream = new StreamWriter(logPath, append: true);
        writeStream.WriteLine(msg);
    }
    
    public static void LogError(string errorMessage, string logPath)
    {
        var msg = $" Error: {errorMessage} At: {DateTime.Now}";
        
        Console.WriteLine(msg);
                
        using var writeStream = new StreamWriter(logPath, append: true);
        writeStream.WriteLine(msg);
    }
}