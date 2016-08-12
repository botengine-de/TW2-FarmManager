using Newtonsoft.Json.Linq;
using System;

namespace FarmManager.TwStruct
{
	public class UnitListControllerCommand
	{
		public Int64? id;

		public string jobType;

		public string type;

		public int? startVillageId;

		public int? startX;

		public int? startY;

		public int? targetVillageId;

		public int? targetCharacterId;

		public int? targetX;

		public int? targetY;

		public Int64? startedAt;

		public Int64? completedAt;

		public Int64? duration;

		public bool? returning;

		public JObject units;
	}

	public class UnitListControllerInfo
	{
		public int? villageId;

		public UnitListControllerCommand[] outgoingArmies;
	}
}
