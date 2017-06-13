using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using static BinaryFog.NameParser.NameComponentSets;
using static BinaryFog.NameParser.RegexNameComponents;

namespace BinaryFog.NameParser.Patterns {
	[UsedImplicitly]
	public class FirstHyphenatedLastNickPattern : IFullNamePattern {
		Regex IFullNamePattern.Rx => Rx;
		private static readonly Regex Rx = new Regex(
			@"^" + First + Space + LastHyphenated + Space + Nick + MaybeSuffixAndOrPostNominal + @"$",
			CommonPatternRegexOptions);

		public ParsedFullName Parse(string rawName) {
			var match = Rx.Match(rawName);
			if (!match.Success) return null;

			var firstName = match.Groups["first"].Value;
			var lastPart1 = match.Groups["last1"].Value;
			var lastPart2 = match.Groups["last2"].Value;
			var lastName = $"{lastPart1}-{lastPart2}";
			var nickName = match.Groups["nick"].Value;

			var scoreMod = 0;
			ModifyScoreExpectedFirstName(ref scoreMod, firstName);
			ModifyScoreExpectedLastName(ref scoreMod, lastPart1);
			ModifyScoreExpectedLastName(ref scoreMod, lastPart2);
			ModifyScoreExpectedName(ref scoreMod, nickName);

			var pn = new ParsedFullName {
				FirstName = firstName,
				LastName = lastName,
				NickName = nickName,
				DisplayName = $"{firstName} {lastName}",
				Suffix = GetSuffixCapturesAndScore(ref scoreMod, match),
				Score = 0 + scoreMod
			};
			return pn;
		}
	}
}