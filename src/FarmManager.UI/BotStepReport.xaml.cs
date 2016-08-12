using BotEngine;
using System.Windows.Controls;

namespace FarmManager.UI
{
	public partial class BotStepReport : UserControl
	{
		public BotStepReport()
		{
			InitializeComponent();
		}

		public void Present(FarmManager.BotStepReport presented)
		{
			SummaryView.Text = presented?.SummaryText();
			DetailView.Text = presented?.SerializeToString(Newtonsoft.Json.Formatting.Indented);
		}
	}
}
