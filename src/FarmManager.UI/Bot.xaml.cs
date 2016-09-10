using System;
using System.Windows.Controls;

namespace FarmManager.UI
{
	public partial class Bot : UserControl
	{
		FarmManager.Bot Presented;

		public Bot()
		{
			InitializeComponent();
		}

		public void Present(FarmManager.Bot presented)
		{
			var nowTimeCal = DateTime.Now;

			Presented = presented;

			var breakDurationRemaining = presented?.BreakEndTimeCal - nowTimeCal;

			BreakPanel.Visibility = (0 < breakDurationRemaining?.TotalSeconds) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
			BreakDurationRemainingView.Text = breakDurationRemaining?.ToLongTimeString();

			StatisticsView.Text = presented?.Statistics.RenderForUI();

			StepLastView?.Present(presented?.StepBeforeBreakLastReport?.Value);

			GameEnterUrlView.Text = presented?.GameEnterUrlMeasurementLast?.Value;
		}

		private void BreakEndButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var presented = this.Presented;

			if (null != presented)
				presented.BreakEndTimeCal = null;
		}
	}
}
