using Limbara;
using System;

namespace FarmManager
{
	public class BrowserService
	{
		public Func<SimpleInterfaceServerDispatcher> InterfaceCreate;

		public Func<SimpleInterfaceServerDispatcher> InterfaceCreatedLast;

		public Func<string, BrowserProcessCreation> BrowserProcessCreate;

		public Func<BrowserProcessCreation> BrowserProcessCreatedLast;
	}
}
