using System;
using System.Diagnostics;

namespace BinaryFog.NameParser {
	[Conditional("TRACE")]
	[AttributeUsage(AttributeTargets.All,Inherited = false)]
	internal class ExcludeFromCodeCoverageAttribute : Attribute {
	}
}