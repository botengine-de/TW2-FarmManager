using Bib3;
using BotEngine;
using System.Collections.Generic;
using System.Linq;

namespace FarmManager.TwStruct
{
	static public class Extension
	{
		static public ReportResultEnum ResultEnum(this int reportResult) => (ReportResultEnum)reportResult;

		static public ReportResultEnum? ResultEnum(this ReportListReportSummary report) => report?.result?.ResultEnum();

		static public ReportResultEnum? ResultEnum(this ReportControllerReport report) => report?.result?.ResultEnum();

		static public FromVillageDropDownControllerInfo FromVillageDropDownControllerInfoParse(this string serial)
		{
			var parsed = serial?.DeserializeFromString<FromVillageDropDownControllerInfo>();

			if (null == parsed)
				return null;

			parsed.villagesParsed =
				parsed?.villages?.Properties()?.Select(property => new KeyValuePair<int?, InCharacterDataVillage>(
					property.Name?.TryParseInt(),
					property.Value.ToString()?.DeserializeFromString<InCharacterDataVillage>()))?.ToArray();

			return parsed;
		}
	}
}
