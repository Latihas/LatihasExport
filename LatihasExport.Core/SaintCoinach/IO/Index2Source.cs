﻿using System;
using System.Collections.Generic;

namespace SaintCoinach.IO {
    public partial class Index2Source : IPackSource {
        #region Fields

        private readonly Dictionary<uint, WeakReference<File>> _Files =
            new Dictionary<uint, WeakReference<File>>();

        private readonly Dictionary<string, uint> _FilePathMap = new Dictionary<string, uint>();

        private readonly Pack _Pack;

        private readonly Index2 _Index;

        #endregion

        #region Properties
        public Index2 Index { get { return _Index; } }
        public Pack Pack { get { return _Pack; } }
        #endregion

        #region Constructor
        internal Index2Source(Pack pack, Index2 index) {
            _Pack = pack;
            _Index = index;
        }
        #endregion

        #region IPackSource Members

        public bool FileExists(string path) {
            uint hash;/*
            if (!_FilePathMap.TryGetValue(path, out hash))
                _FilePathMap.Add(path, hash = Hash.Compute(path));*/
            hash = Hash.Compute(path);
            return FileExists(hash);
        }

        public bool FileExists(uint hash) {
            return Index.Files.ContainsKey(hash);
        }

        public bool TryGetFile(string path, out File value) {
            uint hash;/*
            if (!_FilePathMap.TryGetValue(path, out hash))
                _FilePathMap.Add(path, hash = Hash.Compute(path));*/
            hash = Hash.Compute(path);
            var result = TryGetFile(hash, out value);
            if (result)
                value.Path = path;
            return result;
        }

        public bool TryGetFile(uint hash, out File value) {
            if (_Files.TryGetValue(hash, out var fileRef) && fileRef.TryGetTarget(out value))
                return true;

            if (Index.Files.TryGetValue(hash, out var index)) {
                value = FileFactory.Get(Pack, index);
                if (_Files.ContainsKey(hash))
                    _Files[hash].SetTarget(value);
                else
                    _Files.Add(hash, new WeakReference<File>(value));
                return true;
            }

            value = null;
            return false;
        }

        public File GetFile(string path) {
            uint hash;/*
            if (!_FilePathMap.TryGetValue(path, out hash))
                _FilePathMap.Add(path, hash = Hash.Compute(path));*/
            hash = Hash.Compute(path);
            var f = GetFile(hash);
            f.Path = path;
            return f;
        }

        public File GetFile(uint hash) {
            if (_Files.TryGetValue(hash, out var fileRef) && fileRef.TryGetTarget(out var file))
                return file;

            var index = Index.Files[hash];
            file = FileFactory.Get(Pack, index);
            if (_Files.ContainsKey(hash))
                _Files[hash].SetTarget(file);
            else
                _Files.Add(hash, new WeakReference<File>(file));
            return file;
        }

        #endregion
    }
}
