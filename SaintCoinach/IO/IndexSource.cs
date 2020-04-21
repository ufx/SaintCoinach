using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.IO {
    public partial class IndexSource : IPackSource {
        #region Fields

        private readonly Dictionary<uint, Directory> _Directories =
            new Dictionary<uint, Directory>();

        private readonly Dictionary<string, uint> _DirectoryPathMap = new Dictionary<string, uint>();

        private readonly Pack _Pack;

        private readonly Index _Index;

        #endregion

        #region Properties
        public Index Index { get { return _Index; } }
        public Pack Pack { get { return _Pack; } }
        #endregion

        #region Constructor
        internal IndexSource(Pack pack, Index index) {
            _Pack = pack;
            _Index = index;
        }
        #endregion

        #region Get

        public bool DirectoryExists(string path) {
            uint hash;/*
            if (!_DirectoryPathMap.TryGetValue(path, out hash))
                _DirectoryPathMap.Add(path, hash = Hash.Compute(path));*/
            hash = Hash.Compute(path);
            return DirectoryExists(hash);
        }

        public bool DirectoryExists(uint key) {
            return Index.Directories.ContainsKey(key);
        }

        public Directory GetDirectory(string path) {
            uint hash;/*
            if (!_DirectoryPathMap.TryGetValue(path, out hash))
                _DirectoryPathMap.Add(path, hash = Hash.Compute(path));*/
            hash = Hash.Compute(path);

            Directory dir = GetDirectory(hash);
            dir.Path = path;
            return dir;
        }

        public Directory GetDirectory(uint key) {
            if (_Directories.TryGetValue(key, out Directory directory))
                return directory;

            IndexDirectory index = Index.Directories[key];
            directory = new Directory(this.Pack, index);
            _Directories.Add(key, directory);
            return directory;
        }

        public bool TryGetDirectory(string path, out Directory directory) {
            uint hash;/*
            if (!_DirectoryPathMap.TryGetValue(path, out hash))
                _DirectoryPathMap.Add(path, hash = Hash.Compute(path));*/
            hash = Hash.Compute(path);

            bool result = TryGetDirectory(hash, out directory);
            if (result)
                directory.Path = path;
            return result;
        }

        public bool TryGetDirectory(uint key, out Directory directory) {
            if (_Directories.TryGetValue(key, out directory))
                return true;

            if (Index.Directories.TryGetValue(key, out IndexDirectory index)) {
                directory = new Directory(this.Pack, index);
                _Directories.Add(key, directory);
                return true;
            }

            directory = null;
            return false;
        }

        public bool FileExists(string path) {
            int lastSeperator = path.LastIndexOf('/');
            if (lastSeperator < 0)
                throw new ArgumentException();

            string dirPath = path.Substring(0, lastSeperator);
            string baseName = path.Substring(lastSeperator + 1);
            return TryGetDirectory(dirPath, out Directory dir) && dir.FileExists(baseName);
        }

        public File GetFile(string path) {
            int lastSeperator = path.LastIndexOf('/');
            if (lastSeperator < 0)
                throw new ArgumentException();

            string dirPath = path.Substring(0, lastSeperator);
            string baseName = path.Substring(lastSeperator + 1);
            Directory dir = GetDirectory(dirPath);
            return dir.GetFile(baseName);
        }

        public File GetFile(uint directoryKey, uint fileKey) {
            Directory dir = GetDirectory(directoryKey);
            return dir.GetFile(fileKey);
        }

        public bool TryGetFile(string path, out File file) {
            int lastSeperator = path.LastIndexOf('/');
            if (lastSeperator < 0) {
                file = null;
                return false;
            }

            string dirPath = path.Substring(0, lastSeperator);
            string baseName = path.Substring(lastSeperator + 1);
            if (TryGetDirectory(dirPath, out Directory dir))
                return dir.TryGetFile(baseName, out file);

            file = null;
            return false;
        }

        public bool TryGetFile(uint directoryKey, uint fileKey, out File file) {
            if (TryGetDirectory(directoryKey, out Directory dir))
                return dir.TryGetFile(fileKey, out file);

            file = null;
            return false;
        }

        #endregion
    }
}
