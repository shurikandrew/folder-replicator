using FolderReplicator;

Console.Write("Input path to the source folder: ");
var sourcePath = IOOperations.GetFolderPathFromConsole();

Console.Write("Input path to the replica folder: ");
var replicaPath = IOOperations.GetFolderPathFromConsole(sourcePath);

Console.Write("Input path to log file: ");
var logPath = IOOperations.GetFilePathFromConsole(sourcePath, replicaPath);

Console.Write("Input synchronization interval in milliseconds: ");
var interval = IOOperations.GetIntegerFromConsole();

var replicator = new ReplicatorObject(sourcePath, replicaPath, logPath, interval);

while (true)
{
    replicator.Replicate();
    //write to log file and to console every change
    Thread.Sleep(replicator.Interval);
}