using System;
using System.IO;
using UnityEngine;

public static class TryDicingLogger
{
    private static readonly object FileLock = new object();
    private static bool _fileLoggingUnavailable;
    private static string _logFilePath;

    public static void Info(string message)
    {
        Debug.Log(Prefix(message));
        WriteFile("INFO", message);
    }

    public static void Error(string message)
    {
        Debug.LogError(Prefix(message));
        WriteFile("ERROR", message);
    }

    public static string LogFilePath
    {
        get { return GetLogFilePath(); }
    }

    private static string Prefix(string message)
    {
        return "[tryDicing] " + message;
    }

    private static void WriteFile(string level, string message)
    {
        if (_fileLoggingUnavailable)
        {
            return;
        }

        try
        {
            string path = GetLogFilePath();
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string line = string.Format(
                "{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}{3}",
                DateTime.Now,
                level,
                message,
                Environment.NewLine);

            lock (FileLock)
            {
                File.AppendAllText(path, line);
            }
        }
        catch (Exception ex)
        {
            _fileLoggingUnavailable = true;
            Debug.LogError("[tryDicing] File logging disabled: " + ex.Message);
        }
    }

    private static string GetLogFilePath()
    {
        if (!string.IsNullOrEmpty(_logFilePath))
        {
            return _logFilePath;
        }

        string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _logFilePath = Path.Combine(documents, "library_of_ruina_mod開發", "logs", "tryDicing.log");
        return _logFilePath;
    }
}
