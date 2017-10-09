using System;
using System.IO;
using ArtCopy.FileSystem;
using ArtCopy.IO;
using ArtCopy.Rules;

namespace ArtCopy
{
	class CopyMachine
	{
		readonly IFileSystem _fs;
		readonly string _sourceRoot;
		readonly string _destinationRoot;
		readonly TextWriter _log;
		readonly TextWriter _errorLog;

		int _errorsCount;
		public int FilesCopied;

		public CopyMachine(IFileSystem fs, string sourceRoot, string destinationRoot, TextWriter errorLog, TextWriter log = null)
		{
			_fs = fs;
			_sourceRoot = sourceRoot.Replace('\\', '/').TrimEnd('/');
			_destinationRoot = destinationRoot.Replace('\\', '/').TrimEnd('/');
			_log = log ?? TextWriter.Null;
			_errorLog = errorLog;
		}

		public int Copy(RuleSet ruleSet)
		{
			_log.WriteLine("Start copy.");
			_errorsCount = 0;
			FilesCopied = 0;
			CopyRecurse("", ruleSet);
			_log.WriteLine($"Finish copy. {_errorsCount} errors. {FilesCopied} files copied.");

			return (_errorsCount > 0) ? 1 : 0;
		}

		void CopyRecurse(string relDir, RuleSet ruleSet)
		{
			_log.WriteLine("CopyRecurse: " + relDir);
			var dir = PathEx.CombinePath(_sourceRoot, relDir);

			// process files
			foreach (var fullPath in _fs.GetFiles(dir))
			{
				var filePath = PathEx.RemoveBase(fullPath, _sourceRoot);

				var m = ruleSet.MatchFile(filePath);

				if (m == null || m.Instructions.Count == 0)
				{
					_log.WriteLine($"File '{filePath}': not matched");
					continue;
				}

				_log.WriteLine($"File '{filePath}': matched to {m.Instructions.Count} destinations");

				foreach (var copyInstruction in m.Instructions)
				{
					var src = PathEx.CombinePath(_sourceRoot, filePath);
					var dst = PathEx.CombinePath(PathEx.CombinePath(_destinationRoot, copyInstruction.Destination), PathEx.GetFileName(filePath));

					_log.WriteLine($"Copy '{src}' to '{dst}' by rule #{copyInstruction.ByRule.RuleNo}");

					try
					{
						_fs.CopyFile(src, dst);
						FilesCopied++;
					}
					catch (Exception e)
					{
						_errorLog.WriteLine("ERROR: " + e.Message);
						_errorsCount++;
					}
				}
			}

			// process directories
			foreach (var fullPath in _fs.GetDirectories(dir))
			{
				var dirPath = PathEx.RemoveBase(fullPath, _sourceRoot);
				CopyRecurse(dirPath, ruleSet);
			}
		}
	}
}
