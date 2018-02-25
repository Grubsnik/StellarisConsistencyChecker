using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StellarisConsistencyChecker
{
	public class Tech
	{
		private string raw;
		private string category;
		private string area;
		public string name;
		public List<string> errors;
		private string tier;
		private string costTier;
		private string costSubTier;
		private string costString;


		public Tech(string raw)
		{
			this.raw = raw;
			this.errors = new List<string>();
			this.name = Regex.Match(raw, "^([a-zA-Z0-9_]+)").Groups[0].Value;
			this.category = Regex.Match(raw, "category = { ([a-zA-Z0-9_]+) }").Groups[1].Value;
			this.area = Regex.Match(raw, "area = ([a-zA-Z0-9_]+)").Groups[1].Value;
			this.tier = Regex.Match(raw, "tier = ([a-zA-Z0-9_@]+)").Groups[1].Value;
			var cost = Regex.Match(raw, "cost = (@tier([0-9]+)cost([0-9]+))");
			this.costTier = cost.Groups[2].Value;
			this.costSubTier = cost.Groups[3].Value;
			this.costString = cost.Groups[1].Value; 
			this.PerformValidation();
		}

		private void PerformValidation()
		{
			this.CheckAreaAndCategory();
			this.CheckTierAndCost();
			this.CheckAreaConsistency();
			this.CheckLeaderTraitConsistency();
		}

		private void CheckLeaderTraitConsistency()
		{
			if (this.FilterIrregularTechs() && !this.raw.Contains(string.Format("has_trait = \"leader_trait_expertise_{0}\"", this.category)))
			{
				this.errors.Add(string.Format("\tTechnology belongs to category {0}, but this expertise is not referenced in the tech", this.category));
			}
		}

		private void CheckAreaConsistency()
		{
			var matches = Regex.Matches(this.raw, "area = ([a-zA-Z0-9_]+)");
			foreach (Match match in matches)
			{
				if (match.Groups[1].Value.CompareTo(this.area) != 0)
				{
					this.errors.Add(string.Format("\tContains at least 2 different area references ({0}) and ({1})", this.area, match.Groups[1]));
					break;
				}
			}
		}

		private void CheckAreaAndCategory()
		{
			switch (this.area)
			{
				case "engineering":
					if (!(category == "materials" || category == "voidcraft" || category == "propulsion" || category == "industry"))
					{
						this.errors.Add(string.Format("\tCategory {0} does not belong in area {1}", this.category, this.area));
					}
					break;
				case "society":
					if (!(category == "military_theory" || category == "biology" || category == "statecraft" || category == "psionics" || category == "new_worlds"))
					{
						this.errors.Add(string.Format("\tCategory {0} does not belong in area {1}", this.category, this.area));
					}
					break;
				case "physics":
					if (!(category == "particles" || category == "computing" || category == "field_manipulation"))
					{
						this.errors.Add(string.Format("\tCategory {0} does not belong in area {1}", this.category, this.area));
					}
					break;
				default:
					this.errors.Add("Unknown area");
					break;
			}
		}

		private void CheckTierAndCost()
		{
			if (this.FilterIrregularTechs()
				&& this.tier.CompareTo(this.costTier) != 0)
			{
				this.errors.Add(string.Format("\tTier ({0}) and cost ({1}) do not align", this.tier, this.costString));
			}

			if (string.IsNullOrEmpty(this.tier))
			{
				this.errors.Add("\tUnable to parse Tier Information, maybe it is missing?");
			}
		}

		private bool FilterIrregularTechs()
		{
			int tier = 0;
			bool parsable = int.TryParse(this.tier, out tier);
			return !string.IsNullOrEmpty(this.tier)
							&& parsable
							&& tier > 0
							&& !this.raw.Contains("weight = 0");
		}
	}
}
