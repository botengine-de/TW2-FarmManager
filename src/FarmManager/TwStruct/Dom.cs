using System;

namespace FarmManager.TwStruct
{
	static public class Dom
	{
		public const string openUnitsXPath = ".//*[@ng-click=\"open('units')\"]";
		public const string openListReportXPath = ".//*[@ng-click=\"open('report-list')\"]";

		public const string VillageDropDownControllerXPath = "//div[@ng-controller='VillageDropDownController']";
		public const string ListUnitControllerXPath = "//div[@ng-controller='UnitListController']";
		public const string ListReportControllerXPath = "//div[@ng-controller='ReportListController']";
		public const string ReportControllerXPath = "//div[@ng-controller='ReportController']";
		public const string ModalCustomArmyControllerXPath = "//div[@ng-controller='ModalCustomArmyController']";

		public static string JsGetInfoFromVillageDropDownControllerElement =>
			@"function(){ var scope = angular.element(this).scope(); return JSON.stringify({" +
				nameof(FromVillageDropDownControllerInfo.selectedVillageData) + ":scope.$parent.character.data.selectedVillage.data," +
				nameof(FromVillageDropDownControllerInfo.villages) + ":scope.$parent.character.data.villages," +
				"});}";

		public const string JsGetInfoFromUnitListControllerElement = "function(){ var scope = angular.element(this).scope(); return JSON.stringify({villageId:scope.villageId, outgoingArmies:scope.outgoingArmies});}";
		public const string JsGetListReportSummaryFromControllerElement = "function(){return JSON.stringify(angular.element(this).scope().unfilteredReports);}";
		public const string JsGetReportDetailFromControllerElement = "function(){return JSON.stringify(angular.element(this).scope().report);}";
		public const string JsGetAvailableUnitsFromControllerElement = "function(){return JSON.stringify(angular.element(this).scope().availableUnits);}";

		public const string JsAttackAgainFromReportControllerElement = "function(){return angular.element(this).scope().attackAgain();}";

		static public string JsFunctionShowReportWithIdOnReportsController(this Int64 reportId) =>
			"function(){return angular.element(this).scope().showReport(" + reportId + ");}";

		static public string JsFunctionVillageSelectOnVillageDropDownController(this int villageId) =>
			"function(){return angular.element(this).scope().select(" + villageId + ");}";

		public const string PaginationSetLimitButtonXPath = "//div[contains(@ng-click,'setLimit(')]";
	}
}
