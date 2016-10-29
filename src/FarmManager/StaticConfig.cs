using Bib3;
using BotEngine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FarmManager
{
	static public class StaticConfig
	{
		static public string GameEnterUrlFromCommandlineArgs(this IEnumerable<string> args) =>
			args?.Select(arg => arg.RegexMatchIfSuccess(@"gameenterurl\=([^\s]*)(\s|$)", RegexOptions.IgnoreCase)?.Groups[1]?.Value)
			?.WhereNotDefault()
			?.FirstOrDefault();

		static public string GameEnterUrlFromCommandlineArgs() =>
			Environment.GetCommandLineArgs()?.GameEnterUrlFromCommandlineArgs();
	}
}
