using ArtCopy;
using ArtCopy.FileSystem;
using ArtCopy.Rules;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArtCopyTests
{
	[TestClass]
	public class FileMatchTests
	{
		[TestMethod]
		public void Simple_First_Level()
		{
			var rules = @"
file.txt
";

			var rs = RuleSet.Parse(rules);

			var ci = rs.MatchFile("file.txt");

			ci.Instructions.Count.Should().Be(1);
			ci.Instructions[0].Destination.Should().BeEmpty();
		}

		[TestMethod]
		public void Simple_NotFirst_Level()
		{
			var rules = @"
dir/file.txt
";

			var rs = RuleSet.Parse(rules);

			var ci = rs.MatchFile("dir/file.txt");

			ci.Instructions.Count.Should().Be(1);
			ci.Instructions[0].Destination.Should().BeEmpty();
		}

		[TestMethod]
		public void Simple_WithDestination()
		{
			var rules = @"
dir/file.txt => DDir
";

			var rs = RuleSet.Parse(rules);

			var ci = rs.MatchFile("dir/file.txt");

			ci.Instructions.Count.Should().Be(1);
			ci.Instructions[0].Destination.Should().Be("DDir");
		}

		[TestMethod]
		public void Match_File_With_Wildcards()
		{
			var rules = @"
dir/file*.txt => DDir
";

			var rs = RuleSet.Parse(rules);

			var ci = rs.MatchFile("dir/file-a.txt");

			ci.Instructions.Count.Should().Be(1);
			ci.Instructions[0].Destination.Should().Be("DDir");
		}

		[TestMethod]
		public void Match_File_With_Wildcards2()
		{
			var rules = @"
dir/sub*dir/file.txt => DDir
";

			var rs = RuleSet.Parse(rules);

			var ci = rs.MatchFile("dir/subdir/file.txt");

			ci.Instructions.Count.Should().Be(1);
			ci.Instructions[0].Destination.Should().Be("DDir/subdir");
		}

		[TestMethod]
		public void Match_Several_Copy_Instructions()
		{
			var rules = @"
dir/file.txt => DDir
dir/file.txt => DDir2
";

			var rs = RuleSet.Parse(rules);

			var ci = rs.MatchFile("dir/file.txt");

			ci.Instructions.Count.Should().Be(2);
			ci.Instructions[0].Destination.Should().Be("DDir");
			ci.Instructions[1].Destination.Should().Be("DDir2");
		}

		[TestMethod]
		public void Match_Several_Copy_Instructions_WithExclude()
		{
			var rules = @"
dir/*.txt => DDir
-:dir/file.txt
+:dir/file.txt => DDir2
";

			var rs = RuleSet.Parse(rules);

			var ci = rs.MatchFile("dir/file.txt");

			ci.Instructions.Count.Should().Be(1);
			ci.Instructions[0].Destination.Should().Be("DDir2");
		}

		[TestMethod]
		public void FileSystem()
		{
			var fs = new TextFileSystem(@"
f1
d1/f2
d1/f3
d1/d2/f4
d1/d2/d3/
", "", "");

			var rules = @"
f1 => FF
**/f* => FFF
.artifacts/core/** => .xxx
";

			var rs = RuleSet.Parse(rules);

			var copyMachine = new CopyMachine(fs, "", "");
			copyMachine.Copy(rs);

			var added = fs.CopiedFiles.ToString();
		}
	}
}
