using System.Text.RegularExpressions;
using JetBrains.Annotations;
using static BinaryFog.NameParser.RegexNameComponents;
using static BinaryFog.NameParser.NameComponentSets;

namespace BinaryFog.NameParser.Patterns {
	[UsedImplicitly]
	public class SingleNameOnlyPattern : IFullNamePattern {
		Regex IFullNamePattern.Rx => Rx;
		private static readonly Regex Rx = new Regex(
			@"^" + First + MaybeSuffixAndOrPostNominal + @"$",
			CommonPatternRegexOptions);


		public ParsedFullName Parse(string rawName) {
			var match = Rx.Match(rawName);
			if (!match.Success) return null;
			int scoreMod = 0;
			var pn = new ParsedFullName {
				DisplayName = rawName,

				Suffix = GetSuffixCapturesAndScore(ref scoreMod, match),
				Score = 50 + scoreMod
			};

			var matchedName = match.Groups["first"].Value;

			if (LastNames.Contains(matchedName))
				pn.LastName = matchedName;
			else
				pn.FirstName = matchedName;

			return pn;
		}
	}
}