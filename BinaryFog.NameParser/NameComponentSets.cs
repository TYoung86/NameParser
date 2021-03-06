using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BinaryFog.NameParser {
	internal static class NameComponentSets {
		/// <summary>
		/// Reads lines from the stream as an enumeration of strings.
		/// </summary>
		/// <param name="res">The resource stream.</param>
		/// <returns>Lines extracted as strings from the stream.</returns>
		public static IEnumerable<string> ReadLines(Stream res) {
			using (var reader = new StreamReader(res)) {
				var line = reader.ReadLine();
				while (!string.IsNullOrWhiteSpace(line)) {
					yield return line;
					line = reader.ReadLine();
				}
			}
		}


		[SuppressMessage("ReSharper", "InconsistentNaming")] private static readonly ImmutableHashSet<string> _maleFirstNames =
			ReadLines(Resources.MaleFirstNames).ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

		[SuppressMessage("ReSharper", "InconsistentNaming")] private static readonly ImmutableHashSet<string> _femaleFirstNames =
			ReadLines(Resources.FemaleFirstNames).ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

		public static readonly ImmutableHashSet<string> MaleFirstNames = _maleFirstNames;
		public static readonly ImmutableHashSet<string> FemaleFirstNames = _femaleFirstNames;


		public static readonly ImmutableHashSet<string> FirstNames =
			_maleFirstNames.Union(_femaleFirstNames);

		public static readonly ImmutableHashSet<string> LastNames =
			ReadLines(Resources.LastNames).ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Apply adjustments to a score as a name is matched against the given dictionary.
		/// </summary>
		/// <param name="dictionary">A given dictionary to match against.</param>
		/// <param name="score">The score to adjust.</param>
		/// <param name="name">The name to attempt to match.</param>
		/// <param name="matchValue">The value to adjust the score by if matched by the given dictionary.</param>
		internal static void ModifyScore(ISet<string> dictionary, ref int score, string name, int matchValue, int noMatchValue) {
			if (dictionary.Contains(name)) score += matchValue;
			else score += noMatchValue;
		}

		/// <summary>
		/// Apply adjustments to a score as a name is matched against first and last name dictionaries.
		/// </summary>
		/// <param name="score">The score to adjust.</param>
		/// <param name="name">The name to attempt to match.</param>
		/// <param name="matchedFirstValue">The value to adjust the score by if matched by the first name dictionary.</param>
		/// <param name="matchedLastValue">The value to adjust the score by if matched by the last name dictionary.</param>
		internal static void ModifyScore(ref int score, string name, int matchedFirstValue, int matchedLastValue) {
			ModifyScore(FirstNames, ref score, name, matchedFirstValue, 0);
			ModifyScore(LastNames, ref score, name, matchedLastValue, 0);
		}

		/// <summary>
		/// Apply a slight adjustment to a score if a name is matched against any name dictionary.
		/// </summary>
		/// <param name="score">The score to adjust.</param>
		/// <param name="name">The name to attempt to match.</param>
		/// <param name="value">The value to adjust the score by if matched.</param>
		internal static void ModifyScoreExpectedName(ref int score, string name, int value = 10)
			=> ModifyScore(ref score, name, value, value);

		/// <summary>
		/// Apply an adjustment to a score if a name is matched against the gender separated first name dictionaries.
		/// An additional adjustment is applied if the next name is also matched against the same dictionary.
		/// An opposite adjustments are applied if the names are matched in the last name dictionary.
		/// </summary>
		/// <param name="score">The score to adjust.</param>
		/// <param name="name1">The name to attempt to match as either male or female.</param>
		/// <param name="name2">The name to attempt to match as the same gender as the first.</param>
		/// <param name="value">The value to adjust the score by if matched.</param>
		internal static void ModifyScoreExpectedFirstNames(ref int score, string name1, string name2, int value = 10) {
			var names = new[] {name1, name2};
			var maleNames = MaleFirstNames.Intersect(names);
			var femaleNames = FemaleFirstNames.Intersect(names);
			var maleOnlyNames = maleNames.Except(femaleNames);
			var femaleOnlyNames = femaleNames.Except(maleNames);
			var countSum = maleOnlyNames.Count + femaleOnlyNames.Count;
			if (countSum != 0) {
				score += maleOnlyNames.Count == 0
					|| femaleOnlyNames.Count == 0
					? value * countSum * 2
					: value * countSum;
			}

			var lastNames = LastNames.Intersect(names);
			score += -value * lastNames.Count;
		}

		/// <summary>
		/// Apply an adjustment to a score if a name is matched against the gender separated first name dictionaries.
		/// An additional adjustment is applied if the next names are also matched against the same dictionary.
		/// An opposite adjustments are applied if the names are matched in the last name dictionary.
		/// </summary>
		/// <param name="score">The score to adjust.</param>
		/// <param name="name1">The name to attempt to match as either male or female.</param>
		/// <param name="name2">The name to attempt to match as the same gender as the first.</param>
		/// <param name="name3">Another name to attempt to match as the same gender as the first.</param>
		/// <param name="value">The value to adjust the score by if matched.</param>
		internal static void ModifyScoreExpectedFirstNames(ref int score, string name1, string name2, string name3, int value = 10) {
			var names = new[] {name1, name2, name3};
			var maleNames = MaleFirstNames.Intersect(names);
			var femaleNames = FemaleFirstNames.Intersect(names);
			var maleOnlyNames = maleNames.Except(femaleNames);
			var femaleOnlyNames = femaleNames.Except(maleNames);
			var countSum = maleOnlyNames.Count + femaleOnlyNames.Count;
			if (countSum != 0) {
				score += maleOnlyNames.Count == 0
						|| femaleOnlyNames.Count == 0
					? value * countSum * 2
					: value * countSum;
			}

			var lastNames = LastNames.Intersect(names);
			score += -value * lastNames.Count;
		}

		/// <summary>
		/// Apply an adjustment to a score if a name is matched against the first name dictionary.
		/// An opposite adjustment is applied if the name is matched in the last name dictionary.
		/// </summary>
		/// <param name="score">The score to adjust.</param>
		/// <param name="name">The name to attempt to match.</param>
		/// <param name="value">The value to adjust the score by if matched.</param>
		internal static void ModifyScoreExpectedFirstName(ref int score, string name, int value = 25) {
			ModifyScore(FirstNames, ref score, name, value, 0);
			ModifyScore(LastNames, ref score, name, -value, value/5);
		}

		/// <summary>
		/// Apply an adjustment to a score if a name is matched against the last name dictionary.
		/// An opposite adjustment is applied if the name is matched in the first name dictionary.
		/// </summary>
		/// <param name="score">The score to adjust.</param>
		/// <param name="name">The name to attempt to match.</param>
		/// <param name="value">The value to adjust the score by if matched.</param>
		internal static void ModifyScoreExpectedLastName(ref int score, string name, int value = 25) {
			ModifyScore(LastNames, ref score, name, value, 0);
			ModifyScore(FirstNames, ref score, name, -value, value/5);
		}
	}
}