using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MigrateData3to4
{
	static class Utils
	{
		internal static void LogMessage(string message)
		{
			System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + message);
		}

		internal static char GetLogFileSeparator(string line, char defSep)
		{
			// we know the dayfile and monthly log files start with
			// dd/MM/yy,NN,...
			// dd/MM/yy,hh:mm,N.N,....
			// so we just need to find the first separator after the date before a number

			var reg = Regex.Match(line, @"\d{2}[^\d]+\d{2}[^\d]+\d{2}([^\d])");
			if (reg.Success)
				return reg.Groups[1].Value[0];
			else
				return defSep;
		}

		internal static bool TryDetectNewLine(string path, out string newLine)
		{
			using var fs = File.OpenRead(path);
			char prevChar = '\0';

			// read the first 1000 characters to try and find a newLine
			for (var i = 0; i < 1000; i++)
			{
				int b;
				if ((b = fs.ReadByte()) == -1)
					break;

				char curChar = (char)b;

				if (curChar == '\n')
				{
					newLine = prevChar == '\r' ? "\r\n" : "\n";
					return true;
				}

				prevChar = curChar;
			}

			// Returning false means could not determine linefeed convention
			newLine = Environment.NewLine;
			return false;
		}

		internal static string DdmmyyStrToStr(string d)
		{
			// Horrible hack, but we have localised separators, but UK sequence, so localised parsing may fail
			// Determine separators from the strings, allow for multi-byte!
			var datSep = Regex.Match(d, @"[^0-9]+").Value;

			// Converts a date string in UK order to a DateTime
			string[] date = d.Split(new string[] { datSep }, StringSplitOptions.None);

			return date[0] + "/" + date[1] + "/" + date[2];
		}

		internal static DateTime DdmmyyStrToDate(string d)
		{
			// Horrible hack, but we have localised separators, but UK sequence, so localised parsing may fail
			// Determine separators from the strings, allow for multi-byte!
			var datSep = Regex.Match(d, @"[^0-9]+").Value;

			// Converts a date string in UK order to a DateTime
			string[] date = d.Split(new string[] { datSep }, StringSplitOptions.None);

			int D = Convert.ToInt32(date[0]);
			int M = Convert.ToInt32(date[1]);
			int Y = Convert.ToInt32(date[2]);
			if (Y > 70)
			{
				Y += 1900;
			}
			else
			{
				Y += 2000;
			}

			return new DateTime(Y, M, D);
		}

		internal static DateTime DdmmyyhhmmStrToDate(string d, string t)
		{
			// Horrible hack, but we have localised separators, but UK sequence, so localised parsing may fail
			// Determine separators from the strings, allow for multi-byte!
			var datSep = Regex.Match(d, @"[^0-9]+").Value;
			var timSep = Regex.Match(t, @"[^0-9]+").Value;

			// Converts a date string in UK order to a DateTime
			string[] date = d.Split(new string[] { datSep }, StringSplitOptions.None);
			string[] time = t.Split(new string[] { timSep }, StringSplitOptions.None);

			int D = Convert.ToInt32(date[0]);
			int M = Convert.ToInt32(date[1]);
			int Y = Convert.ToInt32(date[2]);

			// Double check - just in case we get a four digit year!
			if (Y < 1900)
			{
				Y += Y > 70 ? 1900 : 2000;
			}
			int h = Convert.ToInt32(time[0]);
			int m = Convert.ToInt32(time[1]);

			return new DateTime(Y, M, D, h, m, 0, DateTimeKind.Local);
		}

		internal static long ToUnixTime(DateTime dateTime)
		{
			var dateTimeOffset = new DateTimeOffset(dateTime);
			return dateTimeOffset.ToUnixTimeSeconds();
		}

		internal static string GetLogFileName(DateTime thedate)
		{
			var datestring = thedate.ToString("yyyy-MM");

			return datestring + "-log.txt";
		}

		internal static string GetExtraLogFileName(DateTime thedate)
		{
			var datestring = thedate.ToString("yyyy-MM");

			return "Extra-" + datestring + "-log.txt";
		}

		internal static string GetAirLinkLogFileName(DateTime thedate)
		{
			var datestring = thedate.ToString("yyyy-MM");

			return "AirLink-" + datestring + "-log.txt";
		}

		internal static DateTime ResolveAmbiguousTime(DateTime ambiguousTime, bool useDstTime)
		{
			// Time is not ambiguous
			if (!TimeZoneInfo.Local.IsAmbiguousTime(ambiguousTime))
			{
				return ambiguousTime.ToUniversalTime();
			}
			// Time is ambiguous
			else
			{
				var offsets = TimeZoneInfo.Local.GetAmbiguousTimeOffsets(ambiguousTime);
				var offset = TimeZoneInfo.Local.BaseUtcOffset;

				if (useDstTime)
				{
					for (var i = 0; i < offsets.Length; i++)
					{
						if (!offsets[i].Equals(TimeZoneInfo.Local.BaseUtcOffset))
						{
							offset = offsets[i];
							break;
						}
					}
				}
				return DateTime.SpecifyKind(ambiguousTime - offset, DateTimeKind.Utc);
			}
		}
	}
}
