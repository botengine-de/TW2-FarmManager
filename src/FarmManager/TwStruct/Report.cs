using Newtonsoft.Json.Linq;
using System;

namespace FarmManager.TwStruct
{
	/// <summary>
	/// angular.element($x("//div[@ng-controller='ReportListController']")[0]).scope().RESULT_TYPES
	/// </summary>
	public enum ReportResultEnum
	{
		OTHER = 0,
		NO_CASUALTIES = 1,
		CASUALTIES = 2,
		DEFEAT = 3,
	}

	/*
	angular.element($x("//div[@ng-controller='ReportListController']")[0]).scope().HAUL_TYPES
	HAUL_TYPES: Object
		FULL: "full"
		NONE: "none"
		PARTIAL: "partial"
	*/

	public class ReportListReportSummary
	{
		public Int64? id;

		public Int64? time_created;

		public string type;

		public string haul;

		public int? result;

		public int? read;
	}

	public class ReportControllerReport
	{
		public Int64? id;

		public Int64? time_created;

		public string title;

		public string type;

		public string haul;

		public int? result;

		public ReportControllerReportAttack ReportAttack;
	}

	public class ReportControllerReportAttack
	{
		public int? attVillageId;

		public string attVillageName;

		public int? attVillageX;

		public int? attVillageY;

		public int? attCharacterId;

		public JObject attUnits;

		public JObject attLosses;

		public int? defVillageId;

		public string defVillageName;

		public int? defVillageX;

		public int? defVillageY;

		public int? defCharacterId;

		public bool? attWon;

		public JObject defUnits;

		public JObject defLosses;

		public bool? barbarian;

		public JObject haul;
	}
}
