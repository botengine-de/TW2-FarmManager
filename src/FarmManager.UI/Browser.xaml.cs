using Bib3.FCL.UI;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FarmManager.UI
{
	public partial class Browser : UserControl
	{
		public Action BrowserProcessCreateAction;

		public Browser()
		{
			InitializeComponent();
		}

		public bool BrowserProcessStartAutomaticEnable => BrowserProcessStartAutomaticEnableCheckBox?.IsChecked ?? false;

		private void BrowserProcessCreateButton_Click(object sender, RoutedEventArgs e)
		{
			BrowserProcessCreateAction?.Invoke();

			BrowserProcessStartAutomaticEnableCheckBox.IsChecked = true;
		}

		public void Present(BrowserProcessCreation browserProcessCreation)
		{
			StatusIcon.StatusEnum browserProcessStatusEnum;

			var browserProcessStatusText = browserProcessCreation.StatusText(out browserProcessStatusEnum);

			BrowserProcessCurrentStatusTextView.Text = browserProcessStatusText;
			BrowserProcessCurrentStatusIconView.Status = browserProcessStatusEnum;
		}
	}
}
