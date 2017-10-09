using System.Collections.Generic;

namespace ArtCopy.FileSystem
{
	interface IFileSystem
	{
		string[] GetDirectories(string dir);
		string[] GetFiles(string dir);

		bool Force { get; set; }
		void CopyFile(string srcFilePath, string dstFilePath);
	}
}
