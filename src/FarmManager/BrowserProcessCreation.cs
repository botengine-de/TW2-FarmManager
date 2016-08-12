using Bib3;
using Limbara.Interface;
using Limbara.Interface.RemoteControl;
using Limbara.Script.ToScript;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace FarmManager
{
	public class BrowserProcessCreation
	{
		public string Reason;

		public Exception Exception;

		[JsonIgnore]
		public IResultOrError<IBrowserProcess> BrowserProcessOrError;

		[JsonIgnore]
		public IBrowserProcess BrowserProcess;

		public string BrowserProcessErrorMessage;

		[JsonIgnore]
		public IResultOrError<IBrowserConnection> BrowserConnectionOrError;

		[JsonIgnore]
		public IBrowserConnection BrowserConnection;

		public string BrowserConnectionErrorMessage;

		public DateTime StartTimeCal = DateTime.Now;

		public PropertyGenTimespanInt64<string> DocumentLocationHrefMeasurementLast;

		public int? BrowserAddressTcp
		{
			private set;
			get;
		}

		public BrowserProcessCreation(
			IApp browserServiceInterface,
			BrowserProcessConfig browserProcessConfig,
			string url = null)
		{
			try
			{
				if (null == browserServiceInterface)
					throw new NullReferenceException("Browser service interface not available.");

				BrowserProcessOrError =
					browserServiceInterface.ReuseOrCreateProcess(browserProcessConfig);

				Thread.Sleep(1444);

				BrowserProcessErrorMessage = BrowserProcessOrError?.Error?.Message;

				BrowserProcess = BrowserProcessOrError?.Result;

				BrowserConnectionOrError = BrowserProcess?.ReuseOrOpenConnection();

				BrowserConnectionErrorMessage = BrowserConnectionOrError?.Error?.Message;

				BrowserConnection = BrowserConnectionOrError?.Result;

				if (null != BrowserConnection && 0 < url?.Length)
				{
					Thread.Sleep(444);
					BrowserConnection.Document.Result.locationHref = url;
				}

				BrowserAddressTcp = BrowserConnection?.BrowserAddressTcp;
			}
			catch (Exception Exception)
			{
				this.Exception = Exception;
			}
		}
	}
}
