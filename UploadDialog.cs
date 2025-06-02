using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

///////////////////////////////////////////////
namespace DbNetLink.DbNetSuite
///////////////////////////////////////////////
{
    ///////////////////////////////////////////////
    internal class UploadDialog
    ///////////////////////////////////////////////
    {
        internal Shared ParentControl;
        private bool AjaxUpload;

        ///////////////////////////////////////////////
        public UploadDialog(Shared PC, bool ajaxUpload)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
            this.AjaxUpload = ajaxUpload;
        }

        ///////////////////////////////////////////////
        internal Control Build()
        ///////////////////////////////////////////////
        {
            HtmlTable T = new HtmlTable();
            T.Width = "600px";
            T.Height = "200px";

            T.Attributes.Add("class","dbnetsuite upload-dialog");
            T.Attributes.Add("title", ParentControl.Translate("FileUpload"));

            HtmlTableRow R = new HtmlTableRow();
            T.Controls.Add(R);

            HtmlTableCell C = new HtmlTableCell();
            R.Controls.Add(C);

            if (Shared.AjaxUpload == false || this.AjaxUpload == false)
                AddIframe(C);
            else
                AddForm(C);

            R = new HtmlTableRow();
            T.Controls.Add(R);
            C = new HtmlTableCell();
            R.Cells.Add(C);

            C.Controls.Add(AddToolbar());

            ParentControl.AddMessageRow(T);

            return T;
        }

        private void AddIframe(HtmlTableCell C)
        {
            HtmlGenericControl IF = new HtmlGenericControl("iframe");
            C.Controls.Add(IF);
            IF.ID = this.ParentControl.Id + "_uploadFrame";
            IF.Attributes.Add("name", this.ParentControl.Id + "_uploadFrame");
            IF.Attributes.Add("frameborder", "0");
            IF.Attributes.Add("allowtransparency", "true");
            IF.Attributes.Add("scrollbars", "no");
            IF.Style.Add(HtmlTextWriterStyle.Height, "100px");
            IF.Style.Add(HtmlTextWriterStyle.Width, "500px");

            this.ParentControl.MakeIframe508Compliant(IF, "UploadPage");
        }

        private HtmlTable AddToolbar()
        {
            HtmlTable Table = new HtmlTable();
            Table.Style.Add(HtmlTextWriterStyle.Width, "100%");

            HtmlTableRow R = new HtmlTableRow();
            Table.Rows.Add(R);

            HtmlTableCell C;

            if (Shared.AjaxUpload)
            {
                C = new HtmlTableCell();
                R.Controls.Add(C);
                C.Style.Add(HtmlTextWriterStyle.Width, "100%");
                HtmlGenericControl Progress = new HtmlGenericControl("div");
                Progress.ID = "progressBar";
                C.Controls.Add(Progress);
            }

            C = new HtmlTableCell();
            R.Controls.Add(C);
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
            C.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            C.Attributes.Add("class", "upload-toolbar");

            C.Controls.Add(ParentControl.BuildButton("upload", "Upload", "upload", ""));
            C.Controls.Add(ParentControl.BuildButton("cancel", "Cancel", "undo", ""));

            return Table;
        }

        private void AddForm(HtmlTableCell C)
        {
            HtmlGenericControl Layout = new HtmlGenericControl("div");
            Layout.ID = "layout";
            Layout.Style.Add(HtmlTextWriterStyle.TextAlign, "left");
            Layout.Style.Add(HtmlTextWriterStyle.Width, "600px");
            C.Controls.Add(Layout);
            HtmlGenericControl Form = new HtmlGenericControl("form");
            Layout.Controls.Add(Form);

            HtmlTable Table = new HtmlTable();
            Table.CellPadding = 0;
            Table.CellSpacing = 0;
            Table.Style.Add(HtmlTextWriterStyle.Width, "100%");
            Table.Attributes.Add("class", "upload-table");
            Form.Controls.Add(Table);

            HtmlTableRow Row = new HtmlTableRow();
            Table.Rows.Add(Row);
            HtmlTableCell Cell = AddCell(Row); ;
            Cell.Controls.Add(FileSelectRow());

            Row = new HtmlTableRow();
            Row.ID = "fileDataRow";
            Table.Rows.Add(Row);
            Cell = AddCell(Row);
            Cell.Controls.Add(FileDataTable());

            Row = new HtmlTableRow();
            Row.ID = "optionsRow";
            Table.Rows.Add(Row);
            Cell = AddCell(Row);

            Cell.Controls.Add(UploadOptions());
        }


        private HtmlTable FileSelectRow()
        {
            HtmlTable Table = new HtmlTable();
            Table.CellPadding = 0;
            Table.CellSpacing = 0;
            Table.Style.Add(HtmlTextWriterStyle.Width, "100%");

            HtmlTableRow Row = new HtmlTableRow();
            Table.Rows.Add(Row);
            HtmlTableCell Cell = AddCell(Row);

            Cell.Controls.Add(ParentControl.BuildButton("selectFile", "Select File", "select", "Select file for upload"));

            Cell = AddCell(Row);
            Cell.Attributes.Add("class", "or");
            Cell.InnerText = ParentControl.Translate("or");

            Cell = AddCell(Row);
            Cell.Style.Add(HtmlTextWriterStyle.Width, "100%");
            HtmlGenericControl DropZone = new HtmlGenericControl("div");
            DropZone.ID = "dropZone";
            DropZone.Attributes.Add("class", "drop-zone");
            DropZone.InnerHtml = ParentControl.Translate("DragYourFileHere");
            Cell.Controls.Add(DropZone);

            return Table;
        }


        private HtmlGenericControl FileDataTable()
        {
            string[] FileData = new String[] { "name","size","type","lastModified" };

            HtmlGenericControl FieldSet = new HtmlGenericControl("fieldset");
            HtmlGenericControl Legend = new HtmlGenericControl("legend");
            Legend.InnerText = "Selected File";
            FieldSet.Controls.Add(Legend);

            HtmlTable Table = new HtmlTable();
            FieldSet.Controls.Add(Table);
            HtmlTableCell Cell;

            foreach (string id in FileData)
            {
                HtmlTableRow Row = new HtmlTableRow();
                Table.Rows.Add(Row);
                Cell = AddCell(Row);
                Cell.InnerText = ParentControl.Translate(id);

                Cell = AddCell(Row);
                HtmlGenericControl Div = new HtmlGenericControl("div");
                Div.ID = id;
                Div.Attributes.Add("class", "file-data");
                Cell.Controls.Add(Div);
            }

            Cell = AddCell(Table.Rows[0]);
            Cell.ID = "thumbnail";
            Cell.RowSpan = 4;

            return FieldSet;
        }

        private HtmlGenericControl UploadOptions()
        {
            HtmlGenericControl FieldSet = new HtmlGenericControl("fieldset");
            HtmlGenericControl Legend = new HtmlGenericControl("legend");
            Legend.InnerText = "Options";
            FieldSet.Controls.Add(Legend);

            HtmlTable Table = new HtmlTable();
            Table.CellPadding = 0;
            Table.CellSpacing = 0;
            FieldSet.Controls.Add(Table);

            HtmlTableRow Row = new HtmlTableRow();
            Table.Rows.Add(Row);

            HtmlTableCell Cell = AddCell(Row);
            Cell.ID = "overwriteLabel";
            Cell.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            Cell.Attributes.Add("class", "overwrite");
            Cell.InnerText = "Overwrite";

            Cell = AddCell(Row);
            Cell.Attributes.Add("class", "overwrite");

            HtmlInputCheckBox Overwrite = new HtmlInputCheckBox();
            Overwrite.ID = "fileOverwrite";
            Overwrite.Name = "fileOverwrite";
            Cell.Controls.Add(Overwrite);

            Cell = AddCell(Row);
            Cell.Attributes.Add("class", "overwrite");
            Cell.InnerHtml = "&nbsp;";

            Cell = AddCell(Row);
            Cell.ID = "renameLabel";
            Cell.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            Cell.Attributes.Add("class", "rename");
            Cell.InnerText = "Alternate File Name";

            Cell = AddCell(Row);
            Cell.Attributes.Add("class", "rename");

            HtmlInputText Rename = new HtmlInputText();
            Rename.ID = "alternateFileName";
            Rename.Name = "alternateFileName";
            Cell.Controls.Add(Rename);

            HtmlInputFile fileInput = new HtmlInputFile();
            fileInput.ID = "fileInput";
            fileInput.Name = "fileInput";
            fileInput.Size = 50;
            fileInput.Style.Add(HtmlTextWriterStyle.Display, "none");
            Cell.Controls.Add(fileInput);

            HtmlInputHidden hiddenData = new HtmlInputHidden();
            hiddenData.ID = "data";
            hiddenData.Name = "data";
            Cell.Controls.Add(hiddenData);

            return FieldSet;
        }

        private HtmlTableCell AddCell(HtmlTableRow Row)
        {
            Row.Cells.Add(new HtmlTableCell());
            return Row.Cells[Row.Cells.Count - 1];
        }
    }
}
