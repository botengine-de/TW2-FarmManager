using BotEngine;
using FarmManager.TwStruct;
using Limbara;
using Limbara.Interface.InvocationProxy;
using Limbara.Interface.RemoteControl;
using Limbara.Script.ToScript;
using NUnit.Framework;
using System;
using System.Threading;

namespace FarmManager.Test
{
	public class TestContainer
	{
		[Test]
		public void Complete()
		{
			var dispatcher = new BotEngine.Interface.SimpleInterfaceServerDispatcher
			{
				LicenseClient = new BotEngine.Interface.LicenseClient
				{
					ServerAddress = Exe.____DevConfig.LicenseServerUrl,
					Timeout = 4444,
					Request = new BotEngine.Client.AuthRequest
					{
						LicenseKey = Exe.____DevConfig.LicenseKey,
						ServiceId = Exe.____DevConfig.ServiceId,
					}
				},
				SensorAppManager = new BotEngine.Interface.InterfaceAppManager(),
			};

			ProxyManager proxyManager = new ProxyManager();

			IApp app = null;

			while (null == app)
			{
				dispatcher.Exchange();

				var InterfaceRemoteControlProxy = proxyManager?.GetProxy(dispatcher?.SensorAppManager?.AppImplementationOfType<IHost>());

				app = InterfaceRemoteControlProxy?.AppObject;
			}

			var process =
				app.ReuseOrCreateProcess(Exe.____DevConfig.BrowserProcessConfig);

			var browserOpenCon = process?.Result?.ReuseOrOpenConnection();

			var browser = browserOpenCon?.Result;

			var document = new Func<IDocument>(() => browser?.Document?.Result);

			browser.LoadUrl(Exe.____DevConfig.TestTWUrl);

			var openUnitsElem = new Func<IHTMLElement>(() => document().GetElementFromXPath(Dom.openUnitsXPath));
			var openListReportElem = new Func<IHTMLElement>(() => document().GetElementFromXPath(Dom.openListReportXPath));

			openUnitsElem.WaitForNotNull(11111);

			Thread.Sleep(1111);

			UnitsControllerMovement[] listMovement = null;

			openUnitsElem().click();

			Thread.Sleep(1111);

			var UnitListControllerElem =
				document().GetElementFromXPath(Dom.ListUnitControllerXPath);

			//	JSON.stringify(angular.element($x("//div[@ng-controller='UnitListController']")[0]).scope().outgoingArmies)

			var unitsInfoEval =
				//	browser.JavascriptEvalToRefOrValue("JSON.stringify(angular.element($x(\"//div[@ng-controller='UnitListController']\")[0]).scope().outgoingArmies)");
				UnitListControllerElem.JavascriptCallFunction(Dom.JsGetListMovementOutgoingFromControllerElement);

			var listMovementSerial = unitsInfoEval?.Result?.value?.ToString();

			listMovement = listMovementSerial.DeserializeFromString<UnitsControllerMovement[]>();

			openListReportElem().click();

			Thread.Sleep(1111);

			var ListReportControllerElem =
				document().GetElementFromXPath(Dom.ListReportControllerXPath);

			var listReportSummaryEval =
				ListReportControllerElem.JavascriptCallFunction(Dom.JsGetListReportSummaryFromControllerElement);

			var listReportSummarySerial = listReportSummaryEval?.Result?.value?.ToString();

			var listReportSummary = listReportSummarySerial.DeserializeFromString<ReportListReportSummary[]>();
		}
	}
}
