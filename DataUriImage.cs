using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.Reflection;
using System.Drawing.Imaging;
using DbNetLink.Data;

///////////////////////////////////////////////
namespace DbNetLink.DbNetSuite
///////////////////////////////////////////////
{
    ///////////////////////////////////////////////
    internal class ByteInfo
    ///////////////////////////////////////////////
    {
        public string ContentType = String.Empty;
        public string Ext = String.Empty;
        public bool IsImage = false;
        private Winista.Mime.MimeTypes MimeInfo = null;
        private Winista.Mime.MimeType MimeType = null;
        public System.Drawing.Image Img = null;

        ///////////////////////////////////////////////
        public ByteInfo(byte[] Data, string FileName, string ExtFilter)
        ///////////////////////////////////////////////
        {
            try
            {
                MemoryStream ms = new MemoryStream(Data);
                Img = System.Drawing.Image.FromStream(ms);
                ContentType = "image/jpeg";
                IsImage = true;
            }
            catch (Exception)
            {
            }

            if (ContentType != String.Empty)
                return;

            Ext = this.GetExtension(FileName);

            if (Ext == String.Empty)
            {
                if (ExtFilter.Split(',').Length == 1)
                    Ext = ExtFilter.Replace(".", "");

                if (Ext == String.Empty)
                {
                    MimeType = this.GetMimeInfo(Data);
                    if (MimeType != null)
                        Ext = MimeType.Extensions[0];
                }
            }

            if (Ext != String.Empty)
            {
                Shared.LoadMimeTypes();
                if (Shared.MimeTypes.ContainsKey(Ext))
                    ContentType = (string)Shared.MimeTypes[Ext];
            }

            if (ContentType == String.Empty)
                if (MimeType != null)
                    ContentType = MimeType.ToString();

            if (ContentType == String.Empty)
                ContentType = "text/plain";
        }

        ///////////////////////////////////////////////
        private string GetExtension(string FileName)
        ///////////////////////////////////////////////
        {
            if (FileName == String.Empty)
                return String.Empty;
            else
                return Path.GetExtension(FileName).ToLower().Trim().TrimStart('.').Trim(); ;
        }


        ///////////////////////////////////////////////
        internal Winista.Mime.MimeType GetMimeInfo(byte[] byteArray)
        ///////////////////////////////////////////////
        {
            if (this.MimeInfo == null)
            {
                Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("DbNetLink.Resources.Xml.mime-types.xml");
                System.Xml.XmlDocument MT = new System.Xml.XmlDocument();
                StreamReader SR = new StreamReader(s);
                MT.LoadXml(SR.ReadToEnd());
                SR.Close();
                this.MimeInfo = new Winista.Mime.MimeTypes(MT);
            }
            sbyte[] fileData = Winista.Mime.SupportUtil.ToSByteArray(byteArray);
            return this.MimeInfo.GetMimeType(fileData);
        }
    }

    ///////////////////////////////////////////////
    internal class DataUriImage : Image
    ///////////////////////////////////////////////
    {
        public string Key = Guid.NewGuid().ToString();

        public DataUriImage(GridEditControl ParentControl, byte[] Buffer, DbColumn Col, string FileName, bool ThumbNail)
            : this(ParentControl, Buffer, Col, FileName, ThumbNail, false)
        {
        }

        public DataUriImage(GridEditControl ParentControl, byte[] Buffer, DbColumn Col, string FileName, bool ThumbNail, bool Persist)
        {
            ByteInfo ImgInfo = new ByteInfo(Buffer, FileName, Col.UploadExtFilter);

            if (FileName != "" && Col.DataType == "String")
                if (Col.UploadSubFolder != String.Empty)
                    Key = (Col.UploadSubFolder + "/" + FileName).Replace("//", "/");
                else
                    Key = FileName;

            byte[] data;

            if (Persist)
                ParentControl.Context.Session[Key] = new ThumbnailInfo(Buffer);

            if (ImgInfo.IsImage)
            {
                int w = ImgInfo.Img.Width;
                int h = ImgInfo.Img.Height;

                if (ImgInfo.Img.Height > Col.MaxThumbnailHeight && ThumbNail)
                {
                    double factor = ((double)Col.MaxThumbnailHeight / (double)ImgInfo.Img.Height);
                    w = Convert.ToInt16((w * factor));
                    h = Convert.ToInt16((h * factor));
                }

                var ms = new MemoryStream();
                new System.Drawing.Bitmap(ImgInfo.Img, w, h).Save(ms, ImageFormat.Jpeg);
                data = ms.ToArray();

                this.ImageUrl = "data:image/jpeg;base64," + Convert.ToBase64String(data);
                return;
            }

            string ResourceName = "DbNetLink.Resources.Images.Ext.unknown.gif";

            if (ImgInfo.Ext != String.Empty)
            {
                ResourceName = "DbNetLink.Resources.Images.Ext." + ImgInfo.Ext.ToLower() + ".gif";

                if (Assembly.GetExecutingAssembly().GetManifestResourceInfo(ResourceName) == null)
                    ResourceName = "DbNetLink.Resources.Images.Ext.unknown.gif";
            }

            Assembly A = Assembly.GetAssembly(typeof(Shared));

            Stream s = A.GetManifestResourceStream(ResourceName);
            data = new byte[s.Length];
            s.Read(data, 0, data.Length);

            this.ImageUrl = "data:image/gif;base64," + Convert.ToBase64String(data);
        }
    }
}
