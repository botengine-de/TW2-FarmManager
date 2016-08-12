using Bib3;
using BotEngine.Common;
using Limbara.Interface.RemoteControl;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FarmManager
{
	static public class Extension
	{
		static public bool LoadUrl(
			this IBrowserConnection browser,
			string url,
			int timeout = 10000)
		{
			if (null == browser)
				return true;

			browser.Document.Result.locationHref = url;

			var waitStartTime = Bib3.Glob.StopwatchZaitMiliSictInt();

			Thread.Sleep(1111);

			while (true)
			{
				var doc = browser.Document.Result;

				if (doc?.readyState == "complete" && doc.locationHref == url)
					return false;

				var waitDuration = Bib3.Glob.StopwatchZaitMiliSictInt() - waitStartTime;

				if (timeout < waitDuration)
					return true;

				Thread.Sleep(111);
			}
		}

		static public bool WaitForNotNull<T>(
			this Func<T> callback,
			int timeout)
			where T : class
		{
			var waitStartTime = Bib3.Glob.StopwatchZaitMiliSictInt();

			while (null == callback())
			{
				var waitDuration = Bib3.Glob.StopwatchZaitMiliSictInt() - waitStartTime;

				if (timeout < waitDuration)
					return true;

				Thread.Sleep(111);
			}

			return false;
		}

		static public IEnumerable<string> StatusRenderToListString(this Bot bot)
		{
			throw new NotImplementedException();
		}

		static public Int64? ValueAsIntIfInteger(this JToken token) =>
			token?.Type == JTokenType.Integer ? ((token as JValue)?.Value?.ToString()?.TryParseInt64()) : null;

		static public IEnumerable<KeyValuePair<string, Int64>> SubsetPropertyInt(this JObject jObject) =>
			jObject?.Properties()
			?.Select(property => new KeyValuePair<string, Int64?>(property.Name, property.Value.ValueAsIntIfInteger()))
			?.Where(property => property.Value.HasValue)
			?.Select(property => new KeyValuePair<string, Int64>(property.Key, property.Value.Value));

		static public ValueT ValueFromKeyOrDefault<KeyT, ValueT>(
			this IEnumerable<KeyValuePair<KeyT, ValueT>> source, KeyT key) =>
			source.FirstOrDefault(elem => object.Equals(elem.Key, key)).Value;

		static public PropertyGenTimespanInt64<T> InvokeTimespan<T>(this Func<T> getValue, Func<Int64> getTime)
		{
			if (null == getValue)
				return null;

			var startTime = getTime?.Invoke() ?? 0;

			var value = getValue.Invoke();

			return new PropertyGenTimespanInt64<T>(value, startTime, getTime?.Invoke() ?? 0);
		}

		static public PropertyGenTimespanInt64<T> InvokeTimespanFromStopwatchMilli<T>(this Func<T> getValue) =>
			InvokeTimespan(getValue, Bib3.Glob.StopwatchZaitMiliSictInt);

		/// <summary>
		/// sample URL:
		/// https://de.tribalwars2.com/game.php?world=de8&character_id=XXXX
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		static public string TW2WorldIdFromUrl(this string url) =>
			url?.RegexMatchIfSuccess(@"world=(\w+\d+)")?.Groups[1]?.Value;

		static public Int64? ProcessIdFromOS(this BrowserProcessCreation browserProcessCreation) =>
			browserProcessCreation?.BrowserProcess?.ProcessIdFromOS;
	}
}
