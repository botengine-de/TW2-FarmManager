using Bib3;
using Bib3.FCL.UI;
using Bib3.RateLimit;
using Bib3.Synchronization;
using BotEngine;
using BotEngine.Common;
using BotEngine.UI;
using Limbara;
using Limbara.Interface;
using Limbara.Interface.RemoteControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace FarmManager.UI
{
	public partial class ExeMain : UserControl
	{
		readonly object TimerLock = new object();
		readonly object BotStepLock = new object();

		const string BrowserStartUrlDefault = @"https://tribalwars2.com/";
		const int BrowserStartAutomaticDistanceMin = 1000 * 60 * 3;

		string BrowserStartUrl
		{
			get
			{
				var botGameEnterUrl = Bot?.GameEnterUrlMeasurementLast?.Value;

				if (0 < botGameEnterUrl?.Length)
					return botGameEnterUrl;

				return BrowserStartUrlDefault;
			}
		}

		bool InputBrowserProcessStartAutomaticEnable = false;

		readonly FarmManager.Bot Bot = new FarmManager.Bot();

		DispatcherTimer Timer;

		int BotStepDistance = 1777;
		Int64 BotStepLastTime = 0;

		readonly RateLimitStateIntSingle BrowserConnectionMeasureLocationRateLimitState = new RateLimitStateIntSingle();
		readonly RateLimitStateIntSingle BotPresentRateLimitState = new RateLimitStateIntSingle();
		readonly RateLimitStateIntSingle LicenseServerExchangeRateLimitState = new RateLimitStateIntSingle();
		readonly RateLimitStateIntSingle BrowserProcessStartAutomaticRateLimitState = new RateLimitStateIntSingle();

		static public Type BrowserServiceAppDomainSetupType;

		static public Action<LogEntry> LogEntryWrite;

		static public ISingleValueStore<string> LicenseKeyStore;

		LogEntryAppInterface appInterfaceLogged;

		void EnsureAppInterfaceLogged()
		{
			var logEntryAppInterface = new LogEntryAppInterface
			{
				SessionId = InterfaceServerDispatcher?.LicenseClient?.ExchangeAuthLast?.Value?.Response?.SessionId,
				AppInterface = InterfaceServerDispatcher?.AppInterface,
			};

			if (!(logEntryAppInterface?.SessionId == appInterfaceLogged?.SessionId && logEntryAppInterface?.AppInterface == appInterfaceLogged?.AppInterface))
			{
				appInterfaceLogged = logEntryAppInterface;

				LogEntryWrite(new LogEntry
				{
					AppInterfaceChanged = logEntryAppInterface,
				});
			}
		}

		readonly Limbara.SimpleInterfaceServerDispatcher InterfaceServerDispatcher =
			new Limbara.SimpleInterfaceServerDispatcher
			{
				InterfaceAppDomainSetupType = BrowserServiceAppDomainSetupType,
				InterfaceAppDomainSetupTypeLoadFromMainModule = true,
			};

		IApp AppInterface => InterfaceServerDispatcher?.AppInterface;

		public PropertyGenTimespanInt64<BrowserProcessCreation> BrowserProcessCreationLast;

		readonly BrowserService BrowserService;

		public IEnumerable<IEnumerable<Key>> SetKeyBotMotionDisable => null;

		public bool BotMotionEnable
		{
			set
			{
				if (value)
					ToggleButtonMotionEnable?.RightButtonDown();
				else
					ToggleButtonMotionEnable?.LeftButtonDown();
			}

			get
			{
				return ToggleButtonMotionEnable?.ButtonReczIsChecked ?? false;
			}
		}

		public ExeMain()
		{
			InitializeComponent();

			BrowserControl.BrowserInterfaceControl.LicenseDataContext.LicenseKeyStore = LicenseKeyStore;

			ConfigFromModelToView(ExeConfig.Default);

			try
			{
				var config = Bib3.Glob.InhaltAusDataiMitPfaad(ConfigFilePath).DeserializeFromUtf8<ExeConfig>();

				ConfigAttemptWriteToFileLast = config;

				ConfigFromModelToView(config);
			}
			catch { }

			BrowserControl.BrowserProcessCreateAction = () =>
			{
				Task.Run(() =>
				{
					BrowserProcessCreate("UI command");
				});
			};

			BrowserService = new BrowserService
			{
				BrowserProcessCreatedLast = () => BrowserProcessCreationLast?.Value,
				BrowserProcessCreate = BrowserProcessCreate,
			};

			Timer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Normal, new EventHandler(TimerElapsed), Dispatcher);
		}

		BrowserProcessCreation BrowserProcessCreate(string reason)
		{
			var creation = new BrowserProcessCreation(AppInterface, Dispatcher.Invoke(BrowserProcessConfigCreate), BrowserStartUrl)
			{
				Reason = reason,
			};

			BrowserProcessCreationLast = new PropertyGenTimespanInt64<BrowserProcessCreation>(creation, Bib3.Glob.StopwatchZaitMiliSictInt());

			LogEntryWrite(new LogEntry
			{
				BrowserProcessCreated = creation,
			});

			return creation;
		}

		public Limbara.UI.Interface InterfaceControl => BrowserControl?.BrowserInterfaceControl;

		StatusIcon.StatusEnum InterfaceLicensePortionConnectionStatus =>
			InterfaceControl?.LicenseDataContext?.StatusIcon ?? StatusIcon.StatusEnum.None;

		StatusIcon.StatusEnum AppInterfaceStatus =>
			null == AppInterface ? StatusIcon.StatusEnum.Progress : StatusIcon.StatusEnum.Accept;

		StatusIcon.StatusEnum InterfaceLicenseStatus =>
			(BotEngine.UI.Extension.AggregateStatus(new[]
			{
				InterfaceLicensePortionConnectionStatus,
				AppInterfaceStatus,
			})
			?.FirstOrDefault()).GetValueOrDefault(StatusIcon.StatusEnum.Reject);

		const int RemoteControlAddressTcpDefaultBoundA = 3444;

		BrowserProcessConfig BrowserProcessConfigCreate() =>
			ExeConfig.BrowserProcessConfigDefaultCreate(
				ConfigFromViewToModel()?.BrowserProcess,
				Bib3.FCL.Glob.ZuProcessSelbsctMainModuleDirectoryPfaadBerecne().PathToFilesysChild("Browser.UserData"));

		static public string ConfigFilePath =>
			Bib3.FCL.Glob.ZuProcessSelbsctMainModuleDirectoryPfaadBerecne().PathToFilesysChild("config");

		public void ConfigFromModelToView(ExeConfig config)
		{
			InterfaceControl?.ProcessConfigViewModel?.PropagateFromClrMemberToDependencyProperty(config?.BrowserProcess);
		}

		public ExeConfig ConfigFromViewToModel() =>
			new ExeConfig
			{
				BrowserProcess = InterfaceControl?.ProcessConfigViewModel?.PropagateFromDependencyPropertyToClrMember(),
			};

		ExeConfig ConfigAttemptWriteToFileLast;

		void ConfigWriteToFileIfChanged()
		{
			var config = Dispatcher.Invoke(ConfigFromViewToModel);

			if (ConfigAttemptWriteToFileLast?.SerializeToString() == config?.SerializeToString())
				return;

			ConfigAttemptWriteToFileLast = config;

			Bib3.Extension.WriteToFileAndCreateDirectoryIfNotExisting(ConfigFilePath, config.SerializeToUtf8());
		}

		public void ProcessInput()
		{
			if (SetKeyBotMotionDisable?.Any(setKey => setKey?.All(key => Keyboard.IsKeyDown(key)) ?? false) ?? false)
				BotMotionEnable = false;

			try
			{
				InputBrowserProcessStartAutomaticEnable = BrowserControl?.BrowserProcessStartAutomaticEnable ?? false;

				ConfigWriteToFileIfChanged();
			}
			catch { }
		}

		void TimerElapsed(object sender, EventArgs e)
		{
			TimerLock.IfLockIsAvailableEnter(TimerElapsedLocked);
		}

		void TimerElapsedLocked()
		{
			ProcessInput();

			var licenseClientConfig = (ConfigFromViewToModel()?.LicenseClient).CompletedWithDefault().WithRequestLicenseKey(LicenseKeyStore?.Load() ?? ExeConfig.ConfigLicenseKeyDefault);

			Task.Run(() =>
			{
				var exchangeReport = InterfaceServerDispatcher?.Exchange(licenseClientConfig, null == AppInterface ? (int?)null : 1000);

				if (null != exchangeReport)
					LogEntryWrite(new LogEntry
					{
						InterfaceServerDispatcherExchange = exchangeReport,
					});
			});

			EnsureAppInterfaceLogged();

			PresentUI();

			var botStepLastAge = Bib3.Glob.StopwatchZaitMiliSictInt() - BotStepLastTime;

			if (!(botStepLastAge < BotStepDistance))
				BotStep();

			var browserProcessCreationLast = this.BrowserProcessCreationLast;

			Task.Run(() =>
			{
				if (null != browserProcessCreationLast?.Value &&
					BrowserConnectionMeasureLocationRateLimitState.AttemptPassStopwatchMilli(3000))
					browserProcessCreationLast.Value.DocumentLocationHrefMeasurementLast =
						FarmManager.Extension.InvokeTimespanFromStopwatchMilli(() =>
						browserProcessCreationLast?.Value?.BrowserConnectionOrError?.Result?.Document?.Result?.locationHref);
			});

			if (BotPresentRateLimitState.AttemptPassStopwatchMilli(1000))
				BotControl.Present(Bot);

			if (InputBrowserProcessStartAutomaticEnable && (browserProcessCreationLast?.Begin ?? 0) < BotStepLastTime)
			{
				var botRequestBrowserProcessStart = Bot?.StepLastReport?.Value?.RequestBrowserProcessStart;

				if (0 < botRequestBrowserProcessStart?.Length)
					if (BrowserProcessStartAutomaticRateLimitState.AttemptPassStopwatchMilli(BrowserStartAutomaticDistanceMin))
						BrowserProcessCreate("Bot: " + botRequestBrowserProcessStart);
			}
		}

		StatusIcon.StatusEnum BrowserStatusIconType =>
			BrowserControl?.BrowserProcessCurrentStatusIconView?.Status == StatusIcon.StatusEnum.Accept ? StatusIcon.StatusEnum.Accept : StatusIcon.StatusEnum.Reject;

		void PresentUI()
		{
			BrowserControl?.Present(BrowserProcessCreationLast?.Value);

			InterfaceControl?.LicenseHeader?.SetStatus(InterfaceLicenseStatus);
			BrowserStatusIcon.Status = BrowserStatusIconType;

			InterfaceControl?.Present(InterfaceServerDispatcher);
		}

		public void BotStep()
		{
			var botMotionEnable = this.BotMotionEnable;

			if (!botMotionEnable)
				return;

			BotStepLastTime = Bib3.Glob.StopwatchZaitMiliSictInt();

			Task.Run(() =>
			{
				BotStepLock.IfLockIsAvailableEnter(() =>
				{
					Bot.Step(Bib3.Glob.StopwatchZaitMiliSictInt(), BrowserService);

					LogEntryWrite(new LogEntry
					{
						BotStepCompleted = Bot.StepLastReport?.Value,
					});

					BotStepLastTime = Bib3.Glob.StopwatchZaitMiliSictInt();

					Dispatcher.Invoke(() =>
					{
						BotControl.Present(Bot);
					});
				});
			});
		}
	}
}
