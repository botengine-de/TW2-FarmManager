using Bib3;
using BotEngine;
using BotEngine.Common;
using FarmManager.TwStruct;
using Limbara.Interface.RemoteControl;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FarmManager
{
	public class BotRoundReport
	{
	}

	public class BotStepReport
	{
		public DateTime? StartTimeCal;

		public bool BreakActiveNot;

		public int[] FarmEnableSetVillageId;

		public string BreakStartReason;

		public bool browserDocumentAvailable;

		public InCharacterDataVillageData VillageSelected;

		public BotStepMeasureListMovement MeasureListMovement;

		public BotStepMeasureListReportSummary MeasureListReport;

		public BotStepOpenReport OpenReport;

		public Exception Exception;

		public DateTime? BreakEndTimeCal;

		public bool AnyElementFromGetElementFromXPathSucceeded;

		public bool BrowserUsageFailed;

		public string RequestBrowserProcessStart;
	}

	public class BotStepMeasureListMovement
	{
	}

	public class BotStepMeasureListReportSummary
	{
	}

	public class BotStepAttack
	{
		public int? VillageSwitchTo;

		public KeyValuePair<string, Int64>[] AvailableUnits;

		public KeyValuePair<string, Int64>[] LackingUnits;

		public bool UnitCountSufficient;

		public bool Completed;
	}

	public class BotStepOpenReport
	{
		public ReportListReportSummary ReportSummary;

		public ReportHandlingPlanned ReportHandlingPlanned;

		public bool OpenListReport;

		public BotStepAttack AttackAgain;
	}

	public class BotStatistic
	{
		public DateTime StartTimeCal;

		public int BotStepCount;

		public int ReportReadCount;

		public int AttackSentCount;

		public int AttackSentVillageCount;
	}

	public class ReportHandlingPlanned
	{
		public bool Open;

		public bool AttackAgain;

		public int? OriginVillageId;

		public int? TargetVillageId;
	}

	public class Bot
	{
		const int BreakDurationMin = 60 * 3;

		const int BreakDurationMax = 60 * 44;

		const int BrowserRestartAge = 60 * 60;

		readonly object Lock = new object();

		public IApp App;

		public DateTime? BreakEndTimeCal;

		public PropertyGenTimespanInt64<string> GameEnterUrlMeasurementLast
		{
			private set;
			get;
		}

		readonly IDictionary<int, Bib3.PropertyGenTimespanInt64<UnitListControllerCommand[]>> MeasurementOutgoingUnitsLastFromVillageId =
			new Dictionary<int, Bib3.PropertyGenTimespanInt64<UnitListControllerCommand[]>>();

		public Bib3.PropertyGenTimespanInt64<ReportListReportSummary[]> MeasurementListReportLast
		{
			private set;
			get;
		}

		readonly public BotStatistic Statistic = new BotStatistic
		{
			StartTimeCal = DateTime.Now,
		};

		readonly IDictionary<Int64, PropertyGenTimespanInt64<ReportControllerReport>> ReportDetailFromId =
			new Dictionary<Int64, PropertyGenTimespanInt64<ReportControllerReport>>();

		readonly IDictionary<int, Int64> ArmySentLastTimeFromTargetVillageId = new Dictionary<int, Int64>();

		public PropertyGenTimespanInt64<BotStepReport> StepLastReport
		{
			private set;
			get;
		}

		public PropertyGenTimespanInt64<BotStepReport> StepBeforeBreakLastReport
		{
			private set;
			get;
		}

		const int ListStepBrowserUsageFailedRetainLength = 10;

		readonly Queue<PropertyGenTimespanInt64<bool>> ListStepBrowserUsageFailed = new Queue<PropertyGenTimespanInt64<bool>>();

		readonly IDictionary<int, Int64> UnitCountSufficientNotLastTimeFromVillageId = new Dictionary<int, Int64>();

		ReportHandlingPlanned ReportHandlingPlannedFromReportId(
			Int64 reportId,
			IEnumerable<int> farmEnableSetVillageId)
		{
			var reportDetail = ReportDetailFromId?.TryGetValueOrDefault(reportId);

			var reportOriginVillageId = reportDetail?.Value?.ReportAttack?.attVillageId;

			var reportTargetVillageId = reportDetail?.Value?.ReportAttack?.defVillageId;

			var armyOutgoing =
				MeasurementOutgoingUnitsLastFromVillageId?.Any(measurementOutgoingUnitsLast =>
				measurementOutgoingUnitsLast.Value?.Value?.Any(movement =>
				movement?.targetVillageId == (reportTargetVillageId ?? -1) &&
				!(movement?.returning ?? false)) ?? false) ?? false;

			var armySentAfterLastMeasurementListUnit =
				(MeasurementOutgoingUnitsLastFromVillageId?.Select(measurement => measurement.Value?.Begin)?.DefaultIfEmpty(0)?.Min() ?? 0) <
				ArmySentLastTimeFromTargetVillageId?.TryGetValueOrDefault(reportTargetVillageId ?? 0);

			var setReportForVillage =
				ReportDetailFromId?.Values
				?.Where(otherReport => otherReport?.Value?.ReportAttack?.defVillageId == reportTargetVillageId)
				?.ToArray();

			var isLastForVillage =
				!(setReportForVillage?.Any(otherReport => reportDetail?.Value?.time_created < otherReport?.Value?.time_created) ?? false);

			var attackAgain =
				reportOriginVillageId.HasValue &&
				(farmEnableSetVillageId?.Contains(reportOriginVillageId ?? -1) ?? false) &&
				reportDetail?.Value?.ResultEnum() == ReportResultEnum.NO_CASUALTIES &&
				!(0 < reportDetail?.Value?.ReportAttack?.defCharacterId) &&
				!armyOutgoing &&
				!armySentAfterLastMeasurementListUnit &&
				isLastForVillage;

			return new ReportHandlingPlanned
			{
				OriginVillageId = reportOriginVillageId,
				TargetVillageId = reportTargetVillageId,
				AttackAgain = attackAgain,
				Open = null == reportDetail || attackAgain,
			};
		}

		public BotStepReport Step(
			Int64 time,
			BrowserService browserService)
		{
			lock (Lock)
			{
				var report = new BotStepReport();

				var timeCal = report.StartTimeCal = DateTime.Now;

				var breakDurationRemaining = BreakEndTimeCal - DateTime.Now;

				try
				{
					if (0 < breakDurationRemaining?.TotalSeconds)
						return report;

					report.BreakActiveNot = true;

					var browser = browserService?.BrowserProcessCreatedLast?.Invoke()?.BrowserConnection;

					var browserDocument = browser?.Document?.Result;

					report.browserDocumentAvailable = null != browserDocument;

					if (null == browserDocument)
						throw new ArgumentNullException("browserDocument");

					{
						//	GetElementFromXPath(Dom.VillageDropDownControllerXPath) failed for some users without reason being discovered yet.
						//	Test whether GetElementFromXPath yields any element at all.

						var anyElementFromGetElementFromXPath = browserDocument?.GetElementFromXPath("//*");

						if (null == anyElementFromGetElementFromXPath)
							throw new ArgumentNullException("AnyElementFromGetElementFromXPath");
					}

					report.AnyElementFromGetElementFromXPathSucceeded = true;

					var villageDropDownControllerElem =
						browserDocument.GetElementFromXPath(Dom.VillageDropDownControllerXPath)?.Result;

					if (null == villageDropDownControllerElem)
						throw new ArgumentNullException("VillageDropDownControllerElem");

					var villageDropDownInfoEval =
						villageDropDownControllerElem?.JavascriptCallFunction(Dom.JsGetInfoFromVillageDropDownControllerElement);

					var villageDropDownInfoEvalError = villageDropDownInfoEval?.Error?.ToString();

					if (0 < villageDropDownInfoEvalError?.Length)
						throw new Exception("VillageDropDownInfoEvalError: " + villageDropDownInfoEvalError);

					var villageDropDownInfoSerial = villageDropDownInfoEval?.Result?.value?.ToString();

					if (null == villageDropDownInfoSerial)
						throw new ArgumentNullException("VillageDropDownInfoSerial");

					var villageDropDownInfo = villageDropDownInfoSerial?.FromVillageDropDownControllerInfoParse();

					report.VillageSelected = villageDropDownInfo?.selectedVillageData;

					var villageSelectedId = report.VillageSelected?.villageId;

					if (null == villageSelectedId)
						throw new ArgumentNullException("VillageSelectedId");

					var locationHref = browserDocument?.locationHref;

					if (locationHref.RegexMatchSuccessIgnoreCase(TwStruct.Static.GameEnterUrlIndicatorRegexPattern))
						GameEnterUrlMeasurementLast = new PropertyGenTimespanInt64<string>(locationHref, time);

					var farmEnableSetVillageId = report.FarmEnableSetVillageId =
						villageDropDownInfo?.villagesParsed?.Values()?.Select(village => village?.data?.villageId)?.WhereNotNullSelectValue()
						?.Where(villageId => !(time - UnitCountSufficientNotLastTimeFromVillageId?.TryGetValueNullable(villageId) < 1000 * 60 * 3))
						?.ToArray();

					var reportHandlingPlannedFromReportId = new Func<Int64, ReportHandlingPlanned>(reportId => this.ReportHandlingPlannedFromReportId(reportId, farmEnableSetVillageId));

					var listReportToOpen =
						(MeasurementListReportLast?.Value?.OrderByDescending(reportSummary => reportSummary?.time_created ?? 0))
						?.Where(reportSummary => reportHandlingPlannedFromReportId(reportSummary.id ?? -1)?.Open ?? false)
						?.ToArray();

					var measurementOutgoingUnitsLastTime =
						MeasurementOutgoingUnitsLastFromVillageId.TryGetValueOrDefault(villageSelectedId.Value)?.End;

					var measurementOutgoingUnitsLastAge = time - measurementOutgoingUnitsLastTime;

					var measureListReportDue = !(measurementOutgoingUnitsLastTime < MeasurementListReportLast?.Begin);

					if (!(measurementOutgoingUnitsLastAge < 1000 * 60) &&
						(!(measurementOutgoingUnitsLastAge < 1000 * 60 * 5) ||
						(!(0 < listReportToOpen?.Length) && !measureListReportDue)))
					{
						var unitListControllerElem =
							browserDocument.GetElementFromXPath(Dom.ListUnitControllerXPath)?.Result;

						var measureListMovement = report.MeasureListMovement = new BotStepMeasureListMovement();

						if (null != unitListControllerElem)
						{
							var unitListInfoEval =
								unitListControllerElem.JavascriptCallFunction(Dom.JsGetInfoFromUnitListControllerElement);

							var unitListInfoEvalSerial = unitListInfoEval?.Result?.value?.ToString();

							if (null == unitListInfoEvalSerial)
								throw new ArgumentNullException("UnitListInfoEvalSerial");

							var unitListInfo = unitListInfoEvalSerial.DeserializeFromString<UnitListControllerInfo>();

							var villageId = unitListInfo?.villageId;

							if (villageId.HasValue && null != unitListInfo?.outgoingArmies)
								MeasurementOutgoingUnitsLastFromVillageId[villageId.Value] =
									new Bib3.PropertyGenTimespanInt64<UnitListControllerCommand[]>(unitListInfo?.outgoingArmies, time);

							return report;
						}

						var openUnitsElem = browserDocument.GetElementFromXPath(Dom.openUnitsXPath)?.Result;

						if (null == openUnitsElem)
							throw new ArgumentNullException("openUnitsElem");

						openUnitsElem.click();

						return report;
					}

					var openListReportElem = browserDocument.GetElementFromXPath(Dom.openListReportXPath)?.Result;

					if (measureListReportDue)
					{
						var measureListReport = report.MeasureListReport = new BotStepMeasureListReportSummary();

						var listReportControllerElem =
							browserDocument.GetElementFromXPath(Dom.ListReportControllerXPath)?.Result;

						if (null != listReportControllerElem)
						{
							var paginationSetButtonSetLimit =
								browserDocument?.GetListElementFromXPath(Dom.ListReportControllerXPath + Dom.PaginationSetLimitButtonXPath)?.Result?.ToArray();

							var paginationButtonSetLimit =
								paginationSetButtonSetLimit?.OrderByDescending(button => button?.innerText?.RegexMatchIfSuccess(@"\d+")?.Value?.TryParseInt() ?? -1)?.FirstOrDefault();

							paginationButtonSetLimit?.click();
							Thread.Sleep(1444);

							var listReportSummaryEval =
								listReportControllerElem.JavascriptCallFunction(Dom.JsGetListReportSummaryFromControllerElement);

							var listReportSummarySerial = listReportSummaryEval?.Result?.value?.ToString();

							var listReportSummary =
								listReportSummarySerial.DeserializeFromString<ReportListReportSummary[]>()
								?.OrderBy(reportSummary => reportSummary?.time_created)
								?.ToArray();

							MeasurementListReportLast = new Bib3.PropertyGenTimespanInt64<ReportListReportSummary[]>(listReportSummary, time);

							return report;
						}

						openListReportElem.click();

						return report;
					}

					foreach (var reportSummary in listReportToOpen.EmptyIfNull())
					{
						var reportId = reportSummary.id;

						var reportHandlingPlanned = reportHandlingPlannedFromReportId(reportId ?? -1);

						var openReportReport = report.OpenReport = new BotStepOpenReport
						{
							ReportSummary = reportSummary,
							ReportHandlingPlanned = reportHandlingPlanned,
						};

						var listReportControllerElem =
							browserDocument.GetElementFromXPath(Dom.ListReportControllerXPath)?.Result;

						if (null == listReportControllerElem)
						{
							openReportReport.OpenListReport = true;

							openListReportElem?.click();

							Thread.Sleep(1111);

							listReportControllerElem =
								browserDocument.GetElementFromXPath(Dom.ListReportControllerXPath)?.Result;
						}

						if (null == listReportControllerElem)
							throw new ArgumentNullException("ListReportControllerElem");

						listReportControllerElem.JavascriptCallFunction(reportId.Value.JsFunctionShowReportWithIdOnReportsController());

						Thread.Sleep(1111);

						var reportControllerElem =
							browserDocument.GetElementFromXPath(Dom.ReportControllerXPath)?.Result;

						var reportDetailEval =
							reportControllerElem.JavascriptCallFunction(Dom.JsGetReportDetailFromControllerElement);

						var reportDetailSerial = reportDetailEval?.Result?.value?.ToString();

						var reportDetail =
							reportDetailSerial.DeserializeFromString<ReportControllerReport>();

						ReportDetailFromId[reportId.Value] = new PropertyGenTimespanInt64<ReportControllerReport>(reportDetail, time);

						openReportReport.ReportHandlingPlanned = reportHandlingPlanned =
							reportHandlingPlannedFromReportId(reportId.Value);

						var reportAttUnits =
							reportDetail?.ReportAttack?.attUnits?.SubsetPropertyInt()?.ToArray();

						if (!(0 < reportAttUnits?.Values()?.Sum()))
							return report;

						var targetVillageId = reportHandlingPlanned?.TargetVillageId;

						if (!(reportHandlingPlanned?.AttackAgain ?? false) || !targetVillageId.HasValue)
							return report;

						var attackAgainReport = openReportReport.AttackAgain = new BotStepAttack();

						var attVillageId = reportDetail?.ReportAttack?.attVillageId;

						if (!(report.VillageSelected?.villageId == attVillageId))
						{
							attackAgainReport.VillageSwitchTo = attVillageId;
							villageDropDownControllerElem.JavascriptCallFunction(Dom.JsFunctionVillageSelectOnVillageDropDownController(attVillageId.Value));
							return report;
						}

						reportControllerElem.JavascriptCallFunction(Dom.JsAttackAgainFromReportControllerElement);

						Thread.Sleep(1111);

						var modalCustomArmyControllerElem =
							browserDocument.GetElementFromXPath(Dom.ModalCustomArmyControllerXPath)?.Result;

						var availableUnitsSerial =
							modalCustomArmyControllerElem.JavascriptCallFunction(Dom.JsGetAvailableUnitsFromControllerElement)?.Result?.value?.ToString();

						var availableUnits =
							attackAgainReport.AvailableUnits = availableUnitsSerial.DeserializeFromString<JObject>().SubsetPropertyInt()?.ToArray();

						var lackingUnits = attackAgainReport.LackingUnits =
							reportAttUnits
							?.Select(attUnit => new KeyValuePair<string, Int64>(
								attUnit.Key, attUnit.Value - availableUnits?.ValueFromKeyOrDefault(attUnit.Key) ?? 0))
							?.Where(lacking => 0 < lacking.Value)
							?.OrderByDescending(lacking => lacking.Value)
							?.ToArray();

						if (0 < lackingUnits?.Values()?.Sum())
						{
							UnitCountSufficientNotLastTimeFromVillageId[villageSelectedId.Value] = time;
							return report;
						}

						attackAgainReport.UnitCountSufficient = true;

						var sendArmyButtonElement = browserDocument.GetElementFromXPath("//*[@ng-click=\"sendArmy('attack')\"]")?.Result;

						ArmySentLastTimeFromTargetVillageId[targetVillageId.Value] = time;
						Statistic.AttackSentCount++;

						sendArmyButtonElement?.click();

						attackAgainReport.Completed = true;

						return report;
					}

					report.BreakStartReason = "no more reports to open";
				}
				catch (Exception Exception)
				{
					report.Exception = Exception;
				}
				finally
				{
					if (report.BreakActiveNot)
					{
						report.BrowserUsageFailed = !report.AnyElementFromGetElementFromXPathSucceeded;
						ListStepBrowserUsageFailed.Enqueue(new PropertyGenTimespanInt64<bool>(report.BrowserUsageFailed, time));
						ListStepBrowserUsageFailed.ListeKürzeBegin(ListStepBrowserUsageFailedRetainLength);
					}

					var browserAge = DateTime.Now - browserService?.BrowserProcessCreatedLast?.Invoke()?.StartTimeCal;

					var requestBrowserProcessStartByAge =
						!(browserAge?.TotalSeconds < BrowserRestartAge) &&
						breakDurationRemaining?.TotalSeconds < 15;

					report.RequestBrowserProcessStart =
						ListStepBrowserUsageFailedRetainLength <= ListStepBrowserUsageFailed.Count &&
						ListStepBrowserUsageFailed.Count / 2 < ListStepBrowserUsageFailed.Count(failed => failed.Value) ?
						"Browser usage failed" : (requestBrowserProcessStartByAge ? ("browser age " + browserAge?.ToString()) : null);

					Statistic.ReportReadCount = ReportDetailFromId.Count;
					Statistic.AttackSentVillageCount = ArmySentLastTimeFromTargetVillageId.Count;

					if (0 < report.BreakStartReason?.Length)
					{
						MeasurementOutgoingUnitsLastFromVillageId.Clear();
						UnitCountSufficientNotLastTimeFromVillageId.Clear();

						report.BreakEndTimeCal = BreakEndTimeCal = DateTime.Now + TimeSpan.FromSeconds(
							new Random((int)Bib3.Glob.StopwatchZaitMiliSictInt()).Next().SictUmgebrocen(BreakDurationMin, BreakDurationMax));
					}

					var stepLastReportAtTime = new PropertyGenTimespanInt64<BotStepReport>(report, time);

					StepLastReport = stepLastReportAtTime;

					if (report.BreakActiveNot)
					{
						StepBeforeBreakLastReport = stepLastReportAtTime;
						Statistic.BotStepCount++;
					}
				}

				return report;
			}
		}
	}
}
