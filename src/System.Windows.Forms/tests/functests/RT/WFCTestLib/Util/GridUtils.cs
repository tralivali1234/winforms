#define DEBUG

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace WFCTestLib.Util
{
    //
    // Convenience class to programmatically build a table and make its
    // rows, columns, relations, etc. accessible to test classes.
    //
    public class GridUtils
    {
        public static readonly int Parent = 0;  // Parent table's array index
        public static readonly int Child1 = 1;  // 1st child table's array index

        private static RandomUtil ru = new RandomUtil();

        //
        // For all of these arrays, the first dimension of the array refers to which table the elements
        // belong to.  For example, tables[Parent] refers to the parent table, DataRow[Child1][0] refers
        // to the first row of the first child table.
        //
        private static DataTable[] tables;      // Related tables
        private static DataRow[][] rows;        // Rows for each table in tables[]
        private static DataSet dataSet;         // DataSet containing the table relations
        private static DataRelation[] relations;// Contains each relation in this dataset
        private static DataTable unrelatedTable;// An unrelated table with the same data as the parent table
        private static string relationName;     // The name of the relation between Parent and Child1

        // The names of the respective tables
        private static readonly String[] tableNames = new String[] {
        "My Shiny New Parent Table",
        "Related Child Table 1"
    };

        // The DataColumns contained in each table; see createColumns() method
        private static DataColumn[][] columns;

        // Contains an object of the type of each DataColumn in each Table.  This is used for testing setting
        // and getting cell values in the grid.
        private static object[][] columnObjects = new object[][] {
        new object[] {
            "Foobar: The Sample String!",
            Int32.MaxValue,
            Color.FromArgb(166, 202, 240),
            false,                          // TODO: Find out how to do Null for BoolColumn
            new DateTime(1999, 12, 31),
        },

        new object[] {
            "Bletch",                       // This value must exist in the parent table's primary key column
            AnchorStyles.Top,
            -4.27914e-5
        }
    };

        // String version of columnObjects for use with typing values into the grid via SendKeys
        private static string[][] columnStrings = new string[][] {
        new string[] {
            "Foobar: The Sample String!",
            "2147483647",
            // Use TypeDescriptor so the color string is formatted correctly for the current locale
            TypeDescriptor.GetConverter(typeof(Color)).ConvertToString(Color.FromArgb(166, 202, 240)),
            "{SPACE}",                              // This one is iffy.  Bool columns need to be special-cased in code.
            new DateTime(1999, 12, 31).ToString()   // Use ToString() so it's formatted correctly for the locale
        },

        new string[] {
            "Bletch",
            "Top",
            (-4.27914e-5).ToString()                // Use ToString() so it's formatted correctly for the locale
        }
    };

        //
        // Sample table data to use in DataGrid tests.
        // Note: The first column is set as the primary key, so the DataTable will be sorted.  The column
        //       must be in alphabetical order in order for the DataTable to have the same order of rows as
        //       this array.
        //
        private static readonly object[][][] tableData = new object[][][] {
        new object[][]
        {   // table 1
            new object[] { "Bar",                          -53,     Color.FromArgb(0, 255, 0),     false,        new DateTime(1901, 1, 1)      },
            new object[] { "Bletch",                       1234567, Color.FromArgb(0, 0, 255),     true,         new DateTime(1977, 4, 27)     },
            new object[] { "Foo",                          42,      Color.FromArgb(255, 0, 0),     true,         new DateTime(1999, 8, 23)   },
            new object[] { "Cowman the Mighty",            -999999, Color.FromArgb(100, 200, 240), DBNull.Value, new DateTime(2001, 1, 22)   },
            new object[] { "Moo, yeah, that's right, moo", -98765432,Color.FromArgb(255, 255, 200),false,        new DateTime(2450, 2, 28)   }
        },
        new object[][]
        {   // table 2
            new object[] { "Bar", AnchorStyles.None,  7.89012     },
            new object[] { "Bar", AnchorStyles.Left,  42.0 / 7    },
            new object[] { "Foo", AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,  3.145729    },
            new object[] { "Cowman the Mighty", AnchorStyles.Left | AnchorStyles.Right, -9.58e10    },
            new object[] { "Cowman the Mighty", AnchorStyles.Top | AnchorStyles.Right, 9.999999999  },
            new object[] { "Cowman the Mighty", AnchorStyles.Bottom | AnchorStyles.Top, 3.1e8    }
        }
    };

        private static readonly int[] keyColumns = new int[] { 0, 0 };

        //
        // Print Debug output to console.
        //
        static GridUtils()
        {
            //Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        }

        public static DataTable[] Tables
        {
            get
            {
                if (tables == null)
                    MakeDataSet();

                return tables;
            }
        }

        public static DataColumn[][] Columns
        {
            get
            {
                if (columns == null)
                    MakeDataSet();

                return columns;
            }
        }

        public static DataRow[][] Rows
        {
            get
            {
                if (rows == null)
                    MakeDataSet();

                return rows;
            }
        }

        public static object[][] ColumnObjects
        {
            get
            {
                if (columnObjects == null)
                    MakeDataSet();

                return columnObjects;
            }
        }

        public static string[][] ColumnStrings
        {
            get
            {
                if (columnStrings == null)
                    MakeDataSet();

                return columnStrings;
            }
        }

        public static int[] KeyColumns
        {
            get
            {
                if (columns == null)
                    MakeDataSet();

                return keyColumns;
            }
        }

        public static DataSet DataSet
        {
            get
            {
                if (dataSet == null)
                    MakeDataSet();

                return dataSet;
            }
        }

        public static DataRelation[] Relations
        {
            get
            {
                if (relations == null)
                    MakeDataSet();

                return relations;
            }
        }

        public static DataTable UnrelatedTable
        {
            get
            {
                if (unrelatedTable == null)
                    MakeUnrelatedTable();

                return unrelatedTable;
            }
        }

        public static string RelationName
        {
            get
            {
                if (relationName == null)
                    MakeDataSet();

                return relationName;
            }
        }

        //
        // Binds the grid to a given DataTable and sets the grid caption text
        // to the table name.
        //
        public static void BindToDataTable(DataGrid grid, DataTable table)
        {
            BindToDataTable(grid, table, true);
        }

        public static void BindToDataTable(DataGrid grid, DataTable table, bool createStyles)
        {
            grid.CaptionText = table.TableName;
            grid.CaptionVisible = true;
            grid.SetDataBinding(table, null);

            if (createStyles)
                CreateStyles(grid, table);

            Application.DoEvents();             // Let the grid paint
        }

        //
        // Binds the grid to a given DataSet.  Sets the caption text to the table name.
        //
        public static void BindToDataSet(DataGrid grid, DataSet dataSet, int tableIndex)
        {
            BindToDataSet(grid, dataSet, tableIndex, true);
        }

        public static void BindToDataSet(DataGrid grid, DataSet dataSet, int tableIndex, bool createStyles)
        {
            string name = dataSet.Tables[tableIndex].TableName;
            grid.CaptionText = name;
            grid.CaptionVisible = true;
            grid.SetDataBinding(dataSet, name);

            if (createStyles)
                CreateStyles(grid, dataSet);

            Application.DoEvents();             // Let the grid paint
        }

        //
        // Utility methods for getting the row or column count of a grid.
        //
        public static int GetRowCount(DataGrid grid)
        {
            return GridUtils.GetDataTable(grid).Rows.Count;
        }

        public static int GetColumnCount(DataGrid grid)
        {
            return GridUtils.GetDataTable(grid).Columns.Count;
        }

        //
        // Various utility methods for getting random cells, rows, and columns.
        //
        public static int GetRandomRow(DataGrid grid, int rangeFromTop, int rangeFromBottom)
        {
            return ru.GetRange(rangeFromTop, GetRowCount(grid) - 1 - rangeFromBottom);
        }

        public static int GetRandomRow(DataGrid grid)
        {
            return GetRandomRow(grid, 0, 0);
        }

        public static int GetRandomColumn(DataGrid grid, int rangeFromLeft, int rangeFromRight)
        {
            return ru.GetRange(rangeFromLeft, GetColumnCount(grid) - 1 - rangeFromRight);
        }

        public static int GetRandomColumn(DataGrid grid)
        {
            return GetRandomColumn(grid, 0, 0);
        }

        public static DataGridCell GetRandomCell(DataGrid grid)
        {
            return new DataGridCell(GetRandomRow(grid), GetRandomColumn(grid));
        }

        public static DataGridCell GetRandomInnerCell(DataGrid grid)
        {
            return new DataGridCell(GetRandomRow(grid, 1, 1), GetRandomColumn(grid, 1, 1));
        }

        //
        // Returns a string appropriate for use with SendKeys that will change a bool cell from
        // origState to newState.
        //
        public static string BoolCellToggleString(object origState, object newState)
        {
            /*const*/
            string NoSpaces = ""; // Can't declare const due to VSWhidbey #137842
            const string OneSpace = " ";
            const string TwoSpaces = "  ";

            if (origState.Equals(newState))
                return NoSpaces;

            if (origState.Equals(true))
            {
                if (newState.Equals(DBNull.Value))
                    return OneSpace;
                else if (newState.Equals(false))
                    return TwoSpaces;

                throw new ArgumentException("newState was invalid type: " + newState.GetType());
            }

            if (origState.Equals(DBNull.Value))
            {
                if (newState.Equals(false))
                    return OneSpace;
                else if (newState.Equals(true))
                    return TwoSpaces;

                throw new ArgumentException("newState was invalid type: " + newState.GetType());
            }

            if (origState.Equals(false))
            {
                if (newState.Equals(true))
                    return OneSpace;
                else if (newState.Equals(DBNull.Value))
                    return TwoSpaces;

                throw new ArgumentException("newState was invalid type: " + newState.GetType());
            }

            throw new ArgumentException("origState was invalid type: " + origState.GetType());
        }

        //
        // Creates TableStyles and ColumnStyles for the given DataTable or DataSet.
        // The grid doesn't need to be bound to the DataTable or DataSet.
        //
        public static void CreateStyles(DataGrid grid, DataSet dataSet)
        {
            foreach (DataTable table in dataSet.Tables)
                CreateStyles(grid, table);
        }

        public static void CreateStyles(DataGrid grid, DataTable table)
        {
            GetTableStyle(grid, table);

            foreach (DataColumn col in table.Columns)
                GetColumnStyle(grid, col);
        }

        //
        // Utility method for getting the DataTable bound to a DataGrid.
        //
        // TODO: Find out ALL legal DataSource types
        //
        public static DataTable GetDataTable(DataGrid g)
        {
            if (g.DataSource is DataView)
                return ((DataView)g.DataSource).Table;
            else if (g.DataSource is DataTable)
                return (DataTable)g.DataSource;
            else if (g.DataSource is DataViewManager || g.DataSource is DataSet)
            {
                DataSet ds = g.DataSource is DataSet ? (DataSet)g.DataSource : ((DataViewManager)g.DataSource).DataSet;

                // A slightly hackish way of getting the table if it's the child table and the
                // grid is bound to the DataSet.  If it's bound to a DataSet, DataMember might
                // be something like "Parent Table Name.RelationName".
                //
                // TODO: need to make this a loop if we ever add more tables and relations.
                //
                if (ds.Relations.Count != 0 && g.DataMember.IndexOf(ds.Relations[0].RelationName) != -1)
                    return ds.Relations[0].ChildTable;
                else
                    return ds.Tables[g.DataMember];
            }
            else
                throw new Exception("DataSource is an illegal type");
        }

        //
        // In Beta 2, the CurrentTable property was yanked.  This method can substitute
        // for it.  It returns the TableStyle for the currently displayed table.
        //
        public static DataGridTableStyle GetCurrentTableStyle(DataGrid g)
        {
            return GetTableStyle(g, GetDataTable(g));
        }

        public static DataGridTableStyle GetTableStyle(DataGrid g, DataTable table)
        {
            DataGridTableStyle tableStyle = g.TableStyles[table.TableName];

            if (tableStyle == null)
            {  // need to create the table style
                Debug.WriteLine("Creating new table for " + table.TableName);

                tableStyle = new DataGridTableStyle();
                tableStyle.MappingName = table.TableName;
                g.TableStyles.Add(tableStyle);
            }

            return tableStyle;
        }

        //
        // In Beta 1 GridColumns was automatically populated based on the currently
        // displayed table.  In Beta 2, the collection only contains columns which
        // the user adds.  The default columns do not appear in the collection.
        // Thus, we're going to have to create our own.
        //
        // This method will check the column collection of the current table for the
        // proper column, and if it's not there, create one.
        //
        public static DataGridColumnStyle GetColumnStyle(DataGrid g, int col)
        {
            DataTable table = GetDataTable(g);
            return GetColumnStyle(g, table.Columns[col]);
        }

        public static DataGridColumnStyle GetColumnStyle(DataGrid g, DataColumn column)
        {
            DataGridTableStyle tableStyle = GetTableStyle(g, column.Table);
            DataGridColumnStyle columnStyle;

            columnStyle = tableStyle.GridColumnStyles[column.ColumnName];

            if (columnStyle == null)
            {     // need to create new column
                Debug.WriteLine("Creating new column for " + column.ColumnName);

                if (column.DataType == typeof(bool))
                    columnStyle = new DataGridBoolColumn();
                else
                    columnStyle = new DataGridTextBoxColumn();

                columnStyle.MappingName = column.ColumnName;
                columnStyle.HeaderText = column.ColumnName;
                tableStyle.GridColumnStyles.Add(columnStyle);
            }

            return columnStyle;
        }

        //
        // Returns the type of column that is supposed to be instatiated by the DataGrid for a
        // DataColumn of the given type.
        //
        public static Type GetGridColumnType(Type dataType)
        {
            if (dataType == typeof(bool))
                return typeof(DataGridBoolColumn);

            return typeof(DataGridTextBoxColumn);
        }

        //
        // Resets all the tables, columns, etc., to their original state.  Use this method
        // after making changes to any of the variables in this class
        //
        public static void ResetDataSet()
        {
            MakeDataSet();
        }

        public static void ResetUnrelatedTable()
        {
            MakeUnrelatedTable();
        }

        //
        // Create a big table
        //
        public static DataTable MakeHugeTable(int numColumns, int numRows)
        {
            DataTable table = new DataTable("One Big Table!!");

            for (int c = 0; c < numColumns; ++c)
            {
                if (c % 9 == 0)
                    table.Columns.Add(new DataColumn("StringCol" + (c / 9).ToString(), typeof(String)));
                else if (c % 8 == 0)
                    table.Columns.Add(new DataColumn("ByteCol" + (c / 8).ToString(), typeof(Byte)));
                else if (c % 7 == 0)
                    table.Columns.Add(new DataColumn("Int16Col" + (c / 7).ToString(), typeof(Int16)));
                else if (c % 6 == 0)
                    table.Columns.Add(new DataColumn("Int64Col" + (c / 6).ToString(), typeof(Int64)));
                else if (c % 5 == 0)
                    table.Columns.Add(new DataColumn("BooleanCol" + (c / 5).ToString(), typeof(Boolean)));
                else if (c % 4 == 0)
                    table.Columns.Add(new DataColumn("TimeSpanCol" + (c / 4).ToString(), typeof(TimeSpan)));
                else if (c % 3 == 0)
                    table.Columns.Add(new DataColumn("DateTimeCol" + (c / 3).ToString(), typeof(DateTime)));
                else if (c % 2 == 0)
                    table.Columns.Add(new DataColumn("DecimalCol" + (c / 2).ToString(), typeof(Decimal)));
                else if (c % 1 == 0)
                    table.Columns.Add(new DataColumn("Int32Col" + (c / 1).ToString(), typeof(Int32)));
            }

            long startTicks = new DateTime(1977, 4, 27).Ticks;

            for (int r = 0; r < numRows; ++r)
            {
                DataRow row = table.NewRow();
                for (int c = 0; c < numColumns; ++c)
                {
                    if (c % 9 == 0) row[c] = "String" + r.ToString();
                    else if (c % 8 == 0) row[c] = r % Byte.MaxValue;
                    else if (c % 7 == 0) row[c] = r * 5 % Int16.MaxValue;
                    else if (c % 6 == 0) row[c] = (Int64)r * 100 % Int64.MaxValue;
                    else if (c % 5 == 0) row[c] = r % 3 == 0 ? true : (r % 3 == 1 ? (object)false : DBNull.Value);
                    else if (c % 4 == 0) row[c] = new TimeSpan(r * TimeSpan.TicksPerSecond);
                    else if (c % 3 == 0) row[c] = new DateTime(r * TimeSpan.TicksPerDay * 8 + startTicks);
                    else if (c % 2 == 0) row[c] = r / 10.0;
                    else if (c % 1 == 0) row[c] = r * 10 % Int32.MaxValue;
                }

                table.Rows.Add(row);
            }

            return table;
        }

        public static DataTable MakeHugeTable()
        {
            return MakeHugeTable(25, 2000);
        }

        //
        // Copies data from a variant array to a DataRow
        //
        private static void CopyToDataRow(object[] source, DataRow dest)
        {
            int length = source.Length;

            for (int i = 0; i < length; ++i)
            {
                dest[i] = source[i];
            }
        }

        //
        // Creates a new unrelated table (uses the same data as the parent table)
        //
        private static void MakeUnrelatedTable()
        {
            int numCols = Columns[Parent].Length;
            DataColumn[][] cols = createColumns();
            unrelatedTable = new DataTable("My Shiny New Unrelated Table");

            for (int c = 0; c < numCols; ++c)
                unrelatedTable.Columns.Add(cols[Parent][c]);

            int numRows = tableData[Parent].Length;

            for (int r = 0; r < numRows; ++r)
            {
                DataRow row = unrelatedTable.NewRow();
                CopyToDataRow(tableData[Parent][r], row);
                unrelatedTable.Rows.Add(row);
            }
        }

        //
        // Need this method so we don't ever reuse an old DataColumn when recreating the table
        //
        private static DataColumn[][] createColumns()
        {
            return new DataColumn[][] {
            new DataColumn[] {  // Table 1's columns
                new DataColumn("EditColumn",    typeof(String)),
                new DataColumn("NumericColumn", typeof(Int32)),
                new DataColumn("ValueColumn",   typeof(Color)),
                new DataColumn("BooleanColumn", typeof(bool)),
                new DataColumn("DateTimeColumn",typeof(DateTime))
            },
            new DataColumn[] {  // Table 2's Columns
                new DataColumn("Name",      typeof(String)),
                new DataColumn("Anchor",    typeof(AnchorStyles)),
                new DataColumn("Decimal",   typeof(Decimal))
            }
        };
        }

        //
        // Creates a DataSet containing two related tables.
        //
        private static DataSet MakeDataSet()
        {
            int numTables = tableData.GetLength(0);
            tables = new DataTable[numTables];
            columns = createColumns();
            dataSet = new DataSet();

            for (int t = 0; t < numTables; ++t)
            {
                int numCols = columns[t].Length;
                tables[t] = new DataTable(tableNames[t]);
                dataSet.Tables.Add(tables[t]);

                for (int c = 0; c < numCols; ++c)
                {
                    tables[t].Columns.Add(columns[t][c]);
                }
            }

            // Transfer the data from the tableData array to the DataTables
            rows = new DataRow[tables.Length][];

            for (int t = 0; t < numTables; ++t)
            {
                int numRows = tableData[t].Length;
                rows[t] = new DataRow[numRows];

                for (int r = 0; r < numRows; ++r)
                {
                    rows[t][r] = tables[t].NewRow();
                    CopyToDataRow(tableData[t][r], rows[t][r]);
                    tables[t].Rows.Add(rows[t][r]);
                }
            }

            // TODO: Find a way to do this nice and tidy with a loop and some sort of data structure to
            //       hold the relationships
            relations = new DataRelation[1];
            relationName = "Foo Relation 1";
            relations[0] = new DataRelation(relationName, tables[0].Columns[0], tables[1].Columns[0]);
            tables[0].PrimaryKey = new DataColumn[] { tables[0].Columns[0] };
            dataSet.Relations.Add(relations[0]);

            return dataSet;
        }
    }
}



