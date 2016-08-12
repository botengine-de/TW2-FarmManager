using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace FarmManager.TwStruct
{
	public class CharacterData
	{
		public Int64? character_id;

		public InCharacterDataVillage selectedVillage;

		public InCharacterDataVillage[] villages;
	}

	public class InCharacterDataVillage
	{
		public InCharacterDataVillageData data;
	}

	public class InCharacterDataVillageData
	{
		public int? villageId;

		public int? x;

		public int? y;

		public int? points;

		public JObject resources;

		public string name;
	}

	public class FromVillageDropDownControllerInfo
	{
		public InCharacterDataVillageData selectedVillageData;

		public JObject villages;

		public CharacterData characterData;

		public KeyValuePair<int?, InCharacterDataVillage>[] villagesParsed;
	}
}
