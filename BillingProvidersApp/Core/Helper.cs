using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace BillingProvidersApp.Core
{
    /// <summary>
    /// Delegate returns true if file <paramref name="fileName" /> exists.
    /// </summary>
    public delegate bool FileExistsCallback(string fileName);

    /// <summary>
    /// Delegate provides <paramref name="copiedByteCount" /> for stream.
    /// </summary>
    public delegate void StreamCopyCallback(long copiedByteCount);


    /// <summary>
    /// Class contains different helpful methods.
    /// </summary>
    public class Helper
    {
        public const char FileListSeparator = ',';

        const int BufferSizeDefault = 1024 * 1024;

        const string ModuleIsNotInstalledError = "{0} is not installed.";

        const string GetModuleInstanceByNameSql = @"
			select * from Modules m
			inner join ModuleDefinitions md on md.ModuleDefID = m.ModuleDefID
			inner join DesktopModules dm on dm.DesktopModuleID = md.DesktopModuleID
			inner join Tabs t on t.TabID = m.TabID
			where m.IsDeleted = 0
			and dm.FriendlyName = @FriendlyName
			and t.PortalID = @PortalID
		";

        static readonly char[] IncorrectDatabaseNameSymbols = {'.', '\'', '&', ' ', ',', ':', '+', '\\', '/',
                                               '*', '"', '{', '}', '[', ']', '(', ')', '%'};

        private const char ReplacementChar = '_';

        static Helper()
        {
            _nocLabel = "noc-label"; //get from settings

        }

        public static FileInfo[] GetFiles(string sourceDir, string includeMasks, string excludeMasks, bool recursive)
        {
            if (String.IsNullOrEmpty(sourceDir))
            {
                throw (new ArgumentNullException("sourceDir"));
            }

            DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceDir);
            if (!sourceDirInfo.Exists)
            {
                throw (new ArgumentException("sourceDir parameter must be a name of an existing directory."));
            }
            if (String.IsNullOrEmpty(includeMasks))
            {
                includeMasks = "*.*";
            }

            FileInfo[] includedFiles = GetFiles(sourceDirInfo, includeMasks, FileListSeparator);
            FileInfo[] componentFiles = null;
            if (!String.IsNullOrEmpty(excludeMasks))
            {
                FileInfo[] excludedFiles = GetFiles(sourceDirInfo, excludeMasks, FileListSeparator);
                ArrayList alExclusions = new ArrayList(excludedFiles.Length);
                if (excludedFiles.Length > 0)
                {
                    for (int i = 0; i < excludedFiles.Length; i++)
                    {
                        alExclusions.Add(excludedFiles[i].FullName);
                    }
                }
                ArrayList alComponents = new ArrayList();
                for (int i = 0; i < includedFiles.Length; i++)
                {
                    if (!alExclusions.Contains(includedFiles[i].FullName))
                    {
                        alComponents.Add(includedFiles[i]);
                    }
                }
                componentFiles = new FileInfo[alComponents.Count];
                alComponents.CopyTo(componentFiles);
            }
            else
            {
                componentFiles = includedFiles;
            }

            if (recursive)
            {
                ArrayList files = new ArrayList(componentFiles);
                DirectoryInfo[] directories = sourceDirInfo.GetDirectories();
                foreach (DirectoryInfo directory in directories)
                {
                    files.AddRange(GetFiles(directory.FullName, includeMasks, excludeMasks, true));
                }
                componentFiles = new FileInfo[files.Count];
                files.CopyTo(componentFiles, 0);
            }
            return componentFiles;
        }

        public static FileInfo[] GetFiles(DirectoryInfo sourceDirInfo, string patterns, char patternsSeparator)
        {
            if (sourceDirInfo == null)
            {
                throw (new ArgumentNullException("sourceDirInfo"));
            }
            if (String.IsNullOrEmpty(patterns))
            {
                throw (new ArgumentNullException("patterns"));
            }

            string[] patternsArray = patterns.Split(patternsSeparator);
            SortedList filesList = new SortedList();
            foreach (string pattern in patternsArray)
            {
                FileInfo[] patternFiles = sourceDirInfo.GetFiles(pattern);
                foreach (FileInfo file in patternFiles)
                {
                    filesList[file.FullName] = file;
                }
            }
            FileInfo[] files = new FileInfo[filesList.Count];
            filesList.Values.CopyTo(files, 0);
            return files;
        }

        const string USZipRegexPattern = @"^\d{5}$|^\d{5}-\d{4}$";
        const string CAZipRegexPattern = @"^[a-zA-Z0-9]{6}$|^[a-zA-Z0-9]{3} [a-zA-Z0-9]{3}$";
        static readonly string USAExtendedZipPattern = @"^\d{5}-\d{4}$";
        static readonly string CanadaExtendedZipPattern = @"^[A-Za-z0-9]{3,3}\s?[A-Za-z0-9]{3,3}$";
        static readonly Regex usaExtendedZipRegex = new Regex(USAExtendedZipPattern, RegexOptions.IgnoreCase);
        static readonly Regex canadaExtendedZipRegex = new Regex(CanadaExtendedZipPattern, RegexOptions.IgnoreCase);

        private static readonly string _nocLabel;

        public static string GetNocLabel()
        {
            return _nocLabel;
        }

        public static TimeSpan GetTime(Action action)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            action();

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

    }
}
