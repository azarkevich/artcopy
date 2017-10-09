using System;
using System.IO;
using System.Linq;
using ArtCopy.FileSystem;
using ArtCopy.Rules;

namespace ArtCopy
{
	class Program
	{
		class RunOptions
		{
			public string RulesFile;
			public bool DryRun;
			public string SourceRoot = Directory.GetCurrentDirectory();
			public string DestinationRoot = Directory.GetCurrentDirectory();
			public TextWriter Log = TextWriter.Null;
			public bool Force;
			public bool Summary;
		}

		// ReSharper disable once UnusedMember.Local
		static int Main(string[] args)
		{
			var opts = new RunOptions();

			try
			{
				try
				{
					if(args.Length == 0)
					{
						ShowHelp();
						return -1;
					}

					for (var i = 0; i < args.Length; i++)
					{
						if (args[i] == "--help")
						{
							ShowHelp();
							return 0;
						}

						if (args[i] == "--rules")
						{
							if (opts.RulesFile != null)
								throw new Exception("Rules file already specified.");

							opts.RulesFile = args[++i];
							continue;
						}

						if (args[i] == "--log")
						{
							var logTo = args[++i];
							opts.Log = logTo == "-" ? Console.Out : File.CreateText(logTo);
							continue;
						}

						if (args[i] == "--dry-run")
						{
							opts.DryRun = true;
							continue;
						}

						if (args[i] == "--summary")
						{
							opts.Summary = true;
							continue;
						}

						if (args[i] == "--source-base-dir")
						{
							opts.SourceRoot = args[++i];
							continue;
						}

						if (args[i] == "--destination-base-dir")
						{
							opts.DestinationRoot = args[++i];
							continue;
						}

						if (args[i] == "--force")
						{
							opts.Force = true;
							continue;
						}

						ShowHelp("Unknown option: " + args[i]);
						return -1;
					}

					if (opts.RulesFile == null)
					{
						throw new Exception("Rules file does not specified.");
					}

					if (opts.RulesFile != "-" && !File.Exists(opts.RulesFile))
					{
						throw new Exception("Rules file does not exists: " + opts.RulesFile);
					}
				}
				catch (Exception e)
				{
					ShowHelp(e.Message);
					return -1;
				}

				try
				{
					return DoWork(opts);
				}
				catch (Exception e)
				{
					Console.Error.WriteLine(e.Message);
					opts.Log.WriteLine("ERROR: Exception:\n" + e);
					return -1;
				}
			}
			finally
			{
				opts.Log.Flush();
				opts.Log.Dispose();
			}
		}

		static int DoWork(RunOptions opts)
		{
			var rulesText = opts.RulesFile == "-" ? Console.In.ReadToEnd() : File.ReadAllText(opts.RulesFile);

			var ruleSet = RuleSet.Parse(rulesText);

			if (opts.DryRun)
			{
				opts.Log.WriteLine("== DRY RUN ==");

				// load all files from source and destination
				var allFiles = Directory.GetFiles(opts.SourceRoot, "*", SearchOption.AllDirectories)
					.Concat(Directory.GetFiles(opts.DestinationRoot, "*", SearchOption.AllDirectories))
				;

				var fs = new TextFileSystem(allFiles);
				fs.Force = opts.Force;

				var copyMachine = new CopyMachine(fs, opts.SourceRoot, opts.DestinationRoot, Console.Error, opts.Log);
				var result = copyMachine.Copy(ruleSet);

				opts.Log.WriteLine("== DRY RUN RESULTS ==");

				if (fs.CreatedDirectories.Count > 0)
				{
					opts.Log.WriteLine("Created directories:");
					foreach (var dir in fs.CreatedDirectories.OrderBy(x => x))
					{
						opts.Log.WriteLine("\t" + dir);
					}
				}

				if (fs.CopiedFiles.Count > 0)
				{
					opts.Log.WriteLine("Copied files:");
					foreach (var file in fs.CopiedFiles.OrderBy(x => x))
					{
						opts.Log.WriteLine("\t" + file);
					}
				}

				if (fs.CopiedOverFiles.Count > 0)
				{
					opts.Log.WriteLine("Copied with overwite files:");
					foreach (var file in fs.CopiedOverFiles.OrderBy(x => x))
					{
						opts.Log.WriteLine("\t" + file);
					}
				}

				return result;
			}
			else
			{
				// usual run
				var fs = new RealFileSystem();
				fs.Force = opts.Force;

				var copyMachine = new CopyMachine(fs, opts.SourceRoot, opts.DestinationRoot, Console.Error, opts.Log);
				var result = copyMachine.Copy(ruleSet);

				if(opts.Summary)
				{
					Console.WriteLine($"{copyMachine.FilesCopied} files copied");
					Console.WriteLine($"Exit code: {result}");
				}

				return result;
			}
		}

		static void ShowHelp(string errorMessage = null)
		{
			if(errorMessage != null)
			{
				Console.Error.WriteLine(errorMessage);
			}
			Console.WriteLine(@"
Usage: artcopy --rules <path|-> [--dry-run] [--source-base-dir <dir>] [--destination-base-dir <dir>] [--log <path|->] [--force] [--summary]
");
		}
	}
}

