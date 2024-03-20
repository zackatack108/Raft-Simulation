using System.Security.AccessControl;
using System.Text.Json;

namespace Node_API.Util;

public enum FileType
{
    ELECTION,ERROR,NORMAL
}

public class RaftLogEntry
{
    public int Term { get; set; }
    public string NodeID { get; set; }
    public object Command { get; set; }
}

public class LogHandler
{
    private readonly string baseDirectory = Path.Combine(AppContext.BaseDirectory, "RaftLogs");
    private readonly string electionLogFile = "ElectionsLog.json";
    private readonly string normalLogFile = "Log.json";
    private readonly ILogger<LogHandler> logger;

    public LogHandler(ILogger<LogHandler> logger)
    {
        this.logger = logger;
    }

    public void Initialize()
    {
        try
        {

            if (!Directory.Exists(baseDirectory))
            {
                // Create the directory if it doesn't exist
                Directory.CreateDirectory(baseDirectory);
                logger.LogInformation($"Directory '{baseDirectory}' created successfully.");
            }
            else
            {
                logger.LogInformation($"Directory '{baseDirectory}' already exists.");
            }

            // Create or verify existence of normal log file
            string normalLogFilePath = Path.Combine(baseDirectory, normalLogFile);
            InitializeLogFile(normalLogFilePath);

            // Create or verify existence of election log file
            string electionLogFilePath = Path.Combine(baseDirectory, electionLogFile);
            InitializeLogFile(electionLogFilePath);

            logger.LogInformation("Initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError($"Initialization failed: {ex.Message}");
        }
    }

    private void InitializeLogFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
            logger.LogInformation($"File '{filePath}' created successfully.");
        }
        else
        {
            logger.LogInformation($"File '{filePath}' already exists.");
        }
    }

    public void AppendLogEntry(int term, string nodeID, object command, FileType fileType)
    {
        try
        {
            string file = GetFileType(fileType);

            var logEntry = new RaftLogEntry { Term = term, NodeID = nodeID, Command = command };

            List<RaftLogEntry> existingLogs = ReadLogs(fileType);

            existingLogs.Add(logEntry);

            string updatedJsonString = JsonSerializer.Serialize(existingLogs);

            using (var fileStream = new FileStream(Path.Combine(baseDirectory, file), FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var streamWriter = new StreamWriter(fileStream))
            {
                streamWriter.Write(updatedJsonString);
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to append new entry: {ex.Message}");
        }        
    }

    public List<RaftLogEntry> ReadLogs(FileType fileType)
    {
        try
        {
            List<RaftLogEntry> logs = new();
            string file = GetFileType(fileType);

            if (File.Exists(Path.Combine(baseDirectory,file)))
            {
                string jsonString = File.ReadAllText(Path.Combine(baseDirectory, file));
                logs = JsonSerializer.Deserialize<List<RaftLogEntry>>(jsonString);
            }

            return logs;
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to read logs: {ex.Message}");
            return new();
        }
    }

    public RaftLogEntry GetLogEntry(string nodeID, FileType fileType, int? term)
    {
        List<RaftLogEntry> logs = ReadLogs(fileType);

        if(term != null)
        {
            return logs.FirstOrDefault(log => log.Term == term && log.NodeID == nodeID);
        }
        return logs.FirstOrDefault(log => log.NodeID == nodeID);

    }

    public void TruncateLogs(int term, string nodeID, FileType fileType)
    {
        List<RaftLogEntry> logs = ReadLogs(fileType);
        string file = GetFileType(fileType);

        logs.RemoveAll(entry => entry.Term >= term && entry.NodeID == nodeID);

        string updateJsonString = JsonSerializer.Serialize(logs);

        File.WriteAllText(Path.Combine(baseDirectory, file), updateJsonString);
    }

    public int GetLastLogIndex(FileType fileType)
    {
        List<RaftLogEntry> logs = ReadLogs(fileType);
        return logs.Count > 0 ? logs.Count - 1 : 0;
    }

    public int GetLastLogTerm(FileType fileType)
    {
        List<RaftLogEntry> logs = ReadLogs(fileType);
        return logs.Count > 0 ? logs[^1].Term : 0;
    }

    private string GetFileType(FileType logType)
    {

        switch (logType)
        {
            case FileType.ELECTION:
                return electionLogFile;
            case FileType.NORMAL:
                return normalLogFile;
        }
        return "";
    }
}
