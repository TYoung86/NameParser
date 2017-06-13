using System.Text.RegularExpressions;
using JetBrains.Annotations;
using static BinaryFog.NameParser.NameComponentSets;
using static BinaryFog.NameParser.RegexNameComponents;

namespace BinaryFog.NameParser.Patterns {
	[UsedImplicitly]
	public class TitleFirstMiddleLastPattern : IFullNamePattern {
		private static readonly Regex Rx = new Regex(
			@"^" + Title + Space + First + Space + Middle + Space + Last + MaybeSuffixAndOrPostNominal + @"$",
			CommonPatternRegexOptions);

		public ParsedFullName Parse(string rawName) {
			var match = Rx.Match(rawName);
			if (!match.Success) return null;
			var firstName = match.Groups["first"].Value;
			var lastName = match.Groups["last"].Value;
			var middleName = match.Groups["middle"].Value;
			var scoreMod = 0;
			ModifyScoreExpectedFirstNames(ref scoreMod, firstName, middleName);
			ModifyScoreExpectedLastName(ref scoreMod, lastName);
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