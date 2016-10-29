using NUnit.Framework;

namespace FarmManager.Test
{
	public class StaticConfig
	{
		[Test]
		public void GameEnterUrl_From_Commandline()
		{
			Assert.AreEqual(
				"http://test.com",
				FarmManager.StaticConfig.GameEnterUrlFromCommandlineArgs(new[] { null, "otherarg", "gamEenterUrL=http://test.com" }));
		}
	}
}
