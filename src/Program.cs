using System;
using System.IO;
using System.Linq;

namespace SandR
{
	class Program
	{
		private static int _filesProcessed;
	    private static bool _rename;

		static void Main(string[] args)
		{
			_filesProcessed = 0;

			if(args == null || !new[]{ 2, 3, }.Contains(args.Length))
			{
				Usage(args);
				return;
			}

		    var search = args[0];
		    var replace = args[1];

            if (args.Length == 3 && (args[0] ?? string.Empty).Trim().ToLower() == "/rename")
            {
                Console.WriteLine("Rename enabled");
                _rename = true;

                search = args[1];
                replace = args[2];
            }

			Process(new DirectoryInfo("."), search, replace);
			Console.WriteLine(String.Format("Done!  Processed {0} files.", _filesProcessed));
		}

		static void Usage(string[] args)
		{
            Console.WriteLine("usage: sandr (v0.1) [/rename] [searchText / searchFile] [replaceText / replaceFile]");
            Console.WriteLine("sandr will search & replace the specified text (or content of a file) within its current directory");
            Console.WriteLine("use the /rename flag to rename any files found");
            Console.WriteLine("\r\nArgs: {0}", string.Join(", ", args));
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

		    search = search.Trim();
		    replace = replace.Trim();

            if (string.IsNullOrEmpty(search))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Search is empty, exiting...");
                Console.ResetColor();
                return;
            }

            if (string.IsNullOrEmpty(replace))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Replace is empty, exiting...");
                Console.ResetColor();
                return;
            }

		    Console.WriteLine("Replacing {0} with {1}", search, replace);

            // Move the directory
            if (dir.Name.Contains(search) && _rename && dir.Parent != null)
            {
                var oldDirName = dir.FullName;
                var newDirName = Path.Combine(dir.Parent.FullName, dir.Name.Replace(search, replace));

                Console.WriteLine("Moving directory {0} to {1}", oldDirName, newDirName);
                Directory.Move(oldDirName, newDirName);

                // Reset the current directory
                dir = new DirectoryInfo(newDirName);
            }

            if(_rename)
                Console.WriteLine("Renaming '*{0}*' to '*{1}*'", search, replace);

			foreach(var file in dir.GetFiles("*.*"))
			{
				try
				{
					var text = File.ReadAllText(file.FullName);
					if (text.Contains(search) && !IsFile(file, searchFile) && !IsFile(file, replaceFile))
					{
						File.WriteAllText(file.FullName, text.Replace(search, replace));

                        if(file.Name.Contains(search) && _rename)
                        {
                            var oldFileName = Path.Combine(file.Directory.FullName, file.Name);
                            var newFileName = Path.Combine(file.Directory.FullName, file.Name.Replace(search, replace));

                            Console.WriteLine("Renaming {0} to {1}", oldFileName, newFileName);
					        File.Copy(oldFileName, newFileName);
                            File.Delete(oldFileName);
					    }

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
