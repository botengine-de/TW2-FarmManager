using Bib3;
using System;
using System.IO;

namespace FarmManager.Exe
{
	partial class App
	{
		Stream logStream;

		Exception logCreateException;

		Exception logEntryWriteException;

		void LogEntryWrite(LogEntry entry)
		{
			try
			{
				lock (logStream)
				{
					logStream?.Write(entry);
					logStream?.Flush();
				}
			}
			catch (Exception e)
			{
				logEntryWriteException = e;
			}
		}

		void LogEntryWriteNow(LogEntry entry)
		{
			entry.EntryTime = DateTime.Now;

			LogEntryWrite(entry);
		}

		void LogCreate()
		{
			try
			{
				var logFileName = Bib3.Glob.SictwaiseKalenderString(DateTime.Now, ".", 0) + ".FarmManager.log";

				var logFilePath = Bib3.FCL.Glob.ZuProcessSelbsctMainModuleDirectoryPfaadBerecne().PathToFilesysChild("log").PathToFilesysChild(logFileName);

				var directory = new FileInfo(logFilePath).Directory;

				if (!directory.Exists)
					directory.Create();

				logStream = new FileStream(logFilePath, FileMode.CreateNew, FileAccess.Write);
			}
			catch (Exception e)
			{
				logCreateException = e;
			}
		}
	}
}
