using System.Text.RegularExpressions;
using JetBrains.Annotations;
using static BinaryFog.NameParser.NameComponentSets;
using static BinaryFog.NameParser.RegexNameComponents;

namespace BinaryFog.NameParser.Patterns {
	[UsedImplicitly]
	public class TitleFirstMiddlePrefixLastPattern : IFullNamePattern {
		Regex IFullNamePattern.Rx => Rx;
		private static readonly Regex Rx = new Regex(
			@"^" + Title + Space + First + Space + Middle + Space + Prefix + Space + Last + MaybeSuffixAndOrPostNominal + @"$",
			CommonPatternRegexOptions);

		public ParsedFullName Parse(string rawName) {
			var match = Rx.Match(rawName);
			if (!match.Success) return null;
			var firstName = match.Groups["first"].Value;
			var lastPart = match.Groups["last"].Value;
			var middleName = match.Groups["middle"].Value;
			var prefix = match.Groups["prefix"].Value;
			var lastName = $"{prefix} {lastPart}";
			var scoreMod = 0;
			ModifyScoreExpectedFirstName(ref scoreMod, firstName);
			ModifyScoreExpectedName(ref scoreMod, middleName);
			ModifyScoreExpectedLastName(ref scoreMod, lastPart);
			var pn = new ParsedFullName {
				Title = match.Groups["title"].Value,
				FirstName = firstName,
				MiddleName = middleName,
				LastName = lastName,
				DisplayName = $"{firstName} {middleName} {lastName}",

				Suffix = GetSuffixCapturesAndScore(ref scoreMod, match),
				Score = 100 + scoreMod
			};
			return pn;
		}
	}
}