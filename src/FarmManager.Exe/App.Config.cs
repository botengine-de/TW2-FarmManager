using Bib3;
using BotEngine.Common;

namespace FarmManager.Exe
{
	public partial class App
	{
		string LicenseKeyStoreFilePath => AssemblyDirectoryPath.PathToFilesysChild(@"license.key");

		ISingleValueStore<string> LicenseKeyStore;

		void ConfigSetup()
		{
			LicenseKeyStore =
				new SingleValueStoreCached<string>
				{
					BaseStore =
						new SingleValueStoreRelayWithExceptionToDelegate<string>
						{
							BaseStore = new StringStoreToFilePath
							{
								FilePath = LicenseKeyStoreFilePath,
							},

							ExceptionDelegate = e => LogEntryWriteNow(new LogEntry { LicenseKeyStoreException = e }),
						}
				};

			UI.ExeMain.LogEntryWrite = this.LogEntryWrite;
			UI.ExeMain.LicenseKeyStore = LicenseKeyStore;
			UI.ExeMain.BrowserServiceAppDomainSetupType = typeof(InterfaceAppDomainSetup);
		}
	}
}
