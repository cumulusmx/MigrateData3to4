using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MigrateData3to4
{
	static partial class OtherFiles
	{
		static internal void Copy()
		{
			string[] files = [
				"alltime.ini",
				"month.ini",
				"monthlyalltime.ini",
				"today.ini",
				"year.ini",
				"yesterday.ini",
				"cumulusmx.db",
				"diary.db"
			];
			var fileSep = Path.DirectorySeparatorChar;

			Console.WriteLine("\nCopying 'other' files");
			Utils.LogMessage("Copying 'other' files");


			foreach (string file in files)
			{
				var src = Program.Src + fileSep + file;
				var dst = Program.Dst + fileSep + file;
				// test if the file exists
				if (File.Exists(src))
				{
					try
					{
						var col = Console.ForegroundColor;
						Console.Write($"Copying {file} to destination... ");
						Utils.LogMessage($"Copying {file} to destination");
						File.Copy(src, dst, true);
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine($"Copied OK");
						Utils.LogMessage($"Copied {file} to destination");
						Console.ForegroundColor = col;
					}
					catch (Exception ex )
					{
						var col = Console.ForegroundColor;
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"Error copying file {file} - {ex.Message}\n");
						Utils.LogMessage($"Error copying file {file} - {ex.Message}");
						Console.ForegroundColor = col;
					}
				}
				else
				{
					var col = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine($"File {file} not found: Manually copy this file to the \\data folder if required");
					Utils.LogMessage($"File {file} not found");
					Console.ForegroundColor = col;
				}
			}
		}

		static internal void CopyMonthIni()
		{
			var reg = MonthlyIniRegex();
			var monFiles = Directory.GetFiles(Program.Src, "month*.ini").Where(path => reg.IsMatch(path)).ToArray();

			foreach (var file in monFiles)
			{
				try
				{
					var dst = Program.Dst + Path.DirectorySeparatorChar + file.Split(Path.DirectorySeparatorChar)[^1];
					var col = Console.ForegroundColor;
					Console.Write($"Copying {file} to destination... ");
					Utils.LogMessage($"Copying {file} to destination");
					File.Copy(file, dst, true);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine($"Copied OK");
					Utils.LogMessage($"Copied {file} to destination");
					Console.ForegroundColor = col;
				}
				catch (Exception ex)
				{
					var col = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Error copying file {file} - {ex.Message}\n");
					Utils.LogMessage($"Error copying file {file} - {ex.Message}");
					Console.ForegroundColor = col;
				}
			}
		}

		[GeneratedRegex(@"month[0-9]{6}\.ini")]
		private static partial Regex MonthlyIniRegex();
	}
}
