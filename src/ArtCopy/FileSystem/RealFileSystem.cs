using System.IO;
using System.Linq;
using ArtCopy.IO;

namespace ArtCopy.FileSystem
{
	class RealFileSystem : IFileSystem
	{
		public bool Force { get; set; }

		public string[] GetDirectories(string dir)
		{
			dir = dir.Replace('\\', '/');

			return Directory
				.GetDirectories(dir)
				.Select(d => d.Replace('\\', '/'))
				.ToArray()
			;
		}

		public string[] GetFiles(string dir)
		{
			dir = dir.Replace('\\', '/');

			return Directory
				.GetFiles(dir)
				.Select(f => f.Replace('\\', '/'))
				.ToArray()
			;
		}

		public void CopyFile(string srcFilePath, string dstFilePath)
		{
			var dstDir = PathEx.GetDirectoryName(dstFilePath);
			if (!Directory.Exists(dstDir))
				Directory.CreateDirectory(dstDir);

			// if force enabled and file already exists - reset RO flag if necessary
			if(Force && File.Exists(dstFilePath))
			{
				var flags = File.GetAttributes(dstFilePath);
				if (flags.HasFlag(FileAttributes.ReadOnly))
				{
					File.SetAttributes(dstFilePath, flags & ~FileAttributes.ReadOnly);
				}
			}

			File.Copy(srcFilePath, dstFilePath, Force);
		}
	}
}
