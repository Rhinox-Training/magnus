using System;
using System.Collections.Generic;
using System.Data;

namespace Rhinox.Magnus
{
    public interface ILogLine
    {
        string[] Columns { get; }
        IReadOnlyCollection<ColumnData> Data { get; }

        void Append(ColumnData data);
        void Prepend(ColumnData data);
        void Insert(int index, ColumnData data);
    }

    public interface ILogLineHandler
    {
        void Initialize(params string[] identifiers);
        void Dispose();
        
        void Log(ILogLine line);
        
        TimeSpan TimeSinceStart { get; }
    }

    public abstract class BaseLogLineHandler : ILogLineHandler
    {
        /// ================================================================================================================
        /// PARAMS
        protected DateTime StartTime { get; set; }

        public virtual TimeSpan TimeSinceStart => DateTime.Now.Subtract(StartTime);

        public DataTable DataTable { get; set; }

        public bool Initialized { get; private set; }

        /// ================================================================================================================
        /// EVENTS
        public event Action LogStarted;
        
        public event Action TableCreated;

        public event Action LogEnded;

        /// ================================================================================================================
        /// METHODS
        public virtual void Initialize(params string[] identifiers)
        {
            StartTime = DateTime.Now;
            
            HandleStartLog(identifiers);

            Initialized = true;
            LogStarted?.Invoke();
        }

        protected abstract void HandleStartLog(params string[] identifiers);

        public virtual void Dispose()
        {
            LogEnded?.Invoke();
        }

        public virtual void Log(ILogLine line)
        {
            if (DataTable == null)
                CreateDataTable(line.Columns);
            
            DataRow row = DataTable.NewRow();

            foreach (ColumnData data in line.Data)
                row[data.Name] = data.Value;

            DataTable.Rows.Add(row);
            OnRowAdded(row);
        }
        
        protected virtual void CreateDataTable(string[] columns)
        {
            DataTable = new DataTable();
            foreach (var column in columns)
                DataTable.Columns.Add(column);
            
            OnTableCreated();
        }

        protected virtual void OnTableCreated()
        {
            TableCreated?.Invoke();
        }

        protected abstract void OnRowAdded(DataRow row);
    }
}