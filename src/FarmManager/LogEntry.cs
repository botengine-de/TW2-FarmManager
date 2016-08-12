using BotEngine;
using System;
using System.IO;
using System.Linq;

namespace FarmManager
{
	public class LogEntryAppInterface
	{
		public Limbara.Interface.RemoteControl.IApp AppInterface;

		public string SessionId;
	}

	public class LogEntry
	{
		public DateTime EntryTime;

		public LogEntryAppInterface AppInterfaceChanged;

		public BrowserProcessCreation BrowserProcessCreated;

		public BotStepReport BotStepCompleted;

		public Exception LicenseKeyStoreException;

		public BotEngine.Interface.SimpleInterfaceServerDispatcher.ExchangeReport InterfaceServerDispatcherExchange;
	}

	static public class Log
	{
		static public void Write(this Stream destination, LogEntry entry)
		{
			var entrySerial = entry?.SerializeToUtf8();

			if (null == entrySerial)
				return;

			var entrySerialAndDelimiter = entrySerial.Concat(new byte[4]).ToArray();

			destination?.Write(entrySerialAndDelimiter, 0, entrySerialAndDelimiter.Length);
		}
	}
}
