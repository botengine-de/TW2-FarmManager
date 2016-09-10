using System.Windows.Controls;

namespace FarmManager.UI
{
	public partial class BotStatistic : UserControl
	{
		public BotStatistic()
		{
			InitializeComponent();
		}

		public void Present(FarmManager.BotStatistic statistic)
		{
			StartTimeCalView.Text = statistic?.StartTimeCal.ToLongTimeString();
			BotStepCountView.Text = statistic?.BotStepCount.ToString();
			ReportSummaryReadCountView.Text = statistic?.ReportSummaryReadCount.ToString();
			ReportDetailReadCountView.Text = statistic?.ReportDetailReadCount.ToString();
			AttackSentCountView.Text = statistic?.AttackSentCount.ToString();
		}
	}
}
