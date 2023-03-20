using System;
using System.Data;
using System.Globalization;
using Rhinox.Lightspeed;

namespace Rhinox.Magnus
{
	public class CsvLogLineHandler : FileLogLineHandler
	{
		protected override string Ext => ".csv";

		protected override void OnTableCreated()
		{
			base.OnTableCreated();
			
			// Write Column header
			WriteLine(DataTable.Columns.ToCsv());
		}

		protected override void OnRowAdded(DataRow row)
		{
			WriteLine(row.ToCsv());
		}
	}
}