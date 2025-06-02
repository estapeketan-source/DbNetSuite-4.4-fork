using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

///////////////////////////////////////////////
namespace DbNetLink.DbNetSuite
///////////////////////////////////////////////
{
    ///////////////////////////////////////////////
    internal class FileSearchResultsDialog
    ///////////////////////////////////////////////
    {
        internal DbNetFile ParentControl;
        String ContentSearchToken = "";
        Dictionary<string, object> Parameters = null;
        bool IncludeSubFolders = true;
        bool SearchFilesOnly = true;

        Hashtable IndexingServiceFileSystemColumn = new Hashtable();
        Hashtable WindowsSearchFileSystemColumn = new Hashtable();


        ///////////////////////////////////////////////
        public FileSearchResultsDialog(DbNetFile PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;

            IndexingServiceFileSystemColumn[DbNetFile.ColumnTypes.DateCreated] = "Created";
            IndexingServiceFileSystemColumn[DbNetFile.ColumnTypes.DateLastAccessed] = "Access";
            IndexingServiceFileSystemColumn[DbNetFile.ColumnTypes.DateLastModified] = "Written";
            IndexingServiceFileSystemColumn[DbNetFile.ColumnTypes.Size] = "Size";
            IndexingServiceFileSystemColumn[DbNetFile.ColumnTypes.Name] = "FileName";

            WindowsSearchFileSystemColumn[DbNetFile.ColumnTypes.DateCreated] = "System.DateCreated";
            WindowsSearchFileSystemColumn[DbNetFile.ColumnTypes.DateLastAccessed] = "System.DateAccessed";
            WindowsSearchFileSystemColumn[DbNetFile.ColumnTypes.DateLastModified] = "System.DateModified";
            WindowsSearchFileSystemColumn[DbNetFile.ColumnTypes.Size] = "System.Size";
            WindowsSearchFileSystemColumn[DbNetFile.ColumnTypes.Name] = "System.FileName";
        }

        ///////////////////////////////////////////////
        public Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.ToolTip = Translate("FileSearchResults");

            TableRow TR = new TableRow();
            T.Controls.Add(TR);
            TableCell TC = new TableCell();
            TR.Controls.Add(TC);
            Panel P = new Panel();
            P.CssClass = "dbnetfile results-panel";
            TC.Controls.Add(P);

            P = new Panel();
            P.ID = "wait_image";
            System.Web.UI.WebControls.Image I = new System.Web.UI.WebControls.Image();
            I.ImageUrl = ParentControl.GetImageUrl("wait.gif");
            I.AlternateText = "Waiting...";

            P.Style.Add(HtmlTextWriterStyle.Position, "absolute");
            P.Style.Add(HtmlTextWriterStyle.Display, "none");
            P.Style.Add(HtmlTextWriterStyle.Padding, "2px");
            P.BorderColor = System.Drawing.Color.DimGray;
            P.BorderStyle = BorderStyle.Solid;
            P.BorderWidth = Unit.Pixel(1);
            P.BackColor = System.Drawing.Color.LightYellow;

            P.Controls.Add(I);
            TC.Controls.Add(P);

            ParentControl.AddMessageRow(T);

            return T;
        }

        ///////////////////////////////////////////////
        internal void RunSearch()
        ///////////////////////////////////////////////
        {
            this.Parameters = (Dictionary<string, object>)ParentControl.Req["searchCriteria"];

            foreach (string K in Parameters.Keys)
            {
                string TokenValue = Parameters[K].ToString();

                if (TokenValue == "")
                    continue;
                switch (K)
                {
                    case "include_sub_folders":
                        IncludeSubFolders = (TokenValue.ToLower() == Boolean.TrueString.ToLower());
                        break;
                    case "search_files_only":
                        SearchFilesOnly = (TokenValue.ToLower() == Boolean.TrueString.ToLower());
                        break;
                    case "content_search_token":
                        ContentSearchToken = TokenValue;
                        break;
                }
            }

            Table T = (Table)BuildGrid();

            StringWriter SW = new StringWriter();
            HtmlTextWriter HTW = new HtmlTextWriter(SW);
            T.RenderControl(HTW);

            ParentControl.Resp["html"] = SW.ToString();
        }


        ///////////////////////////////////////////////
        private Control BuildGrid()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.ID = this.ParentControl.AssignID("search_results_table");
            T.CssClass = "filee-search-results-table";
            T.Controls.Add(ParentControl.BuildHeaderRow(true));

            string CurrentFolder = Parameters["currentFolder"].ToString();

            DirectoryInfo Dir = ParentControl.InitialiseFileSelection(CurrentFolder);

            switch (ParentControl.SearchMode)
            {
                case DbNetFile.SearchModes.FileSystem:
                    BuildSearchRecordSet(Dir);
                    break;
                case DbNetFile.SearchModes.IndexingService:
                    BuildIndexingServiceRecordSet(Dir);
                    break;
                case DbNetFile.SearchModes.WindowsSearch:
                    BuildWindowsSearchRecordSet(Dir);
                    break;
            }


            string Sort = "recordtype";
            
            if ( this.Parameters.ContainsKey("orderBy") )
            {
                Sort += this.Parameters["orderBy"].ToString().Contains(" desc") ? " desc" : " asc";

                if (this.Parameters["orderBy"].ToString() != "")
                    Sort += ", " + this.Parameters["orderBy"].ToString();
            }
            else
                Sort += ",name";

            string Filter = "";

            DataView FileView = new DataView(ParentControl.FileTable, Filter, Sort, DataViewRowState.CurrentRows);

            int RowIndex = 1;

            foreach (DataRowView Row in FileView)
            {
                if (RowIndex > ParentControl.MaxSearchMatches)
                    break;

                T.Controls.Add(ParentControl.BuildDataRow(Row, RowIndex, true));
                RowIndex++;
            }

            return T;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void BuildSearchRecordSet(DirectoryInfo ParentFolder)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            DirectoryInfo[] Folders = ParentFolder.GetDirectories();

            foreach (DirectoryInfo Folder in Folders)
            {
                if (!SearchFilesOnly)
                    if (FileSearchMatch(Folder))
                        if (ParentControl.AddItemToRecordSet(Folder) == null)
                            break;

                if (this.IncludeSubFolders)
                    BuildSearchRecordSet(Folder);
            }

            FileInfo[] Files = ParentFolder.GetFiles();

            foreach (FileInfo File in Files)
            {
                if (FileSearchMatch(File))
                    if (ParentControl.AddItemToRecordSet(File) == null)
                        break;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void BuildIndexingServiceRecordSet(DirectoryInfo ParentFolder)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            IDataReader Reader = null;
            IDbConnection Connection = null;

            string WhereClause = BuildSearchFilter();

            string Sql = "select  Path, " + String.Join(",", (string[])ParentControl.IndexingServiceColumns.ToArray(typeof(string))) + " ";
            Sql += "from scope('" + ((this.IncludeSubFolders) ? "deep" : "shallow") + " traversal of \"" + ParentFolder.FullName + "\" ')";

            if (WhereClause != "")
                Sql += " where " + WhereClause + " order by rank desc";

            try
            {
                Connection = new OleDbConnection("Provider=\"MSIDXS\";Data Source=\"" + ParentControl.IndexingServiceCatalog + "\";");
                Connection.Open();
                IDbCommand Command = Connection.CreateCommand();
                Command.CommandText = Sql;
                Reader = Command.ExecuteReader();
            }
            catch (Exception E)
            {
                this.ParentControl.ThrowException(E.Message + "<p><b>" + Sql + "</b>");
                return;
            }

            foreach (string Column in ParentControl.IndexingServiceColumns)
                ParentControl.FileTable.Columns.Add(Column, Reader.GetFieldType(Reader.GetOrdinal(Column)));

            while (Reader.Read())
            {
                FileInfo FI = new FileInfo(Reader.GetValue(0).ToString());

                DataRow Row = ParentControl.AddItemToRecordSet(FI);

                if (Row == null)
                    break;

                foreach (string Column in ParentControl.IndexingServiceColumns)
                    Row[Column] = Reader.GetValue(Reader.GetOrdinal(Column));
            }

            Reader.Close();
            Connection.Close();
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void BuildWindowsSearchRecordSet(DirectoryInfo ParentFolder)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            IDataReader Reader = null;
            IDbConnection Connection = null;

            string WhereClause = BuildSearchFilter();

            string Sql = "select System.ItemType, System.ItemUrl, " + String.Join(",", (string[])ParentControl.WindowsSearchColumns.ToArray(typeof(string))) + " ";
            Sql += "from SYSTEMINDEX";

            if (this.IncludeSubFolders)
                Sql += " where System.ItemPathDisplay like '" + ParentFolder.FullName + @"%'";
            else
                Sql += " where System.ItemFolderPathDisplay = '" + ParentFolder.FullName + @"'";

            if (this.SearchFilesOnly)
                Sql += " and System.ItemType <> 'Directory'";

            if (WhereClause != "")
                Sql += " and " + WhereClause;


            Sql += " order by " + ParentControl.WindowsSearchColumns[0].ToString();

            try
            {
                Connection = new OleDbConnection(ParentControl.WindowsSearchConnectionString);
                Connection.Open();
                IDbCommand Command = Connection.CreateCommand();
                Command.CommandText = Sql;
                Reader = Command.ExecuteReader();
            }
            catch (Exception E)
            {
                this.ParentControl.ThrowException(E.Message + "<p><b>" + Sql + "</b>");
                return;
            }

            foreach (string Column in ParentControl.WindowsSearchColumns)
                ParentControl.FileTable.Columns.Add(Column, Reader.GetFieldType(Reader.GetOrdinal(Column)));

            while (Reader.Read())
            {
                FileSystemInfo FI;

                string Path = Reader.GetValue(1).ToString().Replace("file:", "");

                if (Reader.GetValue(0).ToString() == "Directory")
                    FI = new DirectoryInfo(Path);
                else
                    FI = new FileInfo(Path);
                DataRow Row = ParentControl.AddItemToRecordSet(FI);

                if (Row == null)
                    break;

                foreach (string Column in ParentControl.WindowsSearchColumns)
                    Row[Column] = Reader.GetValue(Reader.GetOrdinal(Column));
            }

            Reader.Close();
            Connection.Close();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        string BuildSearchFilter()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            ArrayList FilterItems = new ArrayList();
            foreach (FileColumn FC in ParentControl.Columns)
            {
                string Filter = "";

                if (FC.Search == false)
                    continue;

                string ColumnName = FC.ColumnName;

                switch (FC.ColumnType)
                {
                    case DbNetFile.ColumnTypes.DateCreated:
                    case DbNetFile.ColumnTypes.DateLastAccessed:
                    case DbNetFile.ColumnTypes.DateLastModified:
                    case DbNetFile.ColumnTypes.Size:
                    case DbNetFile.ColumnTypes.Name:
                        if (ParentControl.SearchMode == DbNetFile.SearchModes.IndexingService)
                            ColumnName = IndexingServiceFileSystemColumn[FC.ColumnType].ToString();
                        else
                            ColumnName = WindowsSearchFileSystemColumn[FC.ColumnType].ToString();
                        break;
                    case DbNetFile.ColumnTypes.IndexingService:
                    case DbNetFile.ColumnTypes.WindowsSearch:
                        break;
                    default:
                        continue;
                }

                string[] Tokens = { "", "" };
                string Operator = Parameters[FC.ColumnID + "_search_operator"].ToString();

                Tokens[0] = Parameters[FC.ColumnID + "_search_token_1"].ToString();

                if (Tokens[0] == "")
                    continue;

                if (Operator == "between")
                    Tokens[1] = Parameters[FC.ColumnID + "_search_token_2"].ToString();

                switch (FC.ColumnDataType)
                {
                    case "String":
                        Tokens[0] = Regex.Replace(Tokens[0], @"'", "''");

                        switch (Operator)
                        {
                            case "startswith":
                                Filter = ColumnName + " like '" + Tokens[0] + "%'";
                                break;
                            case "endswith":
                                Filter = ColumnName + " like '%" + Tokens[0] + "'";
                                break;
                            case "equalto":
                                Filter = ColumnName + " = '" + Tokens[0] + "'";
                                break;
                            default:
                                Filter = ColumnName + " like '%" + Tokens[0] + "%'";
                                break;
                        }
                        break;
                    case "Int32":
                        switch (Operator)
                        {
                            case "lessthan":
                                Filter = ColumnName + " < " + Tokens[0];
                                break;
                            case "greaterthan":
                                Filter = ColumnName + " > " + Tokens[0];
                                break;
                            case "equalto":
                                Filter = ColumnName + " = " + Tokens[0];
                                break;
                            case "between":
                                Filter = "(" + ColumnName + " >= " + Tokens[0] + " and " + ColumnName + "<=" + Tokens[1] + ")";
                                break;
                        }
                        break;
                    case "DateTime":
                        switch (Operator)
                        {
                            case "lessthan":
                                Filter = ColumnName + " < '" + SqlDate(Tokens[0], false) + "'";
                                break;
                            case "greaterthan":
                                Filter = ColumnName + " > '" + SqlDate(Tokens[0], true) + "'";
                                break;
                            case "equalto":
                                Filter = "(" + ColumnName + " >= '" + SqlDate(Tokens[0], false) + "' and " + ColumnName + " <= '" + SqlDate(Tokens[0], true) + "')";
                                break;
                            case "between":
                                Filter = "(" + ColumnName + " >= '" + SqlDate(Tokens[0], false) + "' and " + ColumnName + " <= '" + SqlDate(Tokens[1], true) + "')";
                                break;
                        }
                        break;
                }

                if (Filter != "")
                    FilterItems.Add(Filter);
            }

            if (this.ContentSearchToken != "")
                FilterItems.Add("contains (Contents, '" + this.ContentSearchToken + "')");

            return String.Join(" and ", (string[])FilterItems.ToArray(typeof(string)));
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        string SqlDate(string DateToken, bool EndOfDay)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            DateToken = DateTime.Parse(DateToken).ToString("u").Substring(0, 10);
            DateToken += (EndOfDay) ? " 23:59:59" : " 00:00:00";
            return DateToken;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        bool FileSearchMatch(FileSystemInfo Item)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            foreach (FileColumn FC in ParentControl.Columns)
            {
                if (FC.Search == false)
                    continue;

                string[] Tokens = { "", "" };
                string Operator = Parameters[FC.ColumnID + "_search_operator"].ToString();

                Tokens[0] = Parameters[FC.ColumnID + "_search_token_1"].ToString();

                if (Tokens[0] == "")
                    continue;

                if (Operator == "between")
                    Tokens[1] = Parameters[FC.ColumnID + "_search_token_2"].ToString();

                switch (FC.ColumnID)
                {
                    case "Name":
                        switch (Operator)
                        {
                            case "startswith":
                                if (!Item.Name.StartsWith(Tokens[0],StringComparison.CurrentCultureIgnoreCase))
                                    return false;
                                break;
                            case "endswith":
                                if (!Item.Name.EndsWith(Tokens[0], StringComparison.CurrentCultureIgnoreCase))
                                    return false;
                                break;
                            case "equalto":
                                if (!Item.Name.Equals(Tokens[0], StringComparison.CurrentCultureIgnoreCase))
                                    return false;
                                break;
                            default:
                                if (Item.Name.IndexOf(Tokens[0], StringComparison.CurrentCultureIgnoreCase) == -1)
                                    return false;
                                break;
                        }
                        break;

                    case "Size":
                        if (Item is FileInfo)
                        {
                            int FileSize = Convert.ToInt32(((Item as FileInfo).Length + (FileSizeFactor() - 1)) / FileSizeFactor());
                            switch (Operator)
                            {
                                case "lessthan":
                                    if (FileSize >= Int32.Parse(Tokens[0]))
                                        return false;
                                    break;
                                case "greaterthan":
                                    if (FileSize <= Int32.Parse(Tokens[0]))
                                        return false;
                                    break;
                                case "equalto":
                                    if (FileSize != Int32.Parse(Tokens[0]))
                                        return false;
                                    break;
                                case "between":
                                    if (FileSize < Int32.Parse(Tokens[0]) || FileSize > Int32.Parse(Tokens[1]))
                                        return false;
                                    break;
                            }
                        }
                        else
                        {
                            return false;
                        }
                        break;

                    case "DateCreated":
                    case "DateLastModified":
                    case "DateLastAccessed":

                        DateTime FileDate = (Item as FileInfo).CreationTime;

                        switch (FC.ColumnID)
                        {
                            case "DateLastAccessed":
                                FileDate = (Item as FileInfo).LastAccessTime;
                                break;
                            case "DateLastModified":
                                FileDate = (Item as FileInfo).LastWriteTime;
                                break;
                        }

                        FileDate = new DateTime(FileDate.Year, FileDate.Month, FileDate.Day);

                        switch (Operator)
                        {
                            case "lessthan":
                                if (FileDate >= DateTime.Parse(Tokens[0]))
                                    return false;
                                break;
                            case "greaterthan":
                                if (FileDate <= DateTime.Parse(Tokens[0]))
                                    return false;
                                break;
                            case "equalto":
                                if (FileDate != DateTime.Parse(Tokens[0]))
                                    return false;
                                break;
                            case "between":
                                if (FileDate < DateTime.Parse(Tokens[0]) || FileDate > DateTime.Parse(Tokens[1]))
                                    return false;
                                break;
                        }
                        break;
                }
            }

            return true;

        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        int FileSizeFactor()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            switch (Parameters["Size_search_unit_1"].ToString())
            {
                case "bytes":
                    return 1;
                case "kb":
                    return 1024;
                default:
                    return 1024 * 1000;
            }
        }


        ///////////////////////////////////////////////
        private string Translate(string Key)
        ///////////////////////////////////////////////
        {
            return ParentControl.Translate(Key);
        }

    }
}