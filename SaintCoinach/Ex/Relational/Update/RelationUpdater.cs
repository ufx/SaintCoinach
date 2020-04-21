using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using SaintCoinach.Ex.Relational.Definition;
using SaintCoinach.Ex.Relational.Update.Changes;
using SaintCoinach.IO;


namespace SaintCoinach.Ex.Relational.Update {
    public class RelationUpdater {
        #region Static

        private const Language UsedLanguage = Language.Japanese;

        #endregion

        #region Fields

        private readonly RelationalExCollection _Previous;
        private readonly IProgress<UpdateProgress> _Progress;
        private readonly RelationalExCollection _Updated;

        #endregion

        #region Properties

        public RelationDefinition Previous { get; private set; }
        public RelationDefinition Updated { get; private set; }

        #endregion

        #region Constructors

        public RelationUpdater(PackCollection previousPacks,
                               RelationDefinition previousDefinition,
                               PackCollection updatedPacks,
                               string updatedVersion,
                               IProgress<UpdateProgress> progress) {
            _Progress = progress ?? new NullProgress();

            _Previous = new RelationalExCollection(previousPacks);
            Previous = previousDefinition;

            _Updated = new RelationalExCollection(updatedPacks);
            Updated = new RelationDefinition {
                Version = updatedVersion
            };

            _Previous.ActiveLanguage = UsedLanguage;
            _Updated.ActiveLanguage = UsedLanguage;
        }

        #endregion

        #region Update

        public IEnumerable<IChange> Update(bool detectDataChanges) {
            ConcurrentBag<IChange> changes = new ConcurrentBag<IChange>();

            UpdateProgress progress = new UpdateProgress {
                CurrentOperation = "Structure",
                TotalSteps = (detectDataChanges ? 2 : 1) * Previous.SheetDefinitions.Count
            };

            object sheetLock = new object();

            Parallel.ForEach(Previous.SheetDefinitions, prevSheetDef =>
            {
                progress.CurrentFile = prevSheetDef.Name;
                _Progress.Report(progress);

                if (!_Updated.SheetExists(prevSheetDef.Name))
                {
                    changes.Add(new SheetRemoved(prevSheetDef.Name));
                    return;
                }

                IRelationalSheet prevSheet, updatedSheet;
                SheetDefinition updatedSheetDef;
                lock (sheetLock)
                {
                    prevSheet = _Previous.GetSheet(prevSheetDef.Name);
                    updatedSheet = _Updated.GetSheet(prevSheetDef.Name);
                    updatedSheetDef = Updated.GetOrCreateSheet(prevSheetDef.Name);
                }

                SheetUpdater sheetUpdater = new SheetUpdater(prevSheet, prevSheetDef, updatedSheet, updatedSheetDef);
                IChange[] sheetUpdates = MemoryGuard(() => sheetUpdater.Update().ToArray());
                foreach (IChange change in sheetUpdates)
                    changes.Add(change);

                progress.IncrementStep();
            });

            Updated.Compile();

            if (detectDataChanges) {
                progress.CurrentOperation = "Data";

                Parallel.ForEach(Previous.SheetDefinitions, prevSheetDef =>
                {
                    progress.CurrentFile = prevSheetDef.Name;
                    _Progress.Report(progress);

                    if (!_Updated.SheetExists(prevSheetDef.Name))
                        return;

                    IRelationalSheet prevSheet, updatedSheet;
                    SheetDefinition updatedSheetDef;
                    lock (sheetLock)
                    {
                        prevSheet = _Previous.GetSheet(prevSheetDef.Name);
                        updatedSheet = _Updated.GetSheet(prevSheetDef.Name);
                        updatedSheetDef = Updated.GetOrCreateSheet(prevSheetDef.Name);
                    }

                    SheetComparer sheetComparer = new SheetComparer(prevSheet, prevSheetDef, updatedSheet, updatedSheetDef);
                    IChange[] sheetChanges = MemoryGuard(() => sheetComparer.Compare().ToArray());
                    foreach (IChange change in sheetChanges)
                        changes.Add(change);

                    progress.IncrementStep();
                });
            }

            progress.CurrentOperation = "Finished";
            progress.CurrentFile = null;
            _Progress.Report(progress);

            return changes;
        }

        private static T MemoryGuard<T>(Func<T> func) {
            for (int i = 0; i < 5; i++) {
                try {
                    return func();
                }
                catch (OutOfMemoryException) {
                    // Give another task time to progress and free memory.
                    System.Threading.Thread.Sleep(300);
                    GC.Collect();
                }
            }

            throw new OutOfMemoryException("Could not execute function after 5 attempts.");
        }

        #endregion

        private class NullProgress : IProgress<UpdateProgress> {
            #region IProgress<UpdateProgress> Members

            public void Report(UpdateProgress value) { }

            #endregion
        }
    }
}
