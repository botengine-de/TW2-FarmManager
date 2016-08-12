using FarmManager.UI;
using System;
using System.Windows;
using System.Windows.Threading;

namespace FarmManager.Exe
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public string TitleComputed =>
			"Farm Manager v" + (TryFindResource("AppVersionId") ?? "") + " - " + MainControl?.BrowserProcessCreationLast?.Value?.WindowTitle();

		DispatcherTimer Timer;

		public MainWindow()
		{
			InitializeComponent();

			Timer = new DispatcherTimer(TimeSpan.FromMilliseconds(1000), DispatcherPriority.Normal, new EventHandler(TimerElapsed), Dispatcher);
		}

		void TimerElapsed(object sender, EventArgs e)
		{
			this.Title = TitleComputed;
		}
	}
}
