using Microsoft.VisualBasic.FileIO;
using System.Text.Json;

namespace Node_API.Util;

public enum FileType
{
    ELECTION,ERROR,NORMAL
}

public class LogHandler
{
    private readonly string electionLogFile = "ElectionsLog.json";
    private readonly string normalLogFile = "Log.json";

    public void WriteLog((string Key, object Value, int Version) logEntry, FileType logType)
    {
        string filePath = GetLogType(logType);

        List<(string Key, object Value, int Version)> existingLogs = ReadLogs(logType);

        bool logExists = existingLogs.Any(x => x.Key == logEntry.Key);

        if (logExists)
        {
            int existingLogIndex = existingLogs.FindIndex(x => x.Key == logEntry.Key);
            if(existingLogIndex != -1)
            {
                if (existingLogs[existingLogIndex].Version >= logEntry.Version)
                {
                    logEntry.Version = existingLogs[existingLogIndex].Version + 1;
                }
                existingLogs[existingLogIndex] = logEntry;
            }
        } 
        else
        {
            existingLogs.Add(logEntry);
        }

        string updateJsonString = JsonSerializer.Serialize(existingLogs);
        File.WriteAllText(filePath, updateJsonString);
    }

    public List<(string Key, object Value, int Version)> ReadLogs(FileType logType)
    {
        string filePath = GetLogType(logType);

        List<(string Key, object Value, int Version)> logs = new();

        if (File.Exists(filePath))
        {
            string jsonString = File.ReadAllText(filePath);
            logs = JsonSerializer.Deserialize<List<(string Key, object Value, int Version)>>(jsonString);
        }

        return logs;
    }

    public bool LogExists((string Key, object Value, int Version) log, FileType logType)
    {
        List<(string Key, object Value, int Version)> existingLogs = ReadLogs(logType);
        return existingLogs.Contains(log);
    }

    public object GetLogItem(string key, FileType fileType)
    {
        List<(string Key, object Value, int Version)> logs = ReadLogs(fileType);

        var logItem = logs.FirstOrDefault(log => log.Key == key);

        return logItem.Value;
    }

    private string GetLogType(FileType logType)
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

    public (object Value, int Version) GetLogItemWithVersion(string key, FileType fileType)
    {
        List<(string Key, object Value, int Version)> logs = ReadLogs(fileType);

        var logItem = logs.FirstOrDefault(log => log.Key == key);

        if (logItem != default)
        {
            return (logItem.Value, logItem.Version);
        }
        else
        {
            return (null, 0);
        }
    }
}
