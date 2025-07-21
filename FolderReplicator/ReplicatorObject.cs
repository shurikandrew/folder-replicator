using System.Security.Cryptography;

namespace FolderReplicator;

public class ReplicatorObject
{
    private string SourcePath { get; set; }
    private string ReplicaPath { get; set; }
    private Dictionary<string, string> _replicaFolderFiles;
    public string LogPath { get; set; }
    public int Interval { get; set; }
    

    public ReplicatorObject(string sourcePath, string replicaPath, string logPath, int interval)
    {
        SourcePath = sourcePath;
        ReplicaPath = replicaPath;
        LogPath = logPath;
        Interval = interval;

        _replicaFolderFiles = new();
        ClearReplicaFolder();
    }

    private void ClearReplicaFolder()
    {
        foreach (string file in Directory.GetFiles(ReplicaPath, "*", SearchOption.AllDirectories))
        {
            File.Delete(file);
        }

    }

    public void Replicate()
    {
        ReplicateFiles();
        ReplicateEmptyFolders();
    }

    private void ReplicateFiles()
    {
        string[] sourceFolderFiles = Directory.GetFiles(SourcePath, "*", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".DS_Store"))
            .ToArray();
        
        for (int i = 0; i < sourceFolderFiles.Length; i++)
        {
            var sourceFilePath = sourceFolderFiles[i];
            var fileReplicaPath = Path.Combine(ReplicaPath, Path.GetRelativePath(SourcePath, sourceFilePath));
            
            if (!_replicaFolderFiles.ContainsKey(sourceFilePath))
            {
                CreateFileReplica(sourceFilePath, fileReplicaPath);
                
                IOOperations.LogAction(Path.GetRelativePath(SourcePath, sourceFilePath), LogPath, LoggerMode.Create);
            }
            else
            {
                var hash = ComputeHash(sourceFilePath);

                if (hash != _replicaFolderFiles[sourceFilePath])
                {
                    UpdateFileReplica(sourceFilePath, fileReplicaPath, hash);
                    
                    IOOperations.LogAction(Path.GetRelativePath(SourcePath, sourceFilePath), LogPath, LoggerMode.Update);
                }
            }
        }

        if (sourceFolderFiles.Length < _replicaFolderFiles.Count)
        {
            foreach (var sourceFilePath in _replicaFolderFiles.Keys)
            {
                if(!sourceFolderFiles.Contains(sourceFilePath))
                {
                    RemoveFileReplica(sourceFilePath);
                    
                    IOOperations.LogAction(Path.GetRelativePath(SourcePath, sourceFilePath), LogPath, LoggerMode.Remove);
                }
            }
        }
    }

    private void ReplicateEmptyFolders()
    {
        string[] sourceFolders = Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories)
            .OrderBy(str => str.Length)
            .ToArray();

        foreach (var folder in sourceFolders)
        {
            var relativeFolderPath = Path.GetRelativePath(SourcePath, folder);
            var replicaFolderPath = Path.Combine(ReplicaPath, Path.GetRelativePath(SourcePath, folder));
            
            if (!Directory.Exists(replicaFolderPath))
            {
                Directory.CreateDirectory(replicaFolderPath);
                IOOperations.LogAction(relativeFolderPath, LogPath, LoggerMode.Create);
            }
        }

        string[] replicaFolders = Directory.GetDirectories(ReplicaPath, "*", SearchOption.AllDirectories)
            .OrderBy(str => -str.Length)
            .ToArray();

        foreach (var folder in replicaFolders)
        {
            var relativePath = Path.GetRelativePath(ReplicaPath, folder);
            if(!sourceFolders.Contains(Path.Combine(SourcePath, relativePath)))
            {
                foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
                {
                    File.Delete(file);
                }
                
                Directory.Delete(folder);
                IOOperations.LogAction(relativePath, LogPath, LoggerMode.Remove);
            }
        }
    }

    private void CreateFileReplica(string sourceFilePath, string fileReplicaPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(fileReplicaPath)!);
        File.Copy(sourceFilePath, fileReplicaPath);
                
        _replicaFolderFiles.Add(sourceFilePath, ComputeHash(fileReplicaPath));
    }

    private void UpdateFileReplica(string sourceFilePath, string fileReplicaPath, string hash)
    {
        File.Copy(sourceFilePath, fileReplicaPath, overwrite: true);
        _replicaFolderFiles[sourceFilePath] = hash;
    }
    
    private void RemoveFileReplica(string sourceFilePath)
    {
        var fileReplicaPath = Path.Combine(ReplicaPath, Path.GetRelativePath(SourcePath, sourceFilePath));
        File.Delete(fileReplicaPath);

        _replicaFolderFiles.Remove(sourceFilePath);
    }

    private static string ComputeHash(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        using MD5 md5 = MD5.Create();
        var bytes = md5.ComputeHash(stream);
        
        return BitConverter.ToString(bytes);
    }
}