using Bib3;
using System;
using System.Text;
using System.Windows;

namespace FarmManager.Exe
{
	public partial class App : Application
	{
		static string AssemblyDirectoryPath => Bib3.FCL.Glob.ZuProcessSelbsctMainModuleDirectoryPfaadBerecne().EnsureEndsWith(@"\");

		private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			try
			{
				var filePath = AssemblyDirectoryPath.PathToFilesysChild(DateTime.Now.SictwaiseKalenderString(".", 0) + " Exception");

				filePath.WriteToFileAndCreateDirectoryIfNotExisting(Encoding.UTF8.GetBytes(e.Exception.SictString()));

				var message = "exception written to file: " + filePath;

				MessageBox.Show(message, message, MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			catch (Exception PersistException)
			{
				Bib3.FCL.GBS.Extension.MessageBoxException(PersistException);
			}

			Bib3.FCL.GBS.Extension.MessageBoxException(e.Exception);
		}

		public App()
		{
			LogCreate();

			ConfigSetup();
		}
	}

	/// <summary>
	/// This Type must reside in an Assembly that can be resolved by the default assembly resolver.
	/// </summary>
	public class InterfaceAppDomainSetup
	{
		static InterfaceAppDomainSetup()
		{
			BotEngine.Interface.InterfaceAppDomainSetup.Setup();
		}
	}
}
