using System;
using System.IO;
using System.Linq;

namespace SandR
{
	class Program
	{
		private static int _filesProcessed;

		static void Main(string[] args)
		{
			_filesProcessed = 0;

			if(args == null || (args.Length != 2 && args.Length != 3))
			{
				Usage();
				return;
			}

			Process(new DirectoryInfo(args[0]), args[1], args.Length == 3 ? args[2] : String.Empty);
			Console.WriteLine(String.Format("Done!  Processed {0} files.", _filesProcessed));
		}

		static void Usage()
		{
			Console.WriteLine("usage: sandr (v0.1) [directory] [searchText / searchFile] [replaceText / replaceFile] ");
			Console.WriteLine("sandr will search & replace the specified text (or content of a file) within its current directory");
		}

		static void Process(DirectoryInfo dir, string search, string replace)
		{
			// If either argument is a file, use the content of the file
			FileInfo searchFile = null;
			FileInfo replaceFile = null;

			if (File.Exists(search))
			{
				Console.WriteLine(String.Format("Using file for search text, {0}", search));
				searchFile = new FileInfo(search);
				search = File.ReadAllText(search);
			} 
			
			if (File.Exists(replace))
			{
				Console.WriteLine(String.Format("Using file for replace text, {0}", replace));
				replaceFile = new FileInfo(replace);
				replace = File.ReadAllText(replace);
			}

			Console.WriteLine(String.Format("Processing {0}...", dir.FullName));

			foreach(var file in dir.GetFiles("*.*"))
			{
				try
				{
					var text = File.ReadAllText(file.FullName);
					if (text.Contains(search.Trim()) && !IsFile(file, searchFile) && !IsFile(file, replaceFile))
					{
						File.WriteAllText(file.FullName, text.Replace(search.Trim(), replace.Trim()));
						_filesProcessed++;

						Console.WriteLine(String.Format(" replaced {0}", file.FullName));
						continue;
					}
				}
				catch(Exception ex)
				{
					Console.WriteLine(String.Format(" skipping {0}", file.FullName));
					Console.WriteLine(String.Format(" error {0}", ex.Message));
				}

				Console.WriteLine(String.Format(" skipping {0}", file.FullName));
			}

			foreach(var directory in dir.GetDirectories())
			{
				Process(directory, search, replace);
			}
		}

		static bool IsFile(FileSystemInfo fileOne, FileSystemInfo fileTwo)
		{
			if (fileOne == null || fileTwo == null) return false;

			return fileTwo.FullName == fileOne.FullName;
		}
	}
}
