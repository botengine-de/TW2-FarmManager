using Bib3;
using Bib3.FCL.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmManager.UI
{
	static public class Extension
	{
		static public StatusIcon.StatusEnum StatusIcon(this BrowserProcessCreation browserProcessCreation) =>
			null == browserProcessCreation?.BrowserConnection ? Bib3.FCL.UI.StatusIcon.StatusEnum.Reject : Bib3.FCL.UI.StatusIcon.StatusEnum.Accept;

		static public string StatusText(
			this BrowserProcessCreation browserProcessCreation,
			out StatusIcon.StatusEnum statusEnum)
		{
			statusEnum = Bib3.FCL.UI.StatusIcon.StatusEnum.Reject;

			try
			{
				if (null == browserProcessCreation)
					return null;

				var exception = browserProcessCreation.Exception;

				var textLine = new List<string>();

				textLine.Add("created at " + browserProcessCreation.StartTimeCal.ToLongTimeString() +
					" (PID: " + browserProcessCreation?.ProcessIdFromOS() + ", reason: " + browserProcessCreation?.Reason + ")");

				var browserConnection = browserProcessCreation.BrowserConnection;

				var documentLocationHrefMeasurementLast = browserProcessCreation?.DocumentLocationHrefMeasurementLast;

				var documentLocationHref = documentLocationHrefMeasurementLast?.Value;

				if (null != browserConnection)
				{
					if (0 < documentLocationHref?.Length)
					{
						statusEnum = Bib3.FCL.UI.StatusIcon.StatusEnum.Accept;

						textLine.Add("connected to browser.");
						textLine.Add("location (" + documentLocationHrefMeasurementLast?.Duration() + "ms): " + documentLocationHref);
					}
					else
					{
						textLine.Add("connection to browser lost.");
					}
				}

				textLine.AddRange(
					new[]
					{
						browserProcessCreation?.BrowserProcessErrorMessage,
						browserProcessCreation?.BrowserConnectionErrorMessage,
					}.WhereNotDefault()
					.Select(errorMessage => "error: " + errorMessage));

				if (null != exception)
					textLine.Add("Exception: " + Bib3.Glob.SictString(exception));

				return string.Join(Environment.NewLine, textLine);
			}
			catch (Exception e)
			{
				return e.SictString();
			}
		}

		static public string ToLongTimeString(this TimeSpan timeSpan) =>
			0 < timeSpan.Ticks ? (DateTime.MinValue + timeSpan).ToLongTimeString() :
			(-timeSpan).ToLongTimeString();

		static public string UIIdentifierText(this TwStruct.InCharacterDataVillageData village) =>
			null == village ? null :
			village.x + "|" + village.y +
			" [" + village.villageId + "] '" + village.name + "'";

		static public string SummaryText(this FarmManager.BotStepReport botStepReport)
		{
			if (null == botStepReport)
				return null;

			var breakStartReason = botStepReport?.BreakStartReason;
			var openReport = botStepReport?.OpenReport;
			var exception = botStepReport?.Exception;

			var listLine = new List<string>();

			listLine.Add("started at " + botStepReport?.StartTimeCal?.ToLongTimeString());

			listLine.Add("current village: " + botStepReport?.VillageSelected?.UIIdentifierText());

			if (null != botStepReport?.MeasureListMovement)
				listLine.Add("read unit movements.");

			if (null != botStepReport?.MeasureListReport)
				listLine.Add("read report list.");

			if (null != openReport)
			{
				listLine.Add("open report with id " + openReport?.ReportSummary?.id);

				var attackAgain = openReport?.AttackAgain;

				if (null != attackAgain)
					listLine.Add("-> attack again (completed = " + attackAgain.Completed + ")");
			}

			if (null != breakStartReason)
				listLine.Add("start break because: " + breakStartReason);

			if (null != exception)
				listLine.Add(exception?.GetType()?.FullName + ": " + exception?.Message);

			return string.Join(Environment.NewLine, listLine);
		}

		static public string WindowTitle(this BrowserProcessCreation browserProcessCreation)
		{
			var location = browserProcessCreation?.DocumentLocationHrefMeasurementLast?.Value;

			return browserProcessCreation?.BrowserAddressTcp?.ToString() + " - " + (location?.TW2WorldIdFromUrl() ?? location);
		}

		static public string RenderForUI(this BotStatistics statistics) =>
			null == statistics ? null :
			string.Join(Environment.NewLine,
				new[]
				{
					"started at: " + statistics?.StartTimeCal.ToLongTimeString(),
					"bot step count: " + statistics?.BotStepCount,
					"---- reports read ----",
					"report.summary: " + statistics?.ReportSummaryReadCount +
					", report.detail: " + statistics?.ReportDetailReadCount,
					"attacks sent: " + statistics?.AttackSentCount,
				});
	}
}
