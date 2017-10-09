using System.Diagnostics;
using System.IO;

namespace ArtCopy.IO
{
	static class PathEx
	{
		public static string GetDirectoryName(string path)
		{
			if (path.EndsWith("/"))
				return path.TrimEnd('/');

			var lastSep = path.LastIndexOf('/');
			if (lastSep == -1)
				return "";

			return path.Substring(0, lastSep);
		}

		public static string GetFileName(string filePath)
		{
			return Path.GetFileName(filePath);
		}

		public static string CombinePath(string path1, string path2)
		{
			if (path1 == "")
				return path2;

			if (path2 == "")
				return path1;

			return path1 + "/" + path2;
		}

		public static string RemoveBase(string path, string basePath)
		{
			Debug.Assert(path.StartsWith(basePath));

			return path.Substring(basePath.Length).TrimStart('/');
		}
	}
}
