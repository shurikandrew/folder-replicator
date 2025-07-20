using System.Net;
using System.Security.Cryptography;

namespace FolderReplicator;

public class ReplicatorObject
{
    private string SourcePath { get; set; }
    private string ReplicaPath { get; set; }
    private Dictionary<string, string> ReplicaFolderFiles;
    private string LogPath { get; set; }
    public int Interval { get; set; }
    

    public ReplicatorObject(string sourcePath, string replicaPath, string logPath, int interval)
    {
        SourcePath = sourcePath;
        ReplicaPath = replicaPath;
        LogPath = logPath;
        Interval = interval;

        ReplicaFolderFiles = new();
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
        string[] sourceFolderFiles = Directory.GetFiles(SourcePath, "*", SearchOption.AllDirectories)
            .Where(f => !f.Contains(".DS_Store"))
            .ToArray();
        
        for (int i = 0; i < sourceFolderFiles.Length; i++)
        {
            var sourceFilePath = sourceFolderFiles[i];
            var fileReplicaPath = Path.Combine(ReplicaPath, Path.GetRelativePath(SourcePath, sourceFilePath));
            
            if (!ReplicaFolderFiles.ContainsKey(sourceFilePath))
            {
                CreateFileReplica(sourceFilePath, fileReplicaPath);

                var msg = $"Added file {Path.GetRelativePath(SourcePath, sourceFilePath)} At: {DateTime.Now}";
                Console.WriteLine(msg);
                
                using var writeStream = new StreamWriter(LogPath, append: true);
                writeStream.WriteLine(msg);
            }
            else
            {
                var hash = ComputeHash(sourceFilePath);

                if (hash != ReplicaFolderFiles[sourceFilePath])
                {
                    UpdateFileReplica(sourceFilePath, fileReplicaPath, hash);
                    
                    var msg = $"Updated file {Path.GetRelativePath(SourcePath, sourceFilePath)} At: {DateTime.Now}";
                    Console.WriteLine(msg);
                
                    using var writeStream = new StreamWriter(LogPath, append: true);
                    writeStream.WriteLine(msg);
                }
            }
        }

        if (sourceFolderFiles.Length < ReplicaFolderFiles.Count)
        {
            foreach (var sourceFilePath in ReplicaFolderFiles.Keys)
            {
                if(!sourceFolderFiles.Contains(sourceFilePath))
                {
                    RemoveFileReplica(sourceFilePath);
                    
                    var msg = $"Removed file {Path.GetRelativePath(SourcePath, sourceFilePath)} At: {DateTime.Now}";
                    Console.WriteLine(msg);
                
                    using var writeStream = new StreamWriter(LogPath, append: true);
                    writeStream.WriteLine(msg);
                }
            }
        }
    }

    private void CreateFileReplica(string sourceFilePath, string fileReplicaPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(fileReplicaPath)!);
        File.Copy(sourceFilePath, fileReplicaPath);
                
        ReplicaFolderFiles.Add(sourceFilePath, ComputeHash(fileReplicaPath));
    }

    private void UpdateFileReplica(string sourceFilePath, string fileReplicaPath, string hash)
    {
        File.Copy(sourceFilePath, fileReplicaPath, overwrite: true);
        ReplicaFolderFiles[sourceFilePath] = hash;
    }
    
    private void RemoveFileReplica(string sourceFilePath)
    {
        var fileReplicaPath = Path.Combine(ReplicaPath, Path.GetRelativePath(SourcePath, sourceFilePath));
        File.Delete(fileReplicaPath);

        ReplicaFolderFiles.Remove(sourceFilePath);
    }

    private static string ComputeHash(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        using MD5 md5 = MD5.Create();
        var bytes = md5.ComputeHash(stream);
        
        return BitConverter.ToString(bytes);
    }
}