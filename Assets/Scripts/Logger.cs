using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Logger{
    public enum LoggerEvent {OpenApp, HasAppOpenAndInForeground, OpenScene, Interaction, RightAnswer, WrongAnswer, ReadInstructions};
    static Dictionary<string, Queue<LogEntry>> LogEntriesToSync = new Dictionary<string, Queue<LogEntry>>();
    static bool SendLogToServer = false;
    static bool AllSynced = true;

    public static void InitializeLogger(bool sendLogToServer){
        SendLogToServer = sendLogToServer;
        WriteLog("Timestamp;UserId;Event;Addition Information\n");
        LogEvent(LoggerEvent.OpenApp);
    }

    public static void LogEvent(LoggerEvent loggerEvent, string additionalInformation = "") {
        LogEntry entry = new LogEntry();
        entry.Timestamp = ((int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
        entry.UserID = FixedValues.GetUsername();
        entry.Event = loggerEvent.ToString();
        entry.EventDescription = additionalInformation;
        AddLogEntry(entry);
    }

    public static void WriteLog(string newLine){
        string filepath = Application.persistentDataPath + Path.DirectorySeparatorChar + FixedValues.GetUsername() + "-log.txt";
        StreamWriter streamWriter;
        if(!File.Exists(filepath)){
            streamWriter = File.CreateText(filepath);
        } else {
            streamWriter = File.AppendText(filepath);
        }
        streamWriter.WriteLine(newLine);
        streamWriter.Close();
    }

    public static string LogEntryToString(LogEntry logEntry){
        return logEntry.Timestamp + ";" + logEntry.UserID + ";" + logEntry.Event + ";" + logEntry.EventDescription;
    }

    public static void AddLogEntry(LogEntry logEntry){
        WriteLog(LogEntryToString(logEntry));
        if(SendLogToServer){
            AllSynced = false;
            Queue<LogEntry> logEntryQueue;
            if(LogEntriesToSync.TryGetValue(logEntry.UserID, out logEntryQueue)){
                logEntryQueue.Enqueue(logEntry);
            } else {
                logEntryQueue = new Queue<LogEntry>();
                logEntryQueue.Enqueue(logEntry);
                LogEntriesToSync.Add(logEntry.UserID, logEntryQueue);
            }
        }
    }

    public static bool IsAllSynced(){
        return AllSynced;
    }

    public static void AllIsSynced(){
        AllSynced = true;
    }

    public static void LogEntryWasSynced(){
        Queue<LogEntry> logEntries;
        LogEntriesToSync.TryGetValue(FixedValues.GetUsername(), out logEntries);
        logEntries.Dequeue();
    }

    public static bool GetLogEntryToSync(out LogEntry logEntry){
        Queue<LogEntry> logEntries;
        LogEntriesToSync.TryGetValue(FixedValues.GetUsername(), out logEntries);
        if(logEntries != null && logEntries.Count > 0){
            logEntry = logEntries.Peek();
            return true;
        }
        logEntry = new LogEntry();
        return false;
    }
}