using System.Text.RegularExpressions;

namespace ArtCopy.Rules
{
	class Rule
	{
		public int RuleNo;
		public readonly bool Exclude;
		public Regex SourceRegex;
		public readonly string Destination;

		public Rule(int ruleNo, bool exclude, Regex sourceRegex, string destination)
		{
			RuleNo = ruleNo;
			Exclude = exclude;
			SourceRegex = sourceRegex;
			Destination = destination;
		}
	}
}
