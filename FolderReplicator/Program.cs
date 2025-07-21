using FolderReplicator;
using System;
using System.Threading;
using System.Threading.Tasks;

var sourcePath = IOOperations.GetSourceFolderPathFromConsole();
var replicaPath = IOOperations.GetReplicaFolderPathFromConsole(sourcePath);
var logPath = IOOperations.GetLogFilePathFromConsole(sourcePath, replicaPath);
var interval = IOOperations.GetIntervalFromConsole();

var replicator = new ReplicatorObject(sourcePath, replicaPath, logPath, interval);

var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(replicator.Interval));
var semaphore = new SemaphoreSlim(1, 1);

while (await timer.WaitForNextTickAsync())
{
    if (await semaphore.WaitAsync(0))
    {
        _ = Task.Run(() =>
        {
            try
            { 
                replicator.Replicate(); 
            }
            catch (Exception ex)
            {
                IOOperations.LogError(ex.Message, replicator.LogPath);
            }
            finally
            {
                semaphore.Release();
            }
        });
    }
}