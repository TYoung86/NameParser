﻿using System.Text.RegularExpressions;
using JetBrains.Annotations;
using static BinaryFog.NameParser.RegexNameComponents;
using static BinaryFog.NameParser.NameComponentSets;

namespace BinaryFog.NameParser.Patterns {
	[UsedImplicitly]
	public class FirstInitialPrefixedLastPattern : IFullNamePattern {
		Regex IFullNamePattern.Rx => Rx;
		private static readonly Regex Rx = new Regex(
			@"^" + First + Space + Initial + Space + Prefix + Space + Last + MaybeSuffixAndOrPostNominal + @"$",
			CommonPatternRegexOptions);

		public ParsedFullName Parse(string rawName) {
			var match = Rx.Match(rawName);
			if (!match.Success) return null;
			var firstName = match.Groups["first"].Value;
			var middleName = $"{match.Groups["initial"]}.";
			var prefix = match.Groups["prefix"].Value;
			var lastPart = match.Groups["last"].Value;
			var lastName = $"{prefix} {lastPart}";

			var scoreMod = 0;
			ModifyScoreExpectedFirstName(ref scoreMod, firstName);
			ModifyScoreExpectedLastName(ref scoreMod, lastPart);

			var pn = new ParsedFullName {
				FirstName = firstName,
				MiddleName = middleName,
				LastName = lastName,
				DisplayName = $"{firstName} {middleName} {lastName}",
				Suffix = GetSuffixCapturesAndScore(ref scoreMod, match),
				Score = 100
			};
			return pn;
		}
	}
}