﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tharga.Toolkit.Console.Command.Base;

using SaintCoinach.Ex;

#pragma warning disable CS1998

namespace SaintCoinach.Cmd.Commands
{
    public class AllExdRawCommand : ActionCommandBase
    {
        private ARealmReversed _Realm;

        /// <summary>
        /// Setup the command
        /// </summary>
        /// <param name="realm"></param>
        public AllExdRawCommand(ARealmReversed realm)
            : base("allrawexd", "Export all data (default), or only specific data files, seperated by spaces; including all languages. No post-processing is applied to values.")
        {
            _Realm = realm;
        }

        /// <summary>
        /// Obtain game sheets from the game data
        /// </summary>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public override async Task<bool> InvokeAsync(string paramList)
        {
            var versionPath = _Realm.GameVersion;
            if (paramList?.Contains("/UseDefinitionVersion") ?? false)
                versionPath = _Realm.DefinitionVersion;

            AssignVariables(this, paramList);

            const string CsvFileFormat = "raw-exd-all/{0}{1}.csv";

            IEnumerable<string> filesToExport;

            // Gather files to export, may be split by params.
            if (string.IsNullOrWhiteSpace(paramList))
                filesToExport = _Realm.GameData.AvailableSheets;
            else
                filesToExport = paramList.Split(' ').Select(_ => _Realm.GameData.FixName(_));

            // Action counts
            var successCount = 0;
            var failCount = 0;
            var currentCount = 0;
            var total = filesToExport.Count();

            // Process game files.
            foreach (var name in filesToExport)
            {
                currentCount++;
                var sheet = _Realm.GameData.GetSheet(name);

                // Loop through all available languages
                foreach (var lang in sheet.Header.AvailableLanguages)
                {
                    var code = lang.GetCode();
                    if (code.Length > 0)
                        code = "." + code;

                    var target = new FileInfo(Path.Combine(versionPath, string.Format(CsvFileFormat, name, code)));

                    try
                    {
                        if (!target.Directory.Exists)
                            target.Directory.Create();

                        // Save
                        OutputInformation("[{0}/{1}] Processing: {2} - Language: {3}", currentCount, total, name, lang.GetSuffix());
                        ExdHelper.SaveAsCsv(sheet, lang, target.FullName, true);
                        ++successCount;
                    }
                    catch (Exception e)
                    {
                        OutputError("Export of {0} failed: {1}", name, e.Message);
                        try { if (target.Exists) { target.Delete(); } } catch { }
                        ++failCount;
                    }
                }
            }
            OutputInformation("{0} files exported, {1} failed", successCount, failCount);

            return true;
        }
    }
}