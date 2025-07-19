using FolderReplicator;

Console.Write("Input path to the source folder: ");
var sourcePath = IOOperations.GetFolderPathFromConsole();

Console.Write("Input path to the replica folder: ");
var replicaPath = IOOperations.GetFolderPathFromConsole(sourcePath);

Console.Write("Input path to log file: ");
var logPath = IOOperations.GetFilePathFromConsole(sourcePath, replicaPath);

Console.Write("Input synchronization interval: ");
var interval = IOOperations.GetIntegerFromConsole();


while (true)
{
    string[] sourceFiles = Directory.GetFiles(sourcePath);
    string[] replicaFiles = Directory.GetFiles(replicaPath);
}