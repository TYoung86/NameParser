using System.Text.RegularExpressions;
using JetBrains.Annotations;
using static BinaryFog.NameParser.RegexNameComponents;
using static BinaryFog.NameParser.NameComponentSets;

namespace BinaryFog.NameParser.Patterns {
	[UsedImplicitly]
	public class FirstNickMiddleHyphenatedLastPattern : IFullNamePattern {
		Regex IFullNamePattern.Rx => Rx;
		private static readonly Regex Rx = new Regex(
			@"^" + First + Space + Nick + Space + Middle + Space + LastHyphenated + MaybeSuffixAndOrPostNominal + @"$",
			CommonPatternRegexOptions);

		public ParsedFullName Parse(string rawName) {
			var match = Rx.Match(rawName);
			if (!match.Success) return null;
			var firstName = match.Groups["first"].Value;
			var nickName = match.Groups["nick"].Value;
			var middleName = match.Groups["middle"].Value;
			var lastPart1 = match.Groups["last1"].Value;
			var lastPart2 = match.Groups["last2"].Value;

			var scoreMod = 0;
			ModifyScoreExpectedFirstNames(ref scoreMod, firstName, middleName);
			ModifyScoreExpectedName(ref scoreMod, nickName);
			ModifyScoreExpectedLastName(ref scoreMod, lastPart1);
			ModifyScoreExpectedLastName(ref scoreMod, lastPart2);

			var pn = new ParsedFullName {
				FirstName = firstName,
				NickName = nickName,
				MiddleName = middleName,
				LastName = $"{lastPart1}-{lastPart2}",
				DisplayName = $"{firstName} {middleName} {lastPart1}-{lastPart2}",
				Suffix = GetSuffixCapturesAndScore(ref scoreMod, match),
				Score = 175 + scoreMod
			};
			return pn;
		}
	}
}