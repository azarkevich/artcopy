using ArtCopy.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArtCopy.FileSystem
{
	class TextFileSystem : IFileSystem
	{
		class Node
		{
			public string Name;
			public readonly List<string> Files = new List<string>();
			public readonly List<Node> SubNodes = new List<Node>();
		}

		static readonly string[] EmptyStringArray = new string[0];
		static readonly char[] SepCharArray = "/".ToCharArray();


		readonly Node _root = new Node();

		public List<string> CreatedDirectories = new List<string>();
		public List<string> CopiedFiles = new List<string>();
		public List<string> CopiedOverFiles = new List<string>();

		public bool Force { get; set; }

		public TextFileSystem(string fileSystem)
		{
			foreach (var path in fileSystem.Split('\r', '\n').Where(l => !string.IsNullOrEmpty(l)))
			{
				InsertPath(path);
			}
		}

		public TextFileSystem(IEnumerable<string> files)
		{
			foreach (var file in files)
			{
				InsertPath(file);
			}
		}

		void InsertPath(string path)
		{
			path = path.Replace('\\', '/');

			var segments = path.Split(SepCharArray, StringSplitOptions.RemoveEmptyEntries);

			if (path.EndsWith("/"))
			{
				GetNode(segments, true);
			}
			else
			{
				var node = GetNode(segments.Take(segments.Length - 1), true);
				node.Files.Add(segments.Last());
			}
		}

		Node GetNode(string path, bool create)
		{
			var segments = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (segments.Length == 0)
				return _root;

			return GetNode(segments, create);
		}

		Node GetNode(IEnumerable<string> segments, bool create)
		{
			var node = _root;

			foreach (var seg in segments)
			{
				var subNode = node.SubNodes.FirstOrDefault(n => n.Name == seg);
				if(subNode == null)
				{
					if (!create)
						return null;

					subNode = new Node { Name = seg };
					node.SubNodes.Add(subNode);
				}

				node = subNode;
			}

			return node;
		}

		public string[] GetDirectories(string dir)
		{
			var node = GetNode(dir, false);
			if (node == null)
				return EmptyStringArray;

			return node.SubNodes
				.Select(n => PathEx.CombinePath(dir, n.Name))
				.ToArray()
			;
		}

		public string[] GetFiles(string dir)
		{
			var node = GetNode(dir, false);
			if (node == null)
				return EmptyStringArray;

			return node.Files
				.Select(n => PathEx.CombinePath(dir, n))
				.ToArray()
			;
		}

		public void CopyFile(string srcFilePath, string dstFilePath)
		{
			var srcSegments = srcFilePath.Split(SepCharArray, StringSplitOptions.RemoveEmptyEntries);
			if (srcSegments.Length == 0)
				throw new Exception("Invalid source file name: empty");

			var node = GetNode(srcSegments.Take(srcSegments.Length - 1), false);
			if (node == null || !node.Files.Contains(srcSegments.Last()))
				throw new Exception("File not found: " + srcFilePath);

			var dstSegments = dstFilePath.Split(SepCharArray, StringSplitOptions.RemoveEmptyEntries);
			if (dstSegments.Length == 0)
				throw new Exception("Invalid destination file name: empty");

			var dstNode = GetNode(dstSegments.Take(dstSegments.Length - 1), false);
			if(dstNode == null)
			{
				dstNode = GetNode(dstSegments.Take(dstSegments.Length - 1), true);
				CreatedDirectories.Add(PathEx.GetDirectoryName(dstFilePath));
			}

			if(dstNode.Files.Contains(dstSegments.Last()))
			{
				if(!Force)
					throw new Exception("File already exixts: " + dstFilePath);
				CopiedOverFiles.Add(dstFilePath);
			}
			else
			{
				dstNode.Files.Add(dstSegments.Last());
				CopiedFiles.Add(dstFilePath);
			}
		}
	}
}
