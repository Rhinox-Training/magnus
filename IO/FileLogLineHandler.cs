using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Rhinox.Magnus
{
    public abstract class FileLogLineHandler : BaseLogLineHandler
    {
        public const string RootPath = "./Logs/";
        private string _path;

        public string OutputPath
        {
            get
            {
                if (!Initialized)
                    return null;
                return _path;
            }
        }

        /// <summary>
        /// Extension of the file (With the .); i.e. ".csv"
        /// </summary>
        protected abstract string Ext { get; }

        protected override void HandleStartLog(params string[] identifiers)
        {
            // Ensure the directory is created
            var dir = Path.GetDirectoryName(_path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public override void Initialize(params string[] identifiers)
        {
            SetPath(DateTime.Now, identifiers);
            base.Initialize(identifiers);
        }
        
        protected virtual void SetPath(DateTime timeStamp, params string[] identifiers)
        {
            var safeIdentifiers = identifiers.Select(x => GetAsSafeString(x, "Unknown")).ToArray();

            var subPath = Path.Combine(safeIdentifiers);
            
            _path = Path.Combine(RootPath, subPath, $"{string.Join("_", safeIdentifiers)}_{timeStamp:yyyy-MM-dd_HHmmss}{Ext}");
            _path = Path.GetFullPath(_path);

            Debug.Log($"Logging to: {_path}");
        }

        protected void WriteLine(string line)
        {
            if (string.IsNullOrWhiteSpace(_path))
                return;
            
            File.AppendAllText(_path, line + Environment.NewLine, Encoding.UTF8);
        }
        
        protected string GetAsSafeString(string text, string defaultIfNull = "")
        {
            text = text?.Trim(Path.GetInvalidFileNameChars());
            if (string.IsNullOrEmpty(text))
                text = defaultIfNull;

            return text;
        }
    }
}