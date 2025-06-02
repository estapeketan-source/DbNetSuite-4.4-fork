using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Text;
using System;
using DbNetLink.Data;
using System.Web;

///////////////////////////////////////////////
namespace DbNetLink.DbNetSuite
///////////////////////////////////////////////
{
    ///////////////////////////////////////////////
    internal class ThumbnailImage : Image
    ///////////////////////////////////////////////
    {
        public string Key = Guid.NewGuid().ToString();
        public ThumbnailInfo Info;

        ///////////////////////////////////////////////
        public ThumbnailImage(GridEditControl ParentControl, DbColumn Col, byte[] Buffer)
            : this(ParentControl, Col, null, Buffer, "")
        ///////////////////////////////////////////////
        {
        }

        ///////////////////////////////////////////////
        public ThumbnailImage(GridEditControl ParentControl, DbColumn Col, byte[] Buffer, string FileName)
            : this(ParentControl, Col, null, Buffer, FileName)
        ///////////////////////////////////////////////
        {
        }

        ///////////////////////////////////////////////
        public ThumbnailImage(GridEditControl ParentControl, DbColumn Col, Dictionary<string, object> PrimaryKey)
            : this(ParentControl, Col, PrimaryKey, null, "")
        ///////////////////////////////////////////////
        {
        }

        ///////////////////////////////////////////////
        public ThumbnailImage(GridEditControl ParentControl, DbColumn Col, Dictionary<string, object> PrimaryKey, byte[] Buffer, string FileName)
        ///////////////////////////////////////////////
        {
            if (ParentControl is DbNetGrid)
            {
                var grid = (ParentControl as DbNetGrid);
                if (grid.BuildMode == DbNetGrid.BuildModes.Pdf)
                {
                    var filePath = FileName;

                    if (!string.IsNullOrEmpty(Col.UploadRootFolder))
                    {
                        filePath = Col.UploadRootFolder.Replace("~",string.Empty) + "/" + filePath;
                    }
                    this.ImageUrl = HttpContext.Current.Request.MapPath(filePath);
                    return;
                }
            }

            if (FileName != "" && Col.DataType == "String")
                if (Col.UploadSubFolder != String.Empty)
                    Key = (Col.UploadSubFolder + "/" + FileName).Replace("//","/");
                else
                    Key = FileName;

            this.ImageUrl = ParentControl.AssignHandler("dbnetgrid.ashx?method=thumbnail&key=") + Key;

            QueryCommandConfig Query = new QueryCommandConfig();

            if (PrimaryKey != null)
            {
                this.ImageUrl += "&cs=" + ParentControl.ConnectionString;
                Query.Sql = "select " + Col.ColumnExpression + " from " + Col.BaseTableName;
                ParentControl.AddPrimaryKeyFilter(Query, PrimaryKey);
            }

            Info = new ThumbnailInfo(Col, Query, Buffer, FileName);

            if (ParentControl.Context.Session != null)
                ParentControl.Context.Session[Key] = Info;
            else
            {
                DbNetLink.Util.EncryptionEnabled = true;
                this.ImageUrl += "&query=" + DbNetLink.Util.Encrypt(Query.Sql) + "&pk=" + DbNetLink.Util.Encrypt(ParentControl.JSON.Serialize(PrimaryKey)) + "&maxthumbnailheight=" + Col.MaxThumbnailHeight.ToString();
                DbNetLink.Util.EncryptionEnabled = false;
            }  
        }
    }

    [Serializable]
    ///////////////////////////////////////////////
    public class ThumbnailInfo
    ///////////////////////////////////////////////
    {
        internal string DataType;
        internal string UploadRootFolder;
        internal string UploadSubFolder;
        internal int MaxThumbnailHeight;
        internal string UploadExtFilter;
        internal QueryCommandConfig Query;
        public Byte[] Buffer;
        internal string FileName;
        public bool Persist = false;

        ///////////////////////////////////////////////
        public ThumbnailInfo(Byte[] Buffer) : this (null, null,Buffer, String.Empty)
        ///////////////////////////////////////////////
        {
        }

        ///////////////////////////////////////////////
        public ThumbnailInfo(DbColumn Col, QueryCommandConfig Query, Byte[] Buffer, string FileName)
        ///////////////////////////////////////////////
        {
            if (Col != null)
            {
                this.DataType = Col.DataType;
                this.UploadRootFolder = Col.UploadRootFolder;
                this.UploadSubFolder = Col.UploadSubFolder;
                this.MaxThumbnailHeight = Col.MaxThumbnailHeight;
                this.UploadExtFilter = Col.UploadExtFilter;
            }
            this.Query = Query;
            this.Buffer = Buffer;
            this.FileName = FileName;
        }

    }

    [Serializable]
    ///////////////////////////////////////////////
    internal class ImageData
    ///////////////////////////////////////////////
    {
        public Byte[] Data;
        public string FileName;
        public string ContentType;
        public bool IsImage = false;

        ///////////////////////////////////////////////
        public ImageData(Byte[] Data, String ContentType, String FileName)
        ///////////////////////////////////////////////
        {
            this.Data = Data;
            this.ContentType = ContentType;
            this.FileName = FileName;
        }
    }
}
