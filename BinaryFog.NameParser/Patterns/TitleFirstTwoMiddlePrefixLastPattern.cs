using System.Text.RegularExpressions;
using JetBrains.Annotations;
using static BinaryFog.NameParser.NameComponentSets;
using static BinaryFog.NameParser.RegexNameComponents;

namespace BinaryFog.NameParser.Patterns {
	[UsedImplicitly]
	public class TitleFirstTwoMiddlePrefixLastPattern : IFullNamePattern {
		private static readonly Regex Rx = new Regex(
			@"^" + Title + Space + First + Space + TwoMiddle + Space + Prefix + Space + Last + MaybeSuffixAndOrPostNominal + @"$",
			CommonPatternRegexOptions);

		public ParsedFullName Parse(string rawName) {
			var match = Rx.Match(rawName);
			if (!match.Success) return null;
			var firstName = match.Groups["first"].Value;
			var lastPart = match.Groups["last"].Value;
			var middleName1 = match.Groups["middle1"].Value;
			var middleName2 = match.Groups["middle2"].Value;
			var middleName = $"{middleName1} {middleName2}";
			var prefix = match.Groups["prefix"].Value;
			var lastName = $"{prefix} {lastPart}";
			var scoreMod = 0;
			ModifyScoreExpectedFirstNames(ref scoreMod, firstName, middleName1, middleName2);
			ModifyScoreExpectedLastName(ref scoreMod, lastPart);
			var pn = new ParsedFullName {
				Title = match.Groups["title"].Value,
				FirstName = firstName,
				MiddleName = middleName,
				LastName = lastName,
				DisplayName = $"{firstName} {middleName} {lastName}",

				Suffix = GetSuffixCaptures(match),
				Score = 100 + scoreMod
			};
			return pn;
		}
	}
}