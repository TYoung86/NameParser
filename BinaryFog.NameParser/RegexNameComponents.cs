﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BinaryFog.NameParser {
	using static Helpers;

	public static class RegexNameComponents {
		/// <summary>
		/// Read and escape lines from a resource stream and combine
		/// them all together with pipe characters to create a string
		/// capable of being matched against when parsed as a <see cref="Regex"/>.
		/// </summary>
		/// <param name="res">The resource stream containing strings as lines.</param>
		/// <returns>A <see cref="Regex"/> matchable concatenation of the resource.</returns>
		private static string RegexPipeJoin(Stream res) {
			// try to preallocate if stream length is known
			var resLength = checked((int) TryOrDefault(() => res.Length));
			var stringBuilder = resLength != 0
				? new StringBuilder(resLength)
				: new StringBuilder();
			using (var reader = new StreamReader(res)) {
				// first line case
				var line = reader.ReadLine();
				//if (line == null) return "";
				Debug.Assert(line != null);
				stringBuilder.Append(Regex.Escape(line));

				// second line
				line = reader.ReadLine();
				while (line != null) {
					stringBuilder.Append('|')
						.Append(Regex.Escape(line));

					// remaining lines
					line = reader.ReadLine();
				}
			}
			return stringBuilder.ToString();
		}

		public static readonly string LastNamePrefixes = RegexPipeJoin(Resources.LastNamePrefixes);
		public static readonly string PostNominals = RegexPipeJoin(Resources.PostNominals);
		public static readonly string JobTitles = RegexPipeJoin(Resources.JobTitles);
		public static readonly string Suffixes = RegexPipeJoin(Resources.Suffixes);
		public static readonly string Titles = RegexPipeJoin(Resources.Titles);
		public static readonly string CompanySuffixes = RegexPipeJoin(Resources.CompanySuffixes);


		public static readonly string JobTitle = @"(?<jobTitle>" + JobTitles + @")";

		public static readonly string Title = @"(?<title>(" + Titles + @")((?!\s)\W)?)";
		
		public static readonly string Suffix = @"(?<suffix>(" + Suffixes + @")((?!\s)\W)?)";

		public static readonly string PostNominal = @"(?<suffix>((?<=\W)(" + PostNominals + @")[\W^\s]?\s*?)+)";

		public static readonly string MaybeSuffixAndOrPostNominal = @"(" + OptionalCommaSpace + Suffix + @"?" + @"(" + Space + PostNominal + @")?)?";

		public static readonly string Prefix = @"(?<prefix>" + LastNamePrefixes + @")";

		public const string Space = @"(\s+|(?<=\W)\s*|\s*(?=\W))";
		
		// require a space if the previous or next character is a word character
		public const string OptionalSpace = @"\s*?";

		public const string OptionalCommaSpace = @"(" + OptionalSpace + @",)?" + Space;
		public const string CommaSpace = OptionalSpace + @"," + Space;
		public const string Initial = @"(?<initial>[a-z])\.?";
		public const string Vowels = @"[aeiouy]";
		public const string TwoInitial = @"(?<initial1>[a-z])(?!"+Vowels+@")\.?" + Space + @"(?<initial2>[a-z])\.?";
		public const string Name = @"'?(\w+|\w+('\w+)+)('(?=\W))?";
		public const string First = @"(?<first>"+Name+@")";
		public const string Last = @"(?<last>"+Name+@")";
		public const string Middle = @"(?<middle>"+Name+@")";
		public const string TwoMiddle = @"(?<middle1>"+Name+@")" + Space + @"(?<middle2>"+Name+@")";
		public const string Hyphen = "[-\u00AD\u058A\u1806\u2010\u2011\u30FB\uFE63\uFF0D\uFF65]";
		public const string HyphenOptionallySpaced = OptionalSpace + Hyphen + OptionalSpace;
		public const string LastHyphenated = @"(?<last>(?<last1>"+Name+@")" + HyphenOptionallySpaced + @"(?<last2>"+Name+@"))";
		public const string SpaceOrHyphen = Space + "|" + HyphenOptionallySpaced;
		public const string Words = @"(\w+|" + SpaceOrHyphen + @")+";
		public const string Nick = @"(\((?<nick>("+Words+@"))\)|(?<nickquote>['""])(?<nick>("+Words+@"))\k<nickquote>)";


		public const RegexOptions CommonPatternRegexOptions
			= RegexOptions.Compiled
			| RegexOptions.IgnoreCase
			| RegexOptions.Singleline
			| RegexOptions.CultureInvariant
			| RegexOptions.ExplicitCapture;
		
		public static string GetSuffixCapturesAndScore(ref int scoreMod, Match match, int valueEach = 25) {
			var count = match.Groups["suffix"].Captures.Count;
			if (count <= 0) return null;
			scoreMod += valueEach * count;
			return string.Join(" ", match.Groups["suffix"]
				.Captures.Cast<Capture>()
				.Select(c => c.Value));
		}
	}
}