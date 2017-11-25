using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StellarisConsistencyChecker
{
	class Program
	{
		static void Main(string[] args)
		{
			List<Tech> techs = new List<Tech>();
			int techsFound = 0, techsWithErrors = 0, errorsFound = 0;
			string path = @"c:\Program Files (x86)\Steam\steamapps\common\Stellaris\common\technology\";
			foreach (string filepath in Directory.EnumerateFiles(path))
			{
				string tmp = File.ReadAllText(filepath);
				var rawTechs = Regex.Split(tmp, "^tech_", RegexOptions.Multiline);
				for (int i = 1; i < rawTechs.Count(); i++)
				{
					Tech t = new Tech(rawTechs[i]);
					techsFound++;
					if (t.errors.Count > 0)
					{
						techsWithErrors++;
						Console.WriteLine(string.Format("Technology tech_{0}({1}) has the following inconsistensies:", t.name, Regex.Match(filepath, "[a-zA-Z0-9_]+\\.txt$").Value));
						foreach (string error in t.errors)
						{
							errorsFound++;
							Console.WriteLine(error);
						}
						Console.WriteLine(string.Empty);
					}
					techs.Add(t);
				}
			}

			Console.WriteLine(string.Format("Summary, {0} techs scanned of which {1} has 1 or more inconsistencies, for a total issue count of {2}", techsFound, techsWithErrors, errorsFound));
			Console.ReadKey();
		}
	}
}
