using System.Text.RegularExpressions;

namespace BinaryFog.NameParser.Tests {
	public class TestFullNamePattern : IFullNamePattern {
		Regex IFullNamePattern.Rx => null;
		public ParsedFullName Parse(string rawName)
			=> rawName == "EXAMPLE"
				? new ParsedFullName {
					FirstName = "Success",
					LastName = "Success",
					MiddleName = "Success",
					DisplayName = "Success",
					Suffix = "Success",
					NickName = "Success",
					Title = "Success",
					Score = int.MaxValue
				}
				: null;
	}
}