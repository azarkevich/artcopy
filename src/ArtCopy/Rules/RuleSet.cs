using ArtCopy.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ArtCopy.Rules
{
	class RuleSet : IRule
	{
		readonly Rule[] _rules;

		RuleSet(Rule[] rules)
		{
			_rules = rules;
		}

		public FileCopyInstructions MatchFile(string path)
		{
			var instructions = new List<FileCopyInstruction>();
			foreach (var rule in _rules)
			{
				var m = rule.SourceRegex.Match(path);
				if (!m.Success)
					continue;

				if (rule.Exclude)
				{
					instructions.Clear();
					continue;
				}

				var destination = rule.Destination;

				if(m.Groups["wcmatch"].Success)
				{
					var wcmatch = m.Groups["wcmatch"].Value;
					var additionalRelativePath = PathEx.GetDirectoryName(wcmatch);

					if(!string.IsNullOrEmpty(additionalRelativePath))
						destination = destination + "/" + additionalRelativePath;
				}

				if(instructions.All(i => i.Destination != destination))
				{
					instructions.Add(new FileCopyInstruction { Destination = destination, ByRule = rule });
				}
			}

			return new FileCopyInstructions { Instructions = instructions };
		}

		public static RuleSet Parse(string text)
		{
			var rules = new List<Rule>();

			var lines = text.Split('\r', '\n');
			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];

				if (string.IsNullOrWhiteSpace(line))
					continue;

				rules.Add(ParseRule(i + 1, line));
			}

			return new RuleSet(rules.ToArray());
		}

		static readonly Regex DestinationSplitter = new Regex(@"\s*=>\s*");

		static Rule ParseRule(int lineNo, string line)
		{
			var originalLine = line;

			var exclude = false;
			if(line.StartsWith("+:"))
			{
				line = line.Substring(2);
			}
			else if(line.StartsWith("-:"))
			{
				exclude = true;
				line = line.Substring(2);
			}

			// split by =>
			var parts = DestinationSplitter.Split(line);
			if(parts.Length > 2)
			{
				throw new Exception($"Rule parsing error on line {lineNo}. '{originalLine}' has more than one separator '=>'");
			}

			var source = parts[0];
			var destination = parts.Length > 1 ? parts[1] : "";

			return new Rule(lineNo, exclude, ConvertSourceToRegex(source), destination);
		}

		static Regex ConvertSourceToRegex(string path)
		{
			// normalize path
			path = path.Replace('\\', '/');

			var textSegments = path.Split('/')
				.Where(s => s != "")
				.ToArray()
			;

			// segments before first wildcard
			var nonWildcardSegments = textSegments
				.TakeWhile(s => s.IndexOf("*", StringComparison.Ordinal) == -1 && s.IndexOf("?", StringComparison.Ordinal) == -1)
				.ToArray()
			;

			// segments including and after first wildcard
			var wildcardSegments = textSegments
				.SkipWhile(s => s.IndexOf("*", StringComparison.Ordinal) == -1 && s.IndexOf("?", StringComparison.Ordinal) == -1)
				.ToArray()
			;

			var sb = new StringBuilder("^");

			if (nonWildcardSegments.Length > 0)
				sb.Append(ConvertToRegexExpression(string.Join("/", nonWildcardSegments)));

			if(wildcardSegments.Length > 0)
			{
				if (nonWildcardSegments.Length > 0)
					sb.Append("/");
				sb.Append("(?<wcmatch>");
				sb.Append(ConvertToRegexExpression(string.Join("/", wildcardSegments)));
				sb.Append(")");
			}

			sb.Append("$");

			return new Regex(sb.ToString(), RegexOptions.IgnoreCase);
		}

		static string ConvertToRegexExpression(string segment)
		{
			const string DoubleStarMarker = "=FB27CE48CEC64F2C9BD2CF7B6E4491BD=";
			const string StarMarker = "=EB87F8E0727A43A282C9F19BDFB73FF3=";
			const string QuestionMarker = "=D87A57A88EC045928497BE1D9F6C1B1E=";

			var markedSegment = segment
				.Replace("**", DoubleStarMarker)
				.Replace("*", StarMarker)
				.Replace("?", QuestionMarker)
			;

			var escapedSegment = Regex.Escape(markedSegment);

			var expression = escapedSegment
				.Replace(DoubleStarMarker, ".*?")
				.Replace(StarMarker, "[^/]*?")
				.Replace(QuestionMarker, "[^/]")
			;

			return expression;
		}
	}
}
