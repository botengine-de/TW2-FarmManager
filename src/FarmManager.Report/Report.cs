using Bib3;
using BotEngine;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FarmManager.Report
{
	public class ReportContainer
	{
		const string LogFilePath = @"";

		static public IEnumerable<byte[]> ReadSequenceEntry(Stream stream)
		{
			var entryListByte = new List<byte>();

			for (;;)
			{
				var next = stream.ReadByte();

				if (0 == next)
				{
					if (0 < entryListByte.Count)
					{
						yield return entryListByte.ToArray();
						entryListByte.Clear();
					}

					continue;
				}

				entryListByte.Add((byte)next);

				if (next < 0)
					yield break;
			}
		}

		[Test]
		[Explicit("used for reporting.")]
		public void Report()
		{
			using (var fileStream = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read))
			{
				var sequenceEntry =
					ReadSequenceEntry(fileStream)
					?.Select(entry => entry?.DeserializeFromUtf8<JObject>())
					?.ToArray();

				var sequenceBotStep =
					sequenceEntry
					?.Select(entry =>
					{
						var botStepCompletedJson = entry?.Property(nameof(LogEntry.BotStepCompleted))?.Value;

						if (null == botStepCompletedJson)
							return null;

						return new
						{
							BotStepCompleted = botStepCompletedJson.ToString()?.DeserializeFromString<BotStepReport>(),
						};
					})
					?.WhereNotDefault()
					?.ToArray();

				var botStepCount = sequenceBotStep?.Count(entry => entry?.BotStepCompleted?.BreakActiveNot ?? false);
				var botStepExceptionCount = sequenceBotStep?.Count(entry => null != entry?.BotStepCompleted?.Exception);
				var botStepExceptionCountPercentage = (botStepExceptionCount * 100) / Math.Max(1, botStepCount ?? 0);

				Console.WriteLine("botStepCount: " + botStepCount);
				Console.WriteLine("botStepExceptionCount: " + botStepExceptionCount + " (" + botStepExceptionCountPercentage + "%)");

				var entryException =
					sequenceBotStep?.FirstOrDefault(entry => null != entry?.BotStepCompleted?.Exception);
			}
		}
	}
}
