using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;


namespace DbNetLink.DbNetSuite
{
    public class DbNetFile : Shared
    {
        #region Enumerators

        /// <summary> 
        /// Specifies the location of the toolbar 
        /// </summary>

        public enum ToolbarLocation
        {
            /// <summary>
            /// Locate toolbar above grid.
            /// </summary>
            Top,
            /// <summary>
            /// Locate toolbar below grid.
            /// </summary>
            Bottom,
            /// <summary>
            /// Hide the toolbar
            /// </summary>
            Hidden
        }

        /// <summary> 
        /// The SelectionModes enum is assigned to the <see cref="DbNetLink.DbNetSuiteVS.DbNetFile.SelectionMode">SelectionMode</see> property.
        /// </summary>
        public enum SelectionModes
        {
            /// <summary>
            /// Files and folders are selected.
            /// </summary>
            FoldersAndFiles,
            /// <summary>
            /// Only folders are selected.
            /// </summary>
            FoldersOnly,
            /// <summary>
            /// Only files are selected.
            /// </summary>
            FilesOnly
        }

        /// <summary> 
        /// The DisplayStyles enum is assigned to the <see cref="DbNetLink.DbNetSuiteVS.DbNetFile.DisplayStyle">DisplayStyle</see> property. 
        /// </summary>
        /// 
        public enum DisplayStyles
        {
            /// <summary>
            /// Displays file system information in a grid format with a toolbar
            /// </summary>
            Grid,
            /// <summary>
            /// Displays file system information in a heirarchical tree format  
            /// </summary>
            Tree
        }

        /// <summary> 
        /// The ColumnTypes enum is assigned to the <see cref="DbNetLink.DbNetSuiteVS.FileColumn.ColumnType">ColumnType</see> property and describes the type of information displayed in the grid column. 
        /// </summary>
        /// <remarks> 
        /// If the column type is WindowsSearch or IndexingService then the type of WindowsSearch/IndexingService must be also be specified in the <see cref="DbNetLink.DbNetSuiteVS.FileColumn.WindowsSearchColumnType">WindowsSearchColumnType</see> or <see cref="DbNetLink.DbNetSuiteVS.FileColumn.IndexingServiceColumnType">IndexingServiceColumnType</see> respecively. 
        /// You cannot specify WindowsSearch and IndexingService columns together in the same instance of DbNetFile
        /// </remarks>
        public enum ColumnTypes
        {
            /// <summary> 
            /// Icon respresenting the file type in the column
            /// </summary>
            Icon,
            /// <summary> 
            /// File name
            /// </summary>
            Name,
            /// <summary> 
            /// File size
            /// </summary>
            Size,
            /// <summary> 
            /// File type (extension)
            /// </summary>
            Type,
            /// <summary> 
            /// Date the file was created
            /// </summary>
            DateCreated,
            /// <summary> 
            /// Date the file was last modified
            /// </summary>
            DateLastModified,
            /// <summary> 
            /// Date the file was last accessed
            /// </summary>
            DateLastAccessed,
            /// <summary> 
            /// Thumbnail (for image files only)
            /// </summary>
            Thumbnail,
            /// <summary> 
            /// Parent folder
            /// </summary>
            Folder,
            /// <summary> 
            /// Specifies that the column value is a <seealso cref="http://en.wikipedia.org/wiki/Windows_Search">Windows Search</seealso> 
            /// property specified by the <see cref="DbNetLink.DbNetSuiteVS.FileColumn.WindowsSearchColumnType">WindowsSearchColumnType</see> property
            /// </summary>
            WindowsSearch,
            /// <summary> 
            /// Specifies that the column value is a <seealso cref="http://en.wikipedia.org/wiki/Indexing_service">Indexing Service</seealso> 
            /// property specified by the <see cref="DbNetLink.DbNetSuiteVS.FileColumn.IndexingServiceColumnType">IndexingServiceColumnType</see> property
            /// </summary>
            IndexingService
        }

        /// <summary> 
        /// The IndexingServiceColumnTypes enum is assigned to the <see cref="DbNetLink.DbNetSuiteVS.FileColumn.IndexingServiceColumnType">ColumnType</see> property 
        /// and describes the type of indexing service information associated with the column. 
        /// </summary>
        /// <remarks> 
        /// The <see cref="DbNetLink.DbNetSuiteVS.FileColumn.ColumnType">ColumnType</see> property must be set to <see cref="DbNetLink.DbNetSuiteVS.DbNetFile.ColumnType.IndexingServiceColumnType">IndexingServiceColumnType</see>
        /// </remarks>
        public enum IndexingServiceColumnTypes
        {
            /// <summary> 
            /// Unassigned
            /// </summary>
            Unassigned,
            /// <summary> 
            /// Name of application that created the file.
            /// </summary>
            DocAppName,
            /// <summary> 
            /// Author of document.
            /// </summary>
            DocAuthor,
            /// <summary> 
            /// Number of bytes in a document.
            /// </summary>
            DocByteCount,
            /// <summary> 
            /// Type of document such as a memo, schedule, or white paper
            /// </summary>
            DocCategory,
            /// <summary> 
            /// Number of characters in document. 
            /// </summary>
            DocCharCount,
            /// <summary> 
            /// Comments about document.
            /// </summary>
            DocComments,
            /// <summary> 
            /// Name of the company for which the document was written. 
            /// </summary>
            DocCompany,
            /// <summary> 
            /// Number of hidden slides in a Microsoft® PowerPoint document. 
            /// </summary>
            DocHiddenCount,
            /// <summary> 
            /// Document keywords. 
            /// </summary>
            DocKeywords,
            /// <summary> 
            /// Most recent user who edited document. 
            /// </summary>
            DocLastAuthor,
            /// <summary> 
            /// Time document was last printed. 
            /// </summary>
            DocLastPrinted,
            /// <summary> 
            /// Number of lines contained in a document. 
            /// </summary>
            DocLineCount,
            /// <summary> 
            /// Name of the manager of the document’s author. 
            /// </summary>
            DocManager,
            /// <summary> 
            /// Number of pages with notes in a PowerPoint document. 
            /// </summary>
            DocNoteCount,
            /// <summary> 
            /// Number of pages in document. 
            /// </summary>
            DocPageCount,
            /// <summary> 
            /// Number of paragraphs in a document. 
            /// </summary>
            DocParaCount,
            /// <summary> 
            /// Names of document parts. For example, in Microsoft Excel part titles are the names of spreadsheets; in PowerPoint, slide titles; and in Word for Windows, the names of the documents in the master document. 
            /// </summary>
            DocPartTitles,
            /// <summary> 
            /// Target format (35mm, printer, video, and so on) for a presentation in PowerPoint. 
            /// </summary>
            DocPresentationTarget,
            /// <summary> 
            /// Current version number of document. 
            /// </summary>
            DocRevNumber,
            /// <summary> 
            /// Number of slides in a PowerPoint document. 
            /// </summary>
            DocSlideCount,
            /// <summary> 
            /// Subject of document.
            /// </summary>
            DocSubject,
            /// <summary> 
            /// Name of template for document. 
            /// </summary>
            DocTemplate,
            /// <summary> 
            /// Title of document. 
            /// </summary>
            DocTitle,
            /// <summary> 
            /// Number of words in document. 
            /// </summary>
            DocWordCount,
            /// <summary> 
            /// Number of hits (words matching query) in file. 
            /// </summary>
            HitCount,
            /// <summary> 
            /// Rank of row. Ranges from 0 to 1000. Larger numbers indicate better matches. 
            /// </summary>
            Rank
        }

        /// <summary> 
        /// The WindowsSearchColumnTypes enum is assigned to the <see cref="DbNetLink.DbNetSuiteVS.FileColumn.WindowsSearchColumnType">ColumnType</see> property 
        /// and describes the windows search property information associated with the column. 
        /// </summary>
        /// <remarks> 
        /// The <see cref="DbNetLink.DbNetSuiteVS.FileColumn.ColumnType">ColumnType</see> property must be set to <see cref="DbNetLink.DbNetSuiteVS.DbNetFile.ColumnType.IndexingServiceColumnType">IndexingServiceColumnType</see>
        /// </remarks>
        public enum WindowsSearchColumnTypes
        {
            /// <summary> 
            /// Unassigned
            /// </summary>
            Unassigned,
            /// <summary>
            /// A hash value that indicates the acquisition session.
            /// </summary>
            System_AcquisitionID,
            /// <summary>
            /// The name of the application that created this file or item. Do not use version numbers to identify the application's specific version.
            /// </summary>
            System_ApplicationName,
            /// <summary>
            /// Represents the author or authors of the document.
            /// </summary>
            System_Author,
            /// <summary>
            /// The amount of total storage space, expressed in bytes. This property is mainly used to indicate the capacity of storage media such as hard drives.
            /// </summary>
            System_Capacity,
            /// <summary>
            /// Deprecated. The category that can be assigned to an item such as a document or file. This property is inherited from OLE document properties and is deprecated for WindowsÂ Vista. Keywords should be used instead. Older code treats this property as VT_LPSTR.
            /// </summary>
            System_Category,
            /// <summary>
            /// The comment attached to a file, typically added by a user.
            /// </summary>
            System_Comment,
            /// <summary>
            /// The company or publisher.
            /// </summary>
            System_Company,
            /// <summary>
            /// The name of the computer where the item or file is located. This property is automatically populated by Microsoft Windows and should not be used for anything other than its intended purpose.
            /// </summary>
            System_ComputerName,
            /// <summary>
            /// A list of the type of content in the item. This value is represented as a vector array of GUIDs, where each GUID represents a certain type, such as URLs or attachments.
            /// </summary>
            System_ContainedItems,
            /// <summary>
            /// 
            /// </summary>
            System_ContentStatus,
            /// <summary>
            /// 
            /// </summary>
            System_ContentType,
            /// <summary>
            /// Represents the author or authors of the document.
            /// </summary>
            System_Copyright,
            /// <summary>
            /// Indicates the last time the item was accessed. The Indexing Service friendly name is "access".
            /// </summary>
            System_DateAccessed,
            /// <summary>
            /// 
            /// </summary>
            System_DateAcquired,
            /// <summary>
            /// The date the file item was last archived.
            /// </summary>
            System_DateArchived,
            /// <summary>
            /// 
            /// </summary>
            System_DateCompleted,
            /// <summary>
            /// The date and time the item was created on the file system where it is currently located. This property is automatically promoted by the file system. The Indexing Service friendly name is "create".
            /// </summary>
            System_DateCreated,
            /// <summary>
            /// The date and time the file was imported into a private application database. For example, this property can be used when a photo is imported into a photo database. This property is not the same as 
            /// </summary>
            System_DateImported,
            /// <summary>
            /// The date and time of the last modification to the item. The Indexing Service friendly name is 'write'.
            /// </summary>
            System_DateModified,
            /// <summary>
            /// 
            /// </summary>
            System_DueDate,
            /// <summary>
            /// 
            /// </summary>
            System_EndDate,
            /// <summary>
            /// 
            /// </summary>
            System_FileAllocationSize,
            /// <summary>
            /// The attributes of the item. These are equivalent to the values recognized in the 
            /// </summary>
            System_FileAttributes,
            /// <summary>
            /// 
            /// </summary>
            System_FileCount,
            /// <summary>
            /// Represents the author or authors of the document.
            /// </summary>
            System_FileDescription,
            /// <summary>
            /// The file extension of the file-based item, including the leading period.
            /// </summary>
            System_FileExtension,
            /// <summary>
            /// The unique file ID, also known as the File Reference Number. For a given file, this is the same value as the 
            /// </summary>
            System_FileFRN,
            /// <summary>
            /// The file name, including its extension.
            /// </summary>
            System_FileName,
            /// <summary>
            /// The owner of the file, as known by the file system.
            /// </summary>
            System_FileOwner,
            /// <summary>
            /// 
            /// </summary>
            System_FileVersion,
            /// <summary>
            /// Contains the 
            /// </summary>
            System_FindData,
            /// <summary>
            /// 
            /// </summary>
            System_FlagColor,
            /// <summary>
            /// The user-friendly form of 
            /// </summary>
            System_FlagColorText,
            /// <summary>
            /// The status of a flag. Values: (0=none 1=white 2=Red). 
            /// </summary>
            System_FlagStatus,
            /// <summary>
            /// Represents the author or authors of the document.
            /// </summary>
            System_FlagStatusText,
            /// <summary>
            /// The amount of free space in a volume, in bytes.
            /// </summary>
            System_FreeSpace,
            /// <summary>
            /// 
            /// </summary>
            System_Identity,
            /// <summary>
            /// 
            /// </summary>
            System_Importance,
            /// <summary>
            /// The user-friendly form of 
            /// </summary>
            System_ImportanceText,
            /// <summary>
            /// Identifies whether the item is an attachment.
            /// </summary>
            System_IsAttachment,
            /// <summary>
            /// 
            /// </summary>
            System_IsDeleted,
            /// <summary>
            /// 
            /// </summary>
            System_IsEncrypted,
            /// <summary>
            /// 
            /// </summary>
            System_IsFlagged,
            /// <summary>
            /// 
            /// </summary>
            System_IsFlaggedComplete,
            /// <summary>
            /// Identifies whether the message was completely received. This value is used with some error conditions.
            /// </summary>
            System_IsIncomplete,
            /// <summary>
            /// Identifies whether the item has been read.
            /// </summary>
            System_IsRead,
            /// <summary>
            /// Indicates whether an item is a valid SendTo target. This information is provided by certain Shell folders.
            /// </summary>
            System_IsSendToTarget,
            /// <summary>
            /// Indicates whether the item is shared.
            /// </summary>
            System_IsShared,
            /// <summary>
            /// Generic list of authors associated with an item. For example, the artist name for a music track is the item author.
            /// </summary>
            System_ItemAuthors,
            /// <summary>
            /// The primary date of interest for an item. In the case of photos, for example, this property maps to 
            /// </summary>
            System_ItemDate,
            /// <summary>
            /// The user-friendly display name of an item's parent folder.
            /// </summary>
            System_ItemFolderNameDisplay,
            /// <summary>
            /// The user-friendly display path of an item's parent folder.
            /// </summary>
            System_ItemFolderPathDisplay,
            /// <summary>
            /// 
            /// </summary>
            System_ItemFolderPathDisplayNarrow,
            /// <summary>
            /// The base name of the 
            /// </summary>
            System_ItemName,
            /// <summary>
            /// The display name in "most complete" form. It is the unique representation of the item name most appropriate for end users.
            /// </summary>
            System_ItemNameDisplay,
            /// <summary>
            /// The prefix of an item, used for e-mail messages where the subject begins with the prefix "Re:".
            /// </summary>
            System_ItemNamePrefix,
            /// <summary>
            /// The generic list of people associated with and contributing to an item. For example, this is the combination of people in the To list, Cc list, and the sender of an e-mail message.
            /// </summary>
            System_ItemParticipants,
            /// <summary>
            /// The user-friendly display path to the item.
            /// </summary>
            System_ItemPathDisplay,
            /// <summary>
            /// The user-friendly display path to the item.
            /// </summary>
            System_ItemPathDisplayNarrow,
            /// <summary>
            /// The canonical type of the item.
            /// </summary>
            System_ItemType,
            /// <summary>
            /// The user-friendly type name of the item. This value is not intended to be programmatically parsed.
            /// </summary>
            System_ItemTypeText,
            /// <summary>
            /// Represents a well-formed URL that points to the item.
            /// </summary>
            System_ItemUrl,
            /// <summary>
            /// The set of keywords (also known as "tags") assigned to the item.
            /// </summary>
            System_Keywords,
            /// <summary>
            /// Maps extensions to various .Search folders.
            /// </summary>
            System_Kind,
            /// <summary>
            /// The user-friendly form of 
            /// </summary>
            System_KindText,
            /// <summary>
            /// The primary language of the file, particularly if that file is a document.
            /// </summary>
            System_Language,
            /// <summary>
            /// 
            /// </summary>
            System_MileageInformation,
            /// <summary>
            /// The MIME type.
            /// </summary>
            System_MIMEType,
            /// <summary>
            /// 
            /// </summary>
            System_Null,
            /// <summary>
            /// 
            /// </summary>
            System_OfflineAvailability,
            /// <summary>
            /// 
            /// </summary>
            System_OfflineStatus,
            /// <summary>
            /// 
            /// </summary>
            System_OriginalFileName,
            /// <summary>
            /// The parental rating stored in a format typically determined by the organization named in 
            /// </summary>
            System_ParentalRating,
            /// <summary>
            /// Explains file ratings. For example, "Violence, Foul Language" for a rating of R under the MPAA rating system.
            /// </summary>
            System_ParentalRatingReason,
            /// <summary>
            /// The name of the organization whose rating system is used for 
            /// </summary>
            System_ParentalRatingsOrganization,
            /// <summary>
            /// Used to get the 
            /// </summary>
            System_ParsingBindContext,
            /// <summary>
            /// The Shell namespace name of an item relative to a parent folder.
            /// </summary>
            System_ParsingName,
            /// <summary>
            /// The Shell namespace path to the item.
            /// </summary>
            System_ParsingPath,
            /// <summary>
            /// The perceived file type based on its canonical type.
            /// </summary>
            System_PerceivedType,
            /// <summary>
            /// The amount of space filled, as a percentage.
            /// </summary>
            System_PercentFull,
            /// <summary>
            /// 
            /// </summary>
            System_Priority,
            /// <summary>
            /// The user-friendly form of 
            /// </summary>
            System_PriorityText,
            /// <summary>
            /// 
            /// </summary>
            System_Project,
            /// <summary>
            /// 
            /// </summary>
            System_ProviderItemID,
            /// <summary>
            /// A rating system that uses integer values between 0 and 99. This is the rating system used by the WindowsÂ Vista Shell.
            /// </summary>
            System_Rating,
            /// <summary>
            /// The user-friendly form of 
            /// </summary>
            System_RatingText,
            /// <summary>
            /// 
            /// </summary>
            System_Sensitivity,
            /// <summary>
            /// The user-friendly form of 
            /// </summary>
            System_SensitivityText,
            /// <summary>
            /// 
            /// </summary>
            System_SFGAOFlags,
            /// <summary>
            /// Indicates who the item is shared with.
            /// </summary>
            System_SharedWith,
            /// <summary>
            /// 
            /// </summary>
            System_ShareUserRating,
            /// <summary>
            /// Omits an item from Shell views.
            /// </summary>
            System_Shell_OmitFromView,
            /// <summary>
            /// A rating system that uses a range of integer values between 0 and 5.
            /// </summary>
            System_SimpleRating,
            /// <summary>
            /// The system-provided file system size of the item, in bytes.
            /// </summary>
            System_Size,
            /// <summary>
            /// 
            /// </summary>
            System_SoftwareUsed,
            /// <summary>
            /// 
            /// </summary>
            System_SourceItem,
            /// <summary>
            /// 
            /// </summary>
            System_StartDate,
            /// <summary>
            /// Generic status information applicable to the item.
            /// </summary>
            System_Status,
            /// <summary>
            /// The subject of a document. This property maps to the OLE document property 
            /// </summary>
            System_Subject,
            /// <summary>
            /// Represents the thumbnail in VT_CF format.
            /// </summary>
            System_Thumbnail,
            /// <summary>
            /// A unique value used as a key to cache thumbnails. The value changes when the name, volume, or data modified of an item changes.
            /// </summary>
            System_ThumbnailCacheId,
            /// <summary>
            /// Data that represents the thumbnail in VT_STREAM format, supported by Microsoft Windows GDI+ and Windows codecs such as .jpg and .png.
            /// </summary>
            System_ThumbnailStream,
            /// <summary>
            /// Specifies how to configure the Microsoft Windows search engine with respect to a given property definition. If no 
            /// </summary>
            System_Title,
            /// <summary>
            /// 
            /// </summary>
            System_TotalFileSize,
            /// <summary>
            /// The trademark associated with the item, in a string format.
            /// </summary>
            System_Trademarks,
            /// <summary>
            /// Indicates the channel count for the audio file. Values: 1 (mono), 2 (stereo).
            /// </summary>
            Audio_ChannelCount,
            /// <summary>
            /// 
            /// </summary>
            Audio_Compression,
            /// <summary>
            /// Indicates the average data rate for the audio file in bits per second.
            /// </summary>
            Audio_EncodingBitrate,
            /// <summary>
            /// Indicates the format of the audio file.
            /// </summary>
            Audio_Format,
            /// <summary>
            /// 
            /// </summary>
            Audio_IsVariableBitRate,
            /// <summary>
            /// 
            /// </summary>
            Audio_PeakValue,
            /// <summary>
            /// Indicates the sample rate for the audio file in samples per second.
            /// </summary>
            Audio_SampleRate,
            /// <summary>
            /// Indicates the sample size for the audio file in bits per sample.
            /// </summary>
            Audio_SampleSize,
            /// <summary>
            /// 
            /// </summary>
            Audio_StreamName,
            /// <summary>
            /// 
            /// </summary>
            Audio_StreamNumber,
            /// <summary>
            /// 
            /// </summary>
            Document_ByteCount,
            /// <summary>
            /// 
            /// </summary>
            Document_CharacterCount,
            /// <summary>
            /// 
            /// </summary>
            Document_ClientID,
            /// <summary>
            /// 
            /// </summary>
            Document_Contributor,
            /// <summary>
            /// Indicates the date and time that a document was created. This information is stored in the document, not obtained from the file system.
            /// </summary>
            Document_DateCreated,
            /// <summary>
            /// Indicates the date and time the document was last printed. The legacy name is "DocLastPrinted".
            /// </summary>
            Document_DatePrinted,
            /// <summary>
            /// Indicates the date and time the document was last saved. The legacy name is "DocLastSavedTm".
            /// </summary>
            Document_DateSaved,
            /// <summary>
            /// 
            /// </summary>
            Document_Division,
            /// <summary>
            /// 
            /// </summary>
            Document_DocumentID,
            /// <summary>
            /// 
            /// </summary>
            Document_HiddenSlideCount,
            /// <summary>
            /// 
            /// </summary>
            Document_LastAuthor,
            /// <summary>
            /// 
            /// </summary>
            Document_LineCount,
            /// <summary>
            /// 
            /// </summary>
            Document_Manager,
            /// <summary>
            /// 
            /// </summary>
            Document_MultimediaClipCount,
            /// <summary>
            /// 
            /// </summary>
            Document_NoteCount,
            /// <summary>
            /// 
            /// </summary>
            Document_PageCount,
            /// <summary>
            /// 
            /// </summary>
            Document_ParagraphCount,
            /// <summary>
            /// 
            /// </summary>
            Document_PresentationFormat,
            /// <summary>
            /// 
            /// </summary>
            Document_RevisionNumber,
            /// <summary>
            /// Access control information, from SummaryInfo propset
            /// </summary>
            Document_Security,
            /// <summary>
            /// 
            /// </summary>
            Document_SlideCount,
            /// <summary>
            /// 
            /// </summary>
            Document_Template,
            /// <summary>
            /// This property represents the total time between each open and save, accumulated since the creation of the document. This is measured in 100ns units, not milliseconds. VT_FILETIME for IPropertySetStorage handlers (legacy)
            /// </summary>
            Document_TotalEditingTime,
            /// <summary>
            /// 
            /// </summary>
            Document_Version,
            /// <summary>
            /// 
            /// </summary>
            Document_WordCount,
            /// <summary>
            /// Indicates how many bits are used in each pixel of the image. (Usually 8, 16, 24, or 32).
            /// </summary>
            Image_BitDepth,
            /// <summary>
            /// The colorspace embedded in the image. Taken from the Exchangeable Image File (EXIF) information.
            /// </summary>
            Image_ColorSpace,
            /// <summary>
            /// Indicates the image compression level. . Calculated from PKEY_Image_CompressedBitsPerPixelNumerator and PKEY_Image_CompressedBitsPerPixelDenominator.
            /// </summary>
            Image_CompressedBitsPerPixel,
            /// <summary>
            /// The denominator of PKEY_Image_CompressedBitsPerPixel.
            /// </summary>
            Image_CompressedBitsPerPixelDenominator,
            /// <summary>
            /// The numerator of PKEY_Image_CompressedBitsPerPixel.
            /// </summary>
            Image_CompressedBitsPerPixelNumerator,
            /// <summary>
            /// The algorithm used to compress the image.
            /// </summary>
            Image_Compression,
            /// <summary>
            /// The user-friendly form of System.Image.Compression. Not intended to be parsed programmatically.
            /// </summary>
            Image_CompressionText,
            /// <summary>
            /// The image dimensions in string format as 
            /// </summary>
            Image_Dimensions,
            /// <summary>
            /// Indicates the number of pixels per resolution unit in the image width.
            /// </summary>
            Image_HorizontalResolution,
            /// <summary>
            /// The horizontal size of the image, in pixels.
            /// </summary>
            Image_HorizontalSize,
            /// <summary>
            /// 
            /// </summary>
            Image_ImageID,
            /// <summary>
            /// Indicates the resolution units. Used for images with a non-square aspect ratio, but without meaningful absolute dimensions. 1 = No absolute unit of measurement. 2 = Inches. 3 = Centimeters. The default value is 2 (Inches).
            /// </summary>
            Image_ResolutionUnit,
            /// <summary>
            /// Indicates the number of pixels per resolution unit in the image height.
            /// </summary>
            Image_VerticalResolution,
            /// <summary>
            /// The vertical size of the image, in pixels.
            /// </summary>
            Image_VerticalSize,
            /// <summary>
            /// 
            /// </summary>
            Media_AuthorUrl,
            /// <summary>
            /// 
            /// </summary>
            Media_AverageLevel,
            /// <summary>
            /// 
            /// </summary>
            Media_ClassPrimaryID,
            /// <summary>
            /// 
            /// </summary>
            Media_ClassSecondaryID,
            /// <summary>
            /// 
            /// </summary>
            Media_CollectionGroupID,
            /// <summary>
            /// 
            /// </summary>
            Media_CollectionID,
            /// <summary>
            /// 
            /// </summary>
            Media_ContentDistributor,
            /// <summary>
            /// 
            /// </summary>
            Media_ContentID,
            /// <summary>
            /// 
            /// </summary>
            Media_CreatorApplication,
            /// <summary>
            /// 
            /// </summary>
            Media_CreatorApplicationVersion,
            /// <summary>
            /// Represents the date and time the file was encoded. The DateTime is in UTC (in the doc, not file system).
            /// </summary>
            Media_DateEncoded,
            /// <summary>
            /// 
            /// </summary>
            Media_DateReleased,
            /// <summary>
            /// Represents the actual play time of a media file and is measured in 100ns units, not milliseconds.
            /// </summary>
            Media_Duration,
            /// <summary>
            /// 
            /// </summary>
            Media_DVDID,
            /// <summary>
            /// 
            /// </summary>
            Media_EncodedBy,
            /// <summary>
            /// 
            /// </summary>
            Media_EncodingSettings,
            /// <summary>
            /// Indicates the frame count for the image.
            /// </summary>
            Media_FrameCount,
            /// <summary>
            /// 
            /// </summary>
            Media_MCDI,
            /// <summary>
            /// 
            /// </summary>
            Media_MetadataContentProvider,
            /// <summary>
            /// 
            /// </summary>
            Media_Producer,
            /// <summary>
            /// 
            /// </summary>
            Media_PromotionUrl,
            /// <summary>
            /// Describes the type of media protection.
            /// </summary>
            Media_ProtectionType,
            /// <summary>
            /// The rating (0 - 99) supplied by metadata provider.
            /// </summary>
            Media_ProviderRating,
            /// <summary>
            /// Represents the actual play time of a media file and is measured in 100ns units, not milliseconds.
            /// </summary>
            Media_ProviderStyle,
            /// <summary>
            /// 
            /// </summary>
            Media_Publisher,
            /// <summary>
            /// 
            /// </summary>
            Media_SubscriptionContentId,
            /// <summary>
            /// 
            /// </summary>
            Media_SubTitle,
            /// <summary>
            /// 
            /// </summary>
            Media_UniqueFileIdentifier,
            /// <summary>
            /// If true, do not alter this file's metadata. Set by user.
            /// </summary>
            Media_UserNoAutoInfo,
            /// <summary>
            /// 
            /// </summary>
            Media_UserWebUrl,
            /// <summary>
            /// 
            /// </summary>
            Media_Writer,
            /// <summary>
            /// 
            /// </summary>
            Media_Year,
            /// <summary>
            /// 
            /// </summary>
            Music_AlbumArtist,
            /// <summary>
            /// 
            /// </summary>
            Music_AlbumTitle,
            /// <summary>
            /// 
            /// </summary>
            Music_Artist,
            /// <summary>
            /// 
            /// </summary>
            Music_BeatsPerMinute,
            /// <summary>
            /// 
            /// </summary>
            Music_Composer,
            /// <summary>
            /// 
            /// </summary>
            Music_Conductor,
            /// <summary>
            /// 
            /// </summary>
            Music_ContentGroupDescription,
            /// <summary>
            /// 
            /// </summary>
            Music_Genre,
            /// <summary>
            /// 
            /// </summary>
            Music_InitialKey,
            /// <summary>
            /// 
            /// </summary>
            Music_Lyrics,
            /// <summary>
            /// 
            /// </summary>
            Music_Mood,
            /// <summary>
            /// 
            /// </summary>
            Music_PartOfSet,
            /// <summary>
            /// 
            /// </summary>
            Music_Period,
            /// <summary>
            /// 
            /// </summary>
            Music_SynchronizedLyrics,
            /// <summary>
            /// 
            /// </summary>
            Music_TrackNumber,
            /// <summary>
            /// The aperture value of the image, in APEX units. See the Exchangeable Image File (EXIF) 2.2 specification, Annex C, for a comparison of 
            /// </summary>
            Photo_Aperture,
            /// <summary>
            /// The denominator of 
            /// </summary>
            Photo_ApertureDenominator,
            /// <summary>
            /// The date when the photo was taken, as read from the camera in the file's Exchangeable Image File (EXIF) tag.
            /// </summary>
            Photo_ApertureNumerator,
            /// <summary>
            /// The brightness value of the image, in APEX units, usually in the range of -99.99 to 99.99.
            /// </summary>
            Photo_Brightness,
            /// <summary>
            /// The brightness value of the image, in APEX units, usually in the range of -99.99 to 99.99.
            /// </summary>
            Photo_BrightnessDenominator,
            /// <summary>
            /// The numerator of PKEY_Photo_Brightness.
            /// </summary>
            Photo_BrightnessNumerator,
            /// <summary>
            /// The manufacturer name of the camera that took the photo, in a string format.
            /// </summary>
            Photo_CameraManufacturer,
            /// <summary>
            /// The model name of the camera that shot the photo, in string form.
            /// </summary>
            Photo_CameraModel,
            /// <summary>
            /// The serial number of the camera that produced the photo.
            /// </summary>
            Photo_CameraSerialNumber,
            /// <summary>
            /// Indicates the direction of contrast processing applied by the camera when the image was taken. "0" indicates "Normal"; "1" indicates "Soft"; "2" indicates "Hard".
            /// </summary>
            Photo_Contrast,
            /// <summary>
            /// The user-friendly form of System.Photo.Contrast. It is not intended to be parsed programmatically.
            /// </summary>
            Photo_ContrastText,
            /// <summary>
            /// The date when the photo was taken, as read from the camera in the file's Exchangeable Image File (EXIF) tag.
            /// </summary>
            Photo_DateTaken,
            /// <summary>
            /// The digital zoom ratio when the image was shot. Read from the camera in the file's Exchangeable Image File (EXIF) information. This property is calculated from 
            /// </summary>
            Photo_DigitalZoom,
            /// <summary>
            /// The denominator of PKEY_Photo_DigitalZoom.
            /// </summary>
            Photo_DigitalZoomDenominator,
            /// <summary>
            /// The numerator of PKEY_Photo_DigitalZoom.
            /// </summary>
            Photo_DigitalZoomNumerator,
            /// <summary>
            /// The event where the photo was taken. The end-user provides this value.
            /// </summary>
            Photo_Event,
            /// <summary>
            /// The Exchangeable Image File (EXIF) version.
            /// </summary>
            Photo_EXIFVersion,
            /// <summary>
            /// The amount of exposure bias used in the photo, as read from the camera. This property is calculated from 
            /// </summary>
            Photo_ExposureBias,
            /// <summary>
            /// The denominator of PKEY_Photo_ExposureBias.
            /// </summary>
            Photo_ExposureBiasDenominator,
            /// <summary>
            /// The numerator of PKEY_Photo_ExposureBias.
            /// </summary>
            Photo_ExposureBiasNumerator,
            /// <summary>
            /// Indicates the exposure index selected on the camera or input device at the time the photo was taken. Calculated from PKEY_Photo_ExposureIndexNumerator and PKEY_Photo_ExposureIndexDenominator. 
            /// </summary>
            Photo_ExposureIndex,
            /// <summary>
            /// the denominator of PKEY_Photo_ExposureIndex.
            /// </summary>
            Photo_ExposureIndexDenominator,
            /// <summary>
            /// The numerator of PKEY_Photo_ExposureIndex.
            /// </summary>
            Photo_ExposureIndexNumerator,
            /// <summary>
            /// The Exposure Program mode of the camera at the time the photo was taken, as read from the Exchangeable Image File (EXIF) information.
            /// </summary>
            Photo_ExposureProgram,
            /// <summary>
            /// The user-friendly form of System.Photo.ExposureProgram. Not intended to be parsed programmatically.
            /// </summary>
            Photo_ExposureProgramText,
            /// <summary>
            /// The exposure time for the photo, in seconds, as read from the Exchangeable Image File (EXIF) information. This property is calculated from 
            /// </summary>
            Photo_ExposureTime,
            /// <summary>
            /// The denominator of PKEY_Photo_ExposureTime.
            /// </summary>
            Photo_ExposureTimeDenominator,
            /// <summary>
            /// The numerator of PKEY_Photo_ExposureTime.
            /// </summary>
            Photo_ExposureTimeNumerator,
            /// <summary>
            /// An indicator of the flash status when the photo was taken, as read from the Exchangeable Image File (EXIF) info.
            /// </summary>
            Photo_Flash,
            /// <summary>
            /// Indicates the strobe energy at the time the image was captured, measured in Beam Candle Power Seconds. Calculated from PKEY_Photo_FlashEnergyNumerator and PKEY_Photo_FlashEnergyDenominator. 
            /// </summary>
            Photo_FlashEnergy,
            /// <summary>
            /// The denominator of PKEY_Photo_FlashEnergy.
            /// </summary>
            Photo_FlashEnergyDenominator,
            /// <summary>
            /// The numerator of PKEY_Photo_FlashEnergy.
            /// </summary>
            Photo_FlashEnergyNumerator,
            /// <summary>
            /// A string indicating the manufacturer of the flash used to take the picture. Can be blank or not present.
            /// </summary>
            Photo_FlashManufacturer,
            /// <summary>
            /// The amount of exposure bias used in the photo, as read from the camera. This property is calculated from 
            /// </summary>
            Photo_FlashModel,
            /// <summary>
            /// The user-friendly form of 
            /// </summary>
            Photo_FlashText,
            /// <summary>
            /// The FNumber value when the photo was taken, as read from the Exchangeable Image File (EXIF) information.This property is calculated from 
            /// </summary>
            Photo_FNumber,
            /// <summary>
            /// The denominator of PKEY_Photo_FNumber.
            /// </summary>
            Photo_FNumberDenominator,
            /// <summary>
            /// The numerator of PKEY_Photo_FNumber.
            /// </summary>
            Photo_FNumberNumerator,
            /// <summary>
            /// The focal length of the lens as recorded by the camera when the photo was taken, measured in millimeters. This is the actual focal length without conversion to 35mm (
            /// </summary>
            Photo_FocalLength,
            /// <summary>
            /// The denominator of PKEY_Photo_FocalLength.
            /// </summary>
            Photo_FocalLengthDenominator,
            /// <summary>
            /// The focal length of the lens when the photo was taken, as converted to a 35mm film measurement.
            /// </summary>
            Photo_FocalLengthInFilm,
            /// <summary>
            /// The numerator of PKEY_Photo_FocalLength.
            /// </summary>
            Photo_FocalLengthNumerator,
            /// <summary>
            /// Indicates the number of pixels in the image width (X direction) per FocalPlaneResolutionUnit on the camera focal plane. Calculated from PKEY_Photo_FocalPlaneXResolutionNumerator and PKEY_Photo_FocalPlaneXResolutionDenominator.
            /// </summary>
            Photo_FocalPlaneXResolution,
            /// <summary>
            /// The denominator of PKEY_Photo_FocalPlaneXResolution.
            /// </summary>
            Photo_FocalPlaneXResolutionDenominator,
            /// <summary>
            /// The numerator of PKEY_Photo_FocalPlaneXResolution.
            /// </summary>
            Photo_FocalPlaneXResolutionNumerator,
            /// <summary>
            /// Indicates the number of pixels in the image height (Y direction) per FocalPlaneResolutionUnit on the camera focal plane. Calculated from PKEY_Photo_FocalPlaneYResolutionNumerator and PKEY_Photo_FocalPlaneYResolutionDenominator.
            /// </summary>
            Photo_FocalPlaneYResolution,
            /// <summary>
            /// The denominator of PKEY_Photo_FocalPlaneYResolution.
            /// </summary>
            Photo_FocalPlaneYResolutionDenominator,
            /// <summary>
            /// The numerator of PKEY_Photo_FocalPlaneYResolution.
            /// </summary>
            Photo_FocalPlaneYResolutionNumerator,
            /// <summary>
            /// Indicates the degree of overall image gain adjustment. Calculated from PKEY_Photo_GainControlNumerator and PKEY_Photo_GainControlDenominator.
            /// </summary>
            Photo_GainControl,
            /// <summary>
            /// The denominator of PKEY_Photo_GainControl.
            /// </summary>
            Photo_GainControlDenominator,
            /// <summary>
            /// The numerator of PKEY_Photo_GainControl.
            /// </summary>
            Photo_GainControlNumerator,
            /// <summary>
            /// The user-friendly form of 
            /// </summary>
            Photo_GainControlText,
            /// <summary>
            /// The International Standards Organization (ISO) speed as recorded by the camera when the photo was taken.
            /// </summary>
            Photo_ISOSpeed,
            /// <summary>
            /// String indicating the manufacturer of the lens used to take the picture. Can be blank or not present.
            /// </summary>
            Photo_LensManufacturer,
            /// <summary>
            /// A string indicating the model of the lens used to take the picture. Can be blank or not present.
            /// </summary>
            Photo_LensModel,
            /// <summary>
            /// The light source when the photo was taken, as read from the Exchangeable Image File (EXIF) information.
            /// </summary>
            Photo_LightSource,
            /// <summary>
            /// The Exchangeable Image File (EXIF) extensibility mechanism that allows camera manufacturers to provide custom information. This property is not intended to be displayed in the Shell, but it is available programmatically to applications.
            /// </summary>
            Photo_MakerNote,
            /// <summary>
            /// The offset for the maker note specified in 
            /// </summary>
            Photo_MakerNoteOffset,
            /// <summary>
            /// The maximum aperture of the lens as recorded by the camera, taken from the Exchangeable Image File (EXIF) information. This property is calculated from 
            /// </summary>
            Photo_MaxAperture,
            /// <summary>
            /// The denominator of PKEY_Photo_MaxAperture.
            /// </summary>
            Photo_MaxApertureDenominator,
            /// <summary>
            /// The numerator of PKEY_Photo_MaxAperture.
            /// </summary>
            Photo_MaxApertureNumerator,
            /// <summary>
            /// The metering mode used by the camera, taken from the Exchangeable Image File (EXIF) information.
            /// </summary>
            Photo_MeteringMode,
            /// <summary>
            /// The user-friendly form of 
            /// </summary>
            Photo_MeteringModeText,
            /// <summary>
            /// The orientation of the photo when it was taken, as specified in the Exchangeable Image File (EXIF) information and in terms of rows and columns. This allows applications and the Shell to properly orient the image, instead of orienting the pixels and persisting the image in the requested display orientation, which can result in a loss of fidelity. This property is not meant to be displayed in the user interface (UI).
            /// </summary>
            Photo_Orientation,
            /// <summary>
            /// The user-friendly form of 
            /// </summary>
            Photo_OrientationText,
            /// <summary>
            /// The pixel composition. In JPEG compressed data, a JPEG marker is used instead of this property.
            /// </summary>
            Photo_PhotometricInterpretation,
            /// <summary>
            /// The user-friendly form of 
            /// </summary>
            Photo_PhotometricInterpretationText,
            /// <summary>
            /// The class of the program used by the camera to set exposure.
            /// </summary>
            Photo_ProgramMode,
            /// <summary>
            /// The user-friendly form of 
            /// </summary>
            Photo_ProgramModeText,
            /// <summary>
            /// The file name of a sound annotation file associated with the photo.
            /// </summary>
            Photo_RelatedSoundFile,
            /// <summary>
            /// Indicates the direction of saturation processing applied by the camera when the photo was taken.
            /// </summary>
            Photo_Saturation,
            /// <summary>
            /// The user-friendly form of 
            /// </summary>
            Photo_SaturationText,
            /// <summary>
            /// Indicates the direction of sharpness processing applied by the camera when the photo was taken.
            /// </summary>
            Photo_Sharpness,
            /// <summary>
            /// The user-friendly form of 
            /// </summary>
            Photo_SharpnessText,
            /// <summary>
            /// The shutter speed of the camera when the photo was taken. This is given in APEX units. This property is calculated from 
            /// </summary>
            Photo_ShutterSpeed,
            /// <summary>
            /// The denominator of PKEY_Photo_ShutterSpeed.
            /// </summary>
            Photo_ShutterSpeedDenominator,
            /// <summary>
            /// The numerator of PKEY_Photo_ShutterSpeed.
            /// </summary>
            Photo_ShutterSpeedNumerator,
            /// <summary>
            /// The distance to the subject in meters. Calculated from PKEY_Photo_SubjectDistanceNumerator and PKEY_Photo_SubjectDistanceDenominator.
            /// </summary>
            Photo_SubjectDistance,
            /// <summary>
            /// The denominator of PKEY_Photo_SubjectDistance.
            /// </summary>
            Photo_SubjectDistanceDenominator,
            /// <summary>
            /// The numerator of PKEY_Photo_SubjectDistance.
            /// </summary>
            Photo_SubjectDistanceNumerator,
            /// <summary>
            /// A VT_BOOL that indicates whether the image has been transcoded for synchronizing with an external device.
            /// </summary>
            Photo_TranscodedForSync,
            /// <summary>
            /// The white balance mode at the time the photo was shot, as taken from the Exchangeable Image File (EXIF) information.
            /// </summary>
            Photo_WhiteBalance,
            /// <summary>
            /// The white balance mode at the time the photo was shot, as taken from the Exchangeable Image File (EXIF) information.
            /// </summary>
            Photo_WhiteBalanceText,
            /// <summary>
            /// An automated search system summary of the full text contents of a document, displayed in the search results view in 
            /// </summary>
            Search_AutoSummary,
            /// <summary>
            /// Relevance rank of row, with a range from 0-1000. Larger numbers mean better matches. Query-time only; not defined in Search schema. This property is retrievable but not searchable.
            /// </summary>
            Search_Rank,
            /// <summary>
            /// Indicates the level of compression for the video stream. "Compression". 
            /// </summary>
            Video_Compression,
            /// <summary>
            /// Indicates the person who directed the video. 
            /// </summary>
            Video_Director,
            /// <summary>
            /// Indicates the data rate in "bits per second" for the video stream. "DataRate".
            /// </summary>
            Video_EncodingBitrate,
            /// <summary>
            /// Indicates the 4CC for the video stream.
            /// </summary>
            Video_FourCC,
            /// <summary>
            /// Indicates the frame height for the video stream.
            /// </summary>
            Video_FrameHeight,
            /// <summary>
            /// Indicates the frame rate in "frames per millisecond" for the video stream. "FrameRate".
            /// </summary>
            Video_FrameRate,
            /// <summary>
            /// Indicates the frame width for the video stream.
            /// </summary>
            Video_FrameWidth,
            /// <summary>
            /// Indicates the horizontal portion of the aspect ratio. The X portion of XX:YY. For example, 16 is the X portion of 16:9.
            /// </summary>
            Video_HorizontalAspectRatio,
            /// <summary>
            /// Indicates the sample size in bits for the video stream. "SampleSize".
            /// </summary>
            Video_SampleSize,
            /// <summary>
            /// Indicates the name for the video stream. "StreamName".
            /// </summary>
            Video_StreamName,
            /// <summary>
            /// Indicates the ordinal number of the stream being played.
            /// </summary>
            Video_StreamNumber,
            /// <summary>
            /// Indicates the total data rate in "bits per second" for all video and audio streams.
            /// </summary>
            Video_TotalBitrate,
            /// <summary>
            /// Indicates the vertical portion of the aspect ratio. The Y portion of XX:YY. For example, nine is the Y portion of 16:9.
            /// </summary>
            Video_VerticalAspectRatio

        }

        /// <summary> 
        /// Specifies the mechanism by which the file system is searched. 
        /// </summary>

        public enum SearchModes
        {
            /// <summary>
            /// Searches the file system directly. No content searching is available using this method.
            /// </summary>
            FileSystem,
            /// <summary>
            /// Searches the file system using the Indexing Service. The correct catalog must be specified in the <see cref="DbNetLink.DbNetSuiteVS.DbNetFile.IndexingServiceCatalog">IndexingServiceCatalog</see> property
            /// </summary>
            IndexingService,
            /// <summary>
            /// Searches the file system using windows search
            /// </summary>
            WindowsSearch
        }

        /// <summary> 
        /// Specifies the action when a selectable file link is clicked. 
        /// </summary>
        public enum FileSelectionActions
        {
            /// <summary>
            /// Displays the file in a new window 
            /// </summary>
            Display,
            /// <summary>
            /// Prompts the user to download the window 
            /// </summary>
            Download,
            /// <summary>
            /// Displays the file in the Preview Dialog 
            /// </summary>
            Preview,
            /// <summary>
            /// No action is invoked
            /// </summary>
            None
        }

        #endregion

        #region Private Properties

        internal ArrayList WindowsSearchColumns = new ArrayList();
        internal ArrayList IndexingServiceColumns = new ArrayList();

        private bool ColumnsSpecfied = true;
        internal DataTable FileTable;

        #endregion

        #region Public Properties
        [
        CategoryAttribute("Configuration"),
        DefaultValue(false),
        Description("Allow deletion of non-empty folders")
        ]
        public bool AllowFolderDeletion = false;

        internal FileColumnCollection Columns = new FileColumnCollection();
        [
        CategoryAttribute("Display"),
        DefaultValue(SearchModes.FileSystem),
        Description("Specifies the mode used to browse the file system.")
        ]
        public SearchModes BrowseMode = SearchModes.FileSystem;

        [
        CategoryAttribute("Display"),
        DefaultValue(false),
        Description("Creates the RootFolder if it does not exist")
        ]
        public bool CreateFolder = false;

        [
        CategoryAttribute("Display"),
        Description("Assigns custom mime type to a file type")
        ]
        public Dictionary<string, object> CustomMimeTypes = new Dictionary<string, object>();

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds delete button to the toolbar")
        ]
        public bool DeleteRow = false;

        [
        CategoryAttribute("Display"),
        DefaultValue(SelectionModes.FoldersAndFiles),
        Description("File/folder selection mode")
        ]
        public SelectionModes SelectionMode = SelectionModes.FoldersAndFiles;
   
        [
        CategoryAttribute("Display"),
        DefaultValue(DisplayStyles.Grid),
        Description("File/folder display style")
        ]
        public DisplayStyles DisplayStyle = DisplayStyles.Grid;

        [
        CategoryAttribute("Display"),
        DefaultValue(FileSelectionActions.Preview),
        Description("Default file selection action")
        ]
        public FileSelectionActions FileSelectionAction = FileSelectionActions.Preview;

        [
        CategoryAttribute("Display"),
        DefaultValue(ToolbarLocation.Top),
        Description("Toolbar location")
        ]
        public ToolbarLocation FolderPathLocation = ToolbarLocation.Top;
        
        [
        CategoryAttribute("Display"),
        DefaultValue(true),
        Description("Display column headings")
        ]
        public bool HeaderRow = true;

        [
        CategoryAttribute("Display"),
        Description("Control height")
        ]
        public Unit Height = Unit.Empty;

        [
        CategoryAttribute("Display"),
        Description("Indexing service catalog name")
        ]
        public string IndexingServiceCatalog = "system";

        [
        CategoryAttribute("Display"),
        DefaultValue(20),
        Description("Number files/folders per page")
        ]
        public int PageSize = 20;

        [
        CategoryAttribute("Display"),
        DefaultValue(100),
        Description("Maximum number of matches found when searching the file system")
        ]
        public int MaxSearchMatches = 100;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Adds the navigation buttons to the toolbar.")
        ]
        public bool Navigation = true;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Add create folder button to the toolbar")
        ]
        public bool NewFolder = false;

        [
        CategoryAttribute("Configuration"),
        Description("Height of file preview dialog")
        ]
        public int PreviewDialogHeight = 400;

        [
        CategoryAttribute("Configuration"),
        Description("Width of file preview dialog")
        ]
        public int PreviewDialogWidth = 400;

        [
        CategoryAttribute("Configuration"),
        Description("Root folder path")
        ]
        public string RootFolder = null;

        [
        CategoryAttribute("Configuration"),
        Description("Root folder alias")
        ]
        public string RootFolderAlias = "";

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Add search button to the toolbar")
        ]
        public bool Search = true;

        [
        CategoryAttribute("Configuration"),
        DefaultValue(SearchModes.FileSystem),
        Description("Mode used to search the file system")
        ]
        public SearchModes SearchMode = SearchModes.FileSystem;

        [
        CategoryAttribute("Configuration"),
        DefaultValue(""),
        Description("Mode used to search the file system")
        ]
        public string SelectableFileTypes = "";

        [
        CategoryAttribute("Configuration"),
        DefaultValue(0),
        Description("Thumbnail size as a percentage")
        ]
        public int ThumbnailPercent = 0;

        [
        CategoryAttribute("Configuration"),
        DefaultValue(0),
        Description("Thumbnail height in pixels")
        ]
        public int ThumbnailHeight = 0;

        [
        CategoryAttribute("Configuration"),
        DefaultValue(0),
        Description("Thumbnail width in pixels")
        ]
        public int ThumbnailWidth = 0;

        [
        CategoryAttribute("Configuration"),
        DefaultValue("Name"),
        Description("Column type by which to order the data")
        ]
        public string OrderBy = "Name";

        [
        Category("Toolbar"),
        Description("Sets the style of the toolbar button"),
        DefaultValue(ToolButtonStyles.Image)
        ]
        public ToolButtonStyles ToolbarButtonStyle = ToolButtonStyles.Image;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Add upload button to the toolbar")
        ]
        public bool Upload = false;

        [
        CategoryAttribute("Configuration"),
        DefaultValue(false),
        Description("Allow file to overwritten when uploading")
        ]
        public bool UploadOverwrite = false;

        [
        CategoryAttribute("Configuration"),
        DefaultValue(""),
        Description("File types that can be uploaded")
        ]
        public string UploadFileTypes = "";

        [
        CategoryAttribute("Configuration"),
        DefaultValue(1024),
        Description("Maximum size of upload file in KB")
        ]
        public int UploadMaxFileSizeKb = 1024;

        [
        CategoryAttribute("Configuration"),
        DefaultValue(""),
        Description("Displayed file types")
        ]
        public string VisibleFileTypes = "";

        [
        CategoryAttribute("Configuration"),
        Description("Width of control")
        ]
        public Unit Width = Unit.Empty;

        [
        CategoryAttribute("Configuration"),
        DefaultValue("Provider=Search.CollatorDSO;Extended Properties='Application=Windows';"),
        Description("Width of control")
        ]
        public string WindowsSearchConnectionString = "Provider=Search.CollatorDSO;Extended Properties='Application=Windows';";

        [
        CategoryAttribute("Configuration"),
        DefaultValue(""),
        Description("Filter applied to file selection")
        ]
        public string FileFilter = String.Empty;

        [
        CategoryAttribute("Configuration"),
        DefaultValue(""),
        Description("Filter applied to folder selection")
        ]
        public string FolderFilter = String.Empty;

        internal List<object> FileInfoList = new List<object>();
        public string CurrentFolder = "";
        public int CurrentPage = 0;
        public int TotalPages = 0;
        public int TotalRows = 0;

        #endregion

        ///////////////////////////////////////////////
        public override void ProcessRequest(HttpContext context)
        ///////////////////////////////////////////////
        {
            base.ProcessRequest(context);

            switch (Req["method"].ToString())
            {
                case "initialize":
                    ConfigureColumns();
                    this.AssignColumnCollections();
                    this.AssignWindowsSearchDataTypes();
                    BuildToolbar();
                    this.BuildFileList();
                    ClientProperties["columns"] = SerialiseColumns();
                    break;
                case "load-data":
                    this.AssignColumnCollections();
                    this.BuildFileList();
                    break;
                case "search-dialog":
                    Resp["html"] = BuildFileSearchDialog();
                    break;
                case "new-folder-dialog":
                    Resp["html"] = BuildNewFolderDialog();
                    break;
                case "search-results-dialog":
                    Resp["html"] = BuildFileSearchResultsDialog();
                    break;
                case "validate-search-params":
                    ValidateSearchParams();
                    break;
                case "run-search":
                    RunSearch();
                    break;
                case "stream":
                    StreamFile();
                    break;
                case "document-size":
                    GetDocumentSize();
                    break;
                case "thumbnail":
                    StreamThumbnail();
                    break;
                case "validate-upload":
                    ValidateUpload();
                    break;
                case "preview-dialog":
                    Resp["html"] = BuildFilePreviewDialog();
                    break;
                case "upload":
                case "ajax-upload":
                    Upload();
                    break;
                case "file-info":
                    FileInfo();
                    break;
                case "delete-file":
                    DeleteFile();
                    break;
                case "tree":
                    BuildTree();
                    break;
                case "create-folder":
                    CreateNewFolder();
                    break;
            }

            if (Context.Request.RequestType == "POST")
            {
                switch (Req["method"].ToString())
                {
                    case "upload":
                        break;
                    default:
                        context.Response.Write(JSON.Serialize(Resp));
                        break;
                }
            }
          }

        ///////////////////////////////////////////////
        private void BuildFileList()
        ///////////////////////////////////////////////
        {
            if (this.DisplayStyle == DisplayStyles.Grid)
                BuildGrid();
            else
                BuildTree();
        }

        ///////////////////////////////////////////////
        private void BuildTree()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.ID = this.AssignID("datatree"); ;
            T.CssClass = "dbnetfile tree";
            T.CellPadding = 0;
            T.CellSpacing = 0;

            DataView FileView = BuildDataSet();

            int RowIndex = 1;

            foreach (DataRowView Row in FileView)
            {
                T.Controls.Add(BuildTreeDataRow(Row, RowIndex));
                this.FileInfoList.Add(FileInformation(Row));
                RowIndex++;
            }

            Resp["html"] = RenderControlToString(T);
            Resp["fileInfoList"] = this.FileInfoList;
            ClientProperties["browseMode"] = this.BrowseMode;
            ClientProperties["searchMode"] = this.SearchMode;

        }


        ///////////////////////////////////////////////
        internal Dictionary<string,object> FileInformation(DataRowView Row)
        ///////////////////////////////////////////////
        {
            Dictionary<string, object> Info = new Dictionary<string, object>();

            foreach (DataColumn C in Row.DataView.Table.Columns)
            {
                Info.Add( ClientSideName(C.ColumnName), Row[C.ColumnName]);
            }
            return Info;
        }

        ///////////////////////////////////////////////
        internal TableRow BuildTreeDataRow(DataRowView Row, int RowIndex)
        ///////////////////////////////////////////////
        {
            TableRow TR = new TableRow();
            TR.CssClass = "tree-row data-row";
            TR.Attributes.Add("file", Row["name"].ToString());
            if (Row["recordType"].ToString() == "FileInfo")
                TR.Attributes.Add("folder", this.CurrentFolder);
            else if (this.CurrentFolder == "")
                TR.Attributes.Add("folder", Row["name"].ToString());
            else
                TR.Attributes.Add("folder", this.CurrentFolder + "/" + Row["name"].ToString());

            TR.Attributes.Add("recordType", Row["recordType"].ToString());
            TR.Attributes.Add("RowIdx", RowIndex.ToString());

            TableCell TC = new TableCell();
            TC.CssClass = "tree-image-cell";

            string LastNode = "";

            if ( RowIndex == Row.DataView.Count )
                LastNode = "-last";

            if (Row["recordType"].ToString() != "FileInfo")
                TC.CssClass += " tree" + LastNode + "-node-open folder-node";
            else
                TC.CssClass += " tree" + LastNode + "-node file-node";
            TC.Text = "&nbsp;";

            TR.Controls.Add(TC);

            TC = new TableCell();
            System.Web.UI.WebControls.Image I = new System.Web.UI.WebControls.Image();
            I.ImageUrl = GetFileTypeIconUrl(Row);
            TC.CssClass = "tree-icon-cell";
            TC.Controls.Add(I);
            TR.Controls.Add(TC);

            TC = new TableCell();
            TC.CssClass = "tree-text-cell data-cell";
            TC.Attributes.Add("columnType", "Name");
            MakeSelectableLink(TC, Row, Row["name"].ToString());
            TR.Controls.Add(TC);

            return TR;
        }

        ///////////////////////////////////////////////
        private void BuildToolbar()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.ID = AssignID("toolbar");
            T.CssClass = "toolbar";
            T.CellPadding = 0;
            T.CellSpacing = 0;

            T.Rows.Add(new TableRow());
            TableRow TR = T.Rows[0];

            if (this.Search)
                AddToolButton(TR, "search", "folder_find", "Search");

            if (this.Upload)
                AddToolButton(TR, "upload", "upload", "UploadDocument");

            if (this.DeleteRow)
                AddToolButton(TR, "deleteRow", "delete", "DeleteDocument");

            if (this.NewFolder)
                AddToolButton(TR, "newFolder", "NewFolder", "NewFolder");

            if (this.DisplayStyle == DisplayStyles.Grid && this.Navigation)
            {
                TableCell C = new TableCell();
                TR.Controls.Add(C);
                AddPageInformation(C);
            }

            Resp["toolbar"] = RenderControlToString(T);
        }


        ///////////////////////////////////////////////
        private void BuildGrid()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.ID = this.AssignID("datatable"); ;
            T.CssClass = "dbnetfile grid";

            if (this.GetTheme() == UI.Themes.Bootstrap)
                T.CssClass += " table";

            T.CellPadding = 3;

            if (this.FolderPathLocation == ToolbarLocation.Top)
                T.Controls.Add(BuildPathRow());

            if (this.HeaderRow)
                T.Controls.Add(BuildHeaderRow(false));

            int firstRecord = (PageSize * (this.CurrentPage - 1)) + 1;
            int lastRecord = (firstRecord + PageSize) - 1;

            DataView FileView = BuildDataSet();

            int RowIndex = 1;

            foreach (DataRowView Row in FileView)
            {
                if (RowIndex >= firstRecord && RowIndex <= lastRecord)
                {
                    T.Controls.Add(BuildDataRow(Row, RowIndex, false));
                    this.FileInfoList.Add(FileInformation(Row));
                }
                RowIndex++;
            }

            if (this.FolderPathLocation == ToolbarLocation.Bottom)
                T.Controls.Add(BuildPathRow());

            double pages = (double)FileView.Count / (double)PageSize;
            this.TotalPages = (int)Math.Ceiling(pages);
            this.TotalRows = FileView.Count;

            ClientProperties["totalRows"] = this.TotalRows;
            ClientProperties["totalPages"] = this.TotalPages;
            ClientProperties["currentPage"] = this.CurrentPage;

            Resp["html"] = RenderControlToString(T);
            Resp["fileInfoList"] = this.FileInfoList;
            ClientProperties["browseMode"] = this.BrowseMode;
            ClientProperties["searchMode"] = this.SearchMode;

        }

        ///////////////////////////////////////////////
        private DataView BuildDataSet()
        ///////////////////////////////////////////////
        {
            DirectoryInfo Dir = InitialiseFileSelection(CurrentFolder);

            if (Dir == null || !Dir.Exists)
                return new DataView(FileTable);

            switch (BrowseMode)
            {
                case SearchModes.WindowsSearch:
                    BrowseSystemIndexFolder(Dir);
                    break;
                case SearchModes.IndexingService:
                    BrowseIndexingService(Dir);
                    break;
                default:
                    BrowseFileSystemFolder(Dir);
                    break;
            }

            string Sort = "recordtype " + (this.OrderBy.ToString().Contains(" desc") ? "desc" : "asc");

            if (this.OrderBy.ToString() != "")
                Sort += ", " + this.OrderBy.ToString();

            List<string> Filter = new List<string>();

            if (FileFilter != String.Empty)
                Filter.Add("((" + FileFilter + ") and recordType = 'FileInfo')");
            else
                Filter.Add("recordType = 'FileInfo'");

            if (FolderFilter != String.Empty)
                Filter.Add("((" + FolderFilter + ") and recordType <> 'FileInfo')");
            else
                Filter.Add("recordType <> 'FileInfo'");

            return new DataView(FileTable, String.Join(" or ", Filter.ToArray()), Sort, DataViewRowState.CurrentRows);
        }


        ///////////////////////////////////////////////
        private void BrowseSystemIndexFolder(DirectoryInfo Dir)
        ///////////////////////////////////////////////
        {
            string FolderName = Regex.Replace(Dir.FullName, @"\\$", "");
            FolderName = Regex.Replace(FolderName, @"'", "''");

            string Sql = "select System.ItemType,System.ItemUrl, " + String.Join(",", (string[])WindowsSearchColumns.ToArray(typeof(string))) + " ";
            Sql += "from SYSTEMINDEX ";
            Sql += "where System.ItemFolderPathDisplay = '" + FolderName + "' ";

            switch (SelectionMode)
            {
                case SelectionModes.FoldersOnly:
                    Sql += " and System.ItemType = 'Directory'";
                    break;
                case SelectionModes.FilesOnly:
                    Sql += " and System.ItemType <> 'Directory'";
                    break;
            }

            OleDbConnection Connection = OpenConnection(WindowsSearchConnectionString);
            OleDbCommand Command = Connection.CreateCommand();
            Command.CommandText = Sql;

            OleDbDataReader Reader = null;

            try
            {
                Reader = Command.ExecuteReader();
            }
            catch (Exception Ex)
            {
                ThrowException(Ex.Message, "Windows Search Connection ==> " + WindowsSearchConnectionString + " Command Text ==> " + Sql);
            }

            foreach (string Column in WindowsSearchColumns)
                FileTable.Columns.Add(Column, Reader.GetFieldType(Reader.GetOrdinal(Column)));

            while (Reader.Read())
            {
                DataRow Row;

                string Path = Reader.GetValue(1).ToString().Replace("file:", "");

                if (Reader.GetValue(0).ToString() == "Directory")
                    Row = AddItemToRecordSet(new DirectoryInfo(Path));
                else
                    Row = AddItemToRecordSet(new FileInfo(Path));

                if (Row == null)
                    continue;

                foreach (string Column in WindowsSearchColumns)
                    Row[Column] = Reader.GetValue(Reader.GetOrdinal(Column));
            }

            Reader.Close();
            Connection.Close();
        }

        ///////////////////////////////////////////////
        private OleDbConnection OpenConnection(string ConnectionString)
        ///////////////////////////////////////////////
        {
            OleDbConnection Connection = new OleDbConnection(ConnectionString);
            try
            {
                Connection.Open();
            }
            catch (Exception Ex)
            {
                ThrowException(Ex.Message);
            }

            return Connection;
        }

        ///////////////////////////////////////////////
        private void BrowseIndexingService(DirectoryInfo Dir)
        ///////////////////////////////////////////////
        {
            string FolderName = Regex.Replace(Dir.FullName, @"\\$", "");
            FolderName = Regex.Replace(FolderName, @"'", "''");

            string Sql = "select  Path, " + String.Join(",", (string[])IndexingServiceColumns.ToArray(typeof(string))) + " ";
            Sql += " from scope('shallow traversal of \"" + FolderName + "\"')";

            string ConnectionString = "Provider=\"MSIDXS\";Data Source=\"" + this.IndexingServiceCatalog + "\";";
            OleDbConnection Connection = OpenConnection(ConnectionString);

            OleDbCommand Command = Connection.CreateCommand();
            Command.CommandText = Sql;
            OleDbDataReader Reader = null;

            try
            {
                Reader = Command.ExecuteReader();
            }
            catch (Exception Ex)
            {
                ThrowException(Ex.Message, "Indexing Service Connection ==> " + ConnectionString + " Command Text ==> " + Sql);
            }

            foreach (string Column in IndexingServiceColumns)
                FileTable.Columns.Add(Column, Reader.GetFieldType(Reader.GetOrdinal(Column)));

            if (SelectionMode != SelectionModes.FoldersOnly)
            {
                while (Reader.Read())
                {
                    DataRow Row = AddItemToRecordSet(new FileInfo(Reader.GetValue(0).ToString()));

                    if (Row != null)
                        foreach (string Column in IndexingServiceColumns)
                            Row[Column] = Reader.GetValue(Reader.GetOrdinal(Column));
                }
            }

            Reader.Close();
            Connection.Close();

            if (SelectionMode == SelectionModes.FoldersAndFiles || SelectionMode == SelectionModes.FoldersOnly)
                BuildRecordSet(Dir.GetDirectories());
        }

        ///////////////////////////////////////////////
        private void BrowseFileSystemFolder(DirectoryInfo Dir)
        ///////////////////////////////////////////////
        {
            if (SelectionMode == SelectionModes.FoldersAndFiles || SelectionMode == SelectionModes.FoldersOnly)
                BuildRecordSet(Dir.GetDirectories());
            if (SelectionMode == SelectionModes.FoldersAndFiles || SelectionMode == SelectionModes.FilesOnly)
                BuildRecordSet(Dir.GetFiles());
        }


        ///////////////////////////////////////////////
        private Control BuildPathRow()
        ///////////////////////////////////////////////
        {
            TableHeaderRow Row = new TableHeaderRow();
            Row.CssClass = "path-row";
            Row.ID = this.AssignID("pathrow");

            TableCell C = new TableCell();
            C.ColumnSpan = Columns.Count;
            C.Text = Translate("Path");
            Row.Controls.Add(C);

            Table T = new Table();
            C.Controls.Add(T);

            TableRow R = new TableRow();
            R.CssClass = "path_segment_row";
            T.Controls.Add(R);

            C = new TableCell();
            C.Text = "/";
            R.Controls.Add(C);

            System.Web.UI.WebControls.Image I = new System.Web.UI.WebControls.Image();
            C.Controls.Add(I);
            I.ImageUrl = GetImageUrl("parentfolder.png");

            I.ID = this.AssignID("parent_folder_img");
            I.CssClass = "path-segment";
            I.AlternateText = "Parent folder";

            string[] Folders = ("/" + this.GetRootFolderAlias() + "/" + CurrentFolder).Split('/');

            foreach (string Folder in Folders)
            {
                if (Folder == "")
                    continue;
                C = new TableCell();
                C.Text = "/";
                R.Controls.Add(C);

                C = new TableCell();
                C.Attributes.Add("folder", Folder);
                HyperLink H = new HyperLink();
                H.NavigateUrl = "#";
                H.Text = Folder;

                H.CssClass = "path-segment";
                C.Controls.Add(H);
                R.Controls.Add(C);
            }

            return Row;
        }

        ///////////////////////////////////////////////
        internal Control BuildHeaderRow(bool SearchResults)
        ///////////////////////////////////////////////
        {
            TableHeaderRow HR = new TableHeaderRow();
            HR.CssClass = "header-row";

            foreach (FileColumn FC in Columns)
            {
                if (SearchResults)
                {
                    if (!FC.SearchDisplay)
                        continue;
                }
                else
                    if (!FC.BrowseDisplay)
                        continue;

                TableHeaderCell HC = new TableHeaderCell();
                HC.ID = FC.ColumnType.ToString() + "_headerCell";
                HR.Controls.Add(HC);
                HC.CssClass = "header-cell";
                HC.Attributes.Add("columnName", FC.ColumnName);
                HC.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                HC.Attributes.Add("cellType", "headerCell");

                Table T = new Table();
                HC.Controls.Add(T);
                T.CellPadding = 0;
                T.CellSpacing = 0;

                TableRow R = new TableRow();
                T.Controls.Add(R);

                TableCell C = new TableCell();
                R.Controls.Add(C);
                C.Text = FC.Label + "&nbsp;&nbsp;";
                C.Width = Unit.Percentage(100);

                C = new TableCell();
                R.Controls.Add(C);

                Match M = Regex.Match(this.OrderBy, "^" + FC.ColumnName + " (asc|desc)");

                if (M.Success)
                {
                    C.CssClass = M.Groups[1].Value.ToLower() + "-sort-sequence-image";
                    C.Text = "&nbsp;";
                }

            }

            return HR;
        }


        ///////////////////////////////////////////////
        internal TableRow BuildDataRow(DataRowView Row, int RowIndex, bool SearchResults)
        ///////////////////////////////////////////////
        {
            TableRow TR = new TableRow();

            if (Page.IsCallback)
                TR.ID = this.ClientID + "_dataRow" + RowIndex;
            else
                TR.ID = "_dataRow" + RowIndex;


            TR.CssClass = (RowIndex % 2 == 0 ? "data-row even" : "data-row odd");
            TR.Attributes.Add("file", Row["name"].ToString());
            TR.Attributes.Add("recordType", Row["recordType"].ToString());
            TR.Attributes.Add("RowIdx", RowIndex.ToString());

            if (SearchResults)
                TR.Attributes.Add("folder", Row["folder"].ToString());

            foreach (FileColumn FC in Columns)
            {
                if (SearchResults)
                    if (!FC.SearchDisplay)
                        continue;

                BuildDataCell(Row, FC, TR, SearchResults);
            }

            return TR;
        }


        ///////////////////////////////////////////////
        private void BuildDataCell(DataRowView Row, FileColumn FC, TableRow TR, bool SearchResults)
        ///////////////////////////////////////////////
        {
            TableCell TC = new TableCell();
            TC.CssClass = "data-cell";
            TC.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            TC.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            TC.Attributes.Add("cellType", "TC");

            switch (FC.ColumnType)
            {
                case ColumnTypes.WindowsSearch:
                    TC.Attributes.Add("columnType", FC.WindowsSearchColumnType.ToString());
                    break;
                case ColumnTypes.IndexingService:
                    TC.Attributes.Add("columnType", FC.IndexingServiceColumnType.ToString());
                    break;
                default:
                    TC.Attributes.Add("columnType", FC.ColumnType.ToString());
                    break;
            }

            DataColumn Col = null;
            Object Value = null;

            if (Row.DataView.Table.Columns.Contains(FC.ColumnName))
            {
                Col = Row.DataView.Table.Columns[FC.ColumnName];
                Value = Row[FC.ColumnName];
            }

            //    if (FC.Selectable)
            //        if (String.IsNullOrEmpty((Value as string)))
            //            Value = Row["name"];


            switch (FC.ColumnType)
            {
                case ColumnTypes.Icon:
                    System.Web.UI.WebControls.Image I = new System.Web.UI.WebControls.Image();
                    I.ImageUrl = GetFileTypeIconUrl(Row);
                    I.AlternateText = "File type icon";
                    TC.Controls.Add(I);
                    break;
                case ColumnTypes.Size:
                    TC.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
                    if (Value.ToString() != "0")
                        TC.Text = Convert.ToInt32((Convert.ToInt32(Value) + 1024) / 1024).ToString() + " KB";
                    break;
                case ColumnTypes.Folder:
                    if (SearchResults)
                    {
                        HyperLink H = new HyperLink();
                        H.NavigateUrl = "#";
                        H.Text = Row["folder"].ToString();
                        H.CssClass = "parent-folder-link";
                        TC.Controls.Add(H);
                    }
                    else
                    {
                        TC.Text = this.CurrentFolder;
                    }
                    break;
                case ColumnTypes.Thumbnail:
                    switch (Row["type"].ToString().ToLower())
                    {
                        case "gif":
                        case "png":
                        case "jpg":
                        case "jpeg":
                        case "bmp":
                        case "tiff":
                        case "tif":
                            System.Web.UI.WebControls.Image Thumb = new System.Web.UI.WebControls.Image();
                            Thumb.ImageUrl = this.AssignHandler("dbnetfile.ashx?id=") + this.Id + "&method=thumbnail&filepath=" + HttpUtility.UrlEncode(Row["path"].ToString());

                            if (this.ThumbnailPercent > 0)
                                Thumb.ImageUrl += "&p=" + this.ThumbnailPercent.ToString();
                            if (this.ThumbnailWidth > 0)
                                Thumb.ImageUrl += "&w=" + this.ThumbnailWidth.ToString();
                            if (this.ThumbnailHeight > 0)
                                Thumb.ImageUrl += "&h=" + this.ThumbnailHeight.ToString();

                            TC.Controls.Add(Thumb);
                            Thumb.CssClass = "thumbnail";

                            /*
                            if (FC.Selectable)
                            {
                                HyperLink H = new HyperLink();
                                H.NavigateUrl = "#";
                                H.Controls.Add(Thumb);
                                H.CssClass = "file-link";
                                TC.Controls.Add(H);
                                Thumb.CssClass = "img-link";
                                Thumb.BorderStyle = BorderStyle.Solid;
                                Thumb.BorderWidth = new Unit("1px");
                                Thumb.BorderColor = Color.Blue;

                                Thumb.BorderStyle = BorderStyle.Solid;

                            }
                            else
                            {
                                TC.Controls.Add(Thumb);
                            }
                            */
                            break;
                    }
                    break;
                default:
                    if (Col == null || Value == null)
                        break;

                    switch (Value.GetType().FullName)
                    {
                        case "System.DateTime":
                            if (FC.Format == "")
                                FC.Format = "d";
                            if (FC.Selectable)
                                MakeSelectableLink(TC, Row, ((DateTime)Value).ToString(FC.Format));
                            else
                                TC.Text = ((DateTime)Value).ToString(FC.Format);
                            break;
                        case "System.DBNull":
                            break;
                        case "System.String":
                            if (FC.Selectable)
                                MakeSelectableLink(TC, Row, Value);
                            else
                                TC.Text = Value.ToString();
                            break;
                        case "System.String[]":
                            TC.Text = String.Join("<br>",Value as System.String[]);
                            break;
                        default:
                            TC.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
                            TC.Text = Value.ToString();
                            break;
                    }
                    break;
            }

            if (FC.BrowseDisplay || SearchResults)
                TR.Controls.Add(TC);
            else
                TR.Attributes.Add(FC.ColumnName, TC.Text);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal DirectoryInfo InitialiseFileSelection(string CurrentFolder)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            DirectoryInfo Folder = GetFolder(CurrentFolder);

            FileTable = new DataTable("FileTable");

            FileTable.Columns.Add("recordType", typeof(String));
            FileTable.Columns.Add("name", typeof(String));
            FileTable.Columns.Add("dateCreated", typeof(DateTime));
            FileTable.Columns.Add("size", typeof(Int64));
            FileTable.Columns.Add("dateLastAccessed", typeof(DateTime));
            FileTable.Columns.Add("dateLastModified", typeof(DateTime));
            FileTable.Columns.Add("type", typeof(String));
            FileTable.Columns.Add("path", typeof(String));
            FileTable.Columns.Add("subFolderCount", typeof(Int32));
            FileTable.Columns.Add("fileCount", typeof(Int32));
            FileTable.Columns.Add("folder", typeof(String));

            return Folder;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void MakeSelectableLink(TableCell TC, DataRowView Row, Object Value)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (!SelectableFileType(Row))
            {
                TC.Text = Value.ToString();
                return;
            }

            HyperLink H = new HyperLink();
            H.NavigateUrl = "#";
            H.Text = Value.ToString();

            if (Row["recordType"].ToString() == "FileInfo")
                H.CssClass = "file-link";
            else
                H.CssClass = "folder-link";

            TC.Controls.Add(H);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool SelectableFileType(DataRowView Row)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (Row["recordType"].ToString() == "FileInfo")
                if (GetSelectableFileTypes().Count > 0)
                    if (!GetSelectableFileTypes().Contains(Row["type"].ToString().ToLower()))
                        return false;
            return true;
        }

        ///////////////////////////////////////////////
        private ArrayList GetSelectableFileTypes()
        ///////////////////////////////////////////////
        {
            if (this.SelectableFileTypes.Replace(" ", "") == "")
                return new ArrayList();
            else
                return new ArrayList(this.SelectableFileTypes.Replace(" ", "").ToLower().Split(','));
        }

        ///////////////////////////////////////////////
        private ArrayList GetVisibleFileTypes()
        ///////////////////////////////////////////////
        {
            if (this.VisibleFileTypes.Replace(" ", "") == "")
                return new ArrayList();
            else
                return new ArrayList(this.VisibleFileTypes.Replace(" ", "").ToLower().Split(','));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private string GetFileTypeIconUrl(DataRowView Row)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string ResourceName = "";
            if (Row["recordType"].ToString() != "FileInfo")
                ResourceName = "FolderClosed.png";
            else
                ResourceName = "Ext." + Row["type"].ToString().ToLower() + ".gif";

            if (Assembly.GetExecutingAssembly().GetManifestResourceInfo("DbNetLink.Resources.Images." + ResourceName) == null)
                ResourceName = "Ext.default.gif";

            return this.GetImageUrl(ResourceName);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private DirectoryInfo GetFolder(string CurrentFolder)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string PhysicalPath = this.GetRootFolder();

            if (CurrentFolder != "")
            {
                PhysicalPath += @"\" + CurrentFolder.Replace("/", @"\");

                if (!Directory.Exists(PhysicalPath))
                    if (this.CreateFolder)
                        CreateDirectory(PhysicalPath);
            }

            DirectoryInfo Folder = null;

            try
            {
                Folder = new DirectoryInfo(PhysicalPath);
            }
            catch (Exception Error)
            {
                ThrowException("Error getting DirectoryInfo for: <b>" + PhysicalPath + "</b> [" + Error.Message + "]");
                return null;
            }

            if (CurrentFolder == "")
            {
                if (!Folder.Exists)
                    this.RootFolderAlias = "Root Folder <b>" + Folder.Name + "</b> does not exist";
                else
                {
                    try
                    {
                        Folder.GetDirectories();
                    }
                    catch (Exception Ex)
                    {
                        this.RootFolderAlias = "Error accessing root folder <b>" + Folder.Name + "</b> [" + Ex.Message + "]";
                        ThrowException(this.RootFolderAlias);
                        Folder = null;
                    }
                }
            }

            return Folder;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void BuildRecordSet(FileSystemInfo[] Items)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            foreach (FileSystemInfo Item in Items)
                AddItemToRecordSet(Item);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal DataRow AddItemToRecordSet(FileSystemInfo Item)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (Item is FileInfo)
                if (GetVisibleFileTypes().Count > 0)
                    if (!GetVisibleFileTypes().Contains(((FileInfo)Item).Extension.Replace(".", "").ToLower()))
                        return null;

            if ((Item.Attributes | FileAttributes.Hidden) == Item.Attributes)
                return null;

            DataRow Row = FileTable.NewRow();

            Row["recordType"] = Item.GetType().Name;
            Row["name"] = Item.Name;
            Row["dateCreated"] = Item.CreationTime;
            Row["size"] = FileSystemSize(Item);
            Row["dateLastAccessed"] = Item.LastAccessTime;
            Row["dateLastModified"] = Item.LastWriteTime;

            string ParentFolder = "";

            if (Item is DirectoryInfo)
            {
                ParentFolder = ((DirectoryInfo)Item).Parent.FullName;
                Row["type"] = "Folder";
            }
            else
            {
                ParentFolder = ((FileInfo)Item).DirectoryName;
                Row["type"] = ((FileInfo)Item).Extension.Replace(".", "").ToLower();
            }

            string Path = Item.FullName;

            Row["path"] = Path;

            Row["folder"] = Regex.Replace(ParentFolder, this.GetRootFolder().Replace(@"\", @"\\"), "", RegexOptions.IgnoreCase).Replace("\\", "/");
            if (Row["folder"].ToString() == "")
                Row["folder"] = "/";

            Row["subFolderCount"] = 0;
            Row["fileCount"] = 0;

            if (Item is DirectoryInfo)
            {
                try
                {
                    Row["subFolderCount"] = ((DirectoryInfo)Item).GetDirectories().Length;
                    Row["fileCount"] = ((DirectoryInfo)Item).GetFiles().Length;
                }
                catch (Exception)
                {
                }
            }

            FileTable.Rows.Add(Row);
            return Row;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal long FileSystemSize(FileSystemInfo Item)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (Item is FileInfo)
                return ((FileInfo)Item).Length;
            else
                return 0;
        }


        ///////////////////////////////////////////////
        private string GetDefaultRootFolder()
        ///////////////////////////////////////////////
        {
            if (this.DesignMode)
                return "";
            else
                return "~";
        }

        ///////////////////////////////////////////////
        internal string GetRootFolderAlias()
        ///////////////////////////////////////////////
        {
            if (this.RootFolderAlias != "")
                return this.RootFolderAlias;

            string RootFolder = GetRootFolder();
            return RootFolder.Split('\\')[RootFolder.Split('\\').Length - 1];
        }

        ///////////////////////////////////////////////
        internal string BuildFileSearchDialog()
        ///////////////////////////////////////////////
        {
            FileSearchDialog SD = new FileSearchDialog(this);
            return RenderControlToString(SD.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildNewFolderDialog()
        ///////////////////////////////////////////////
        {
            NewFolderDialog D = new NewFolderDialog(this);
            return RenderControlToString(D.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildFileSearchResultsDialog()
        ///////////////////////////////////////////////
        {
            FileSearchResultsDialog SD = new FileSearchResultsDialog(this);
            return RenderControlToString(SD.Build());
        }

        ///////////////////////////////////////////////
        internal void ValidateSearchParams()
        ///////////////////////////////////////////////
        {
            FileSearchDialog SD = new FileSearchDialog(this);
            SD.ValidateParameters();
        }

        ///////////////////////////////////////////////
        internal void RunSearch()
        ///////////////////////////////////////////////
        {
            FileSearchResultsDialog SD = new FileSearchResultsDialog(this);
            SD.RunSearch();
        }

        ///////////////////////////////////////////////
        internal string GetRootFolder()
        ///////////////////////////////////////////////
        {
            string FolderName = this.RootFolder;

            if (String.IsNullOrEmpty(FolderName))
                FolderName = HttpContext.Current.Request.QueryString["RootFolder"];

            if (String.IsNullOrEmpty(FolderName))
                FolderName = (string)GetSessionVariable("RootFolder");

            if (String.IsNullOrEmpty(FolderName))
                FolderName = "~";

            FolderName = FolderName.Replace("~", HttpContext.Current.Request.ApplicationPath);
            if (FolderName.StartsWith("/"))
                FolderName = HttpContext.Current.Request.MapPath(FolderName);

            if (!Directory.Exists(FolderName))
            {
                if (this.CreateFolder)
                    CreateDirectory(FolderName);
                else
                    this.ThrowException("RootFolder:<b>" + FolderName + "</b> does not exist");
            }

            return FolderName;
        }

        ///////////////////////////////////////////////
        internal object GetSessionVariable(string Key)
        ///////////////////////////////////////////////
        {
            if (Context.Session == null)
                throw new Exception("Session state is disabled");

            object Value = Context.Session[Req["id"].ToString() + Key];
            SetProperty(this,Key,Value);
            return Value;
        }

        ///////////////////////////////////////////////
        internal void SetSessionVariable(string Key)
        ///////////////////////////////////////////////
        {
            if (Context.Session == null)
                throw new Exception("Session state is disabled");

            FieldInfo FI = this.GetType().GetField(Key);
            Context.Session[Req["id"].ToString() + Key] = FI.GetValue(this);
        }

        ///////////////////////////////////////////////
        internal void AddDefaultColumns()
        ///////////////////////////////////////////////
        {
            if (Columns.Count == 0)
            {
                if (this.DisplayStyle == DisplayStyles.Grid)
                    Columns.Add(new FileColumn(DbNetFile.ColumnTypes.Icon));
                Columns.Add(new FileColumn(DbNetFile.ColumnTypes.Name, "File Name"));
                Columns[Columns.Count - 1].Selectable = true;
                ColumnsSpecfied = false;
            }
        }

        ///////////////////////////////////////////////
        protected void AssignColumnCollections()
        ///////////////////////////////////////////////
        {
            WindowsSearchColumns = new ArrayList();
            IndexingServiceColumns = new ArrayList();
            foreach (FileColumn C in Columns)
            {
                switch (C.ColumnType)
                {
                    case ColumnTypes.WindowsSearch:
                        WindowsSearchColumns.Add(C.ColumnName);
                        break;
                    case ColumnTypes.IndexingService:
                        IndexingServiceColumns.Add(C.ColumnName);
                        break;
                }
            }
        }

        ///////////////////////////////////////////////
        protected void ConfigureColumns()
        ///////////////////////////////////////////////
        {
            AddDefaultColumns();

            this.SetSessionVariable("RootFolder");
            this.SetSessionVariable("CustomMimeTypes");

            foreach (FileColumn C in Columns)
            {
                C.ColumnDataType = typeof(String).Name;
                C.ColumnID = C.ColumnType.ToString();

                switch (C.ColumnType)
                {
                    case ColumnTypes.WindowsSearch:
                        C.ColumnID = C.WindowsSearchColumnType.ToString();

                        if (IndexingServiceColumns.Count > 0)
                        {
                            C.ColumnType = ColumnTypes.Name;
                        }
                        else
                        {
                            C.ColumnName = C.WindowsSearchColumnType.ToString().Replace("_", ".");

                            if (!C.ColumnName.StartsWith("System."))
                                C.ColumnName = "System." + C.ColumnName;

                            if (C.Label == "")
                                C.Label = C.ColumnName.Split('.')[C.ColumnName.Split('.').Length - 1];
                            if (C.Search)
                                SearchMode = SearchModes.WindowsSearch;
                            if (C.BrowseDisplay)
                                BrowseMode = SearchModes.WindowsSearch;

                            switch (C.WindowsSearchColumnType)
                            {
                                case WindowsSearchColumnTypes.Search_AutoSummary:
                                case WindowsSearchColumnTypes.Search_Rank:
                                    C.Search = false;
                                    C.SearchDisplay = true;
                                    C.BrowseDisplay = false;
                                    break;
                            }
                        }
                        break;
                    case ColumnTypes.IndexingService:
                        C.ColumnID = C.IndexingServiceColumnType.ToString();
                        C.ColumnName = C.IndexingServiceColumnType.ToString();
                        if (WindowsSearchColumns.Count > 0)
                        {
                            C.ColumnType = ColumnTypes.Name;
                        }
                        else
                        {
                            IndexingServiceColumns.Add(C.ColumnName);
                            if (C.Label == "")
                                C.Label = GenerateLabel(C.ColumnName);
                            if (C.Search)
                                SearchMode = SearchModes.IndexingService;
                            if (C.BrowseDisplay)
                                BrowseMode = SearchModes.IndexingService;
                        }

                        if (C.ColumnName.EndsWith("Count"))
                            C.ColumnDataType = typeof(Int32).Name;

                        if (C.ColumnName.EndsWith("Printed"))
                            C.ColumnDataType = typeof(DateTime).Name;

                        break;
                    default:
                        switch (C.ColumnType)
                        {
                            case ColumnTypes.Icon:
                            case ColumnTypes.Thumbnail:
                            case ColumnTypes.Folder:
                                C.Search = false;
                                break;
                            case ColumnTypes.Size:
                                C.ColumnDataType = typeof(Int32).Name;
                                break;
                            case ColumnTypes.DateCreated:
                            case ColumnTypes.DateLastModified:
                            case ColumnTypes.DateLastAccessed:
                                C.ColumnDataType = typeof(DateTime).Name;
                                break;
                        }

                        C.ColumnName = C.ColumnType.ToString();

                        if (C.ColumnType != ColumnTypes.Icon)
                            if (C.Label == "")
                                C.Label = GenerateLabel(C.ColumnName);

                        break;
                }

                if (C.Search)
                    C.Search = (C.Label != "");
            }
        }

        ///////////////////////////////////////////////
        private void AssignWindowsSearchDataTypes()
        ///////////////////////////////////////////////
        {

            string ConnectionString = "";
            string Sql = "";

            switch (BrowseMode)
            {
                case SearchModes.WindowsSearch:
                    ConnectionString = WindowsSearchConnectionString;
                    break;
                default:
                    return;
            }

            OleDbConnection Connection;

            try
            {
                Connection = OpenConnection(ConnectionString);
            }
            catch (Exception Ex)
            {
                this.ThrowException("Error connecting to Windows Search [" + Ex.Message + "]<br><br>Please check that Windows Search has been installed on the server");
                return;
            }

            OleDbCommand Command = Connection.CreateCommand();
            OleDbDataReader Reader = null;

            foreach (string ColName in WindowsSearchColumns)
            {
                Sql = "select top 1 " + ColName + " from SystemIndex where " + ColName + " is not null";
                Command.CommandText = Sql;
                Reader = Command.ExecuteReader();

                if (Reader.Read())
                    foreach (FileColumn C in Columns)
                        if (Reader.GetName(0).ToUpper() == C.ColumnName.ToUpper())
                        {
                            C.ColumnDataType = Reader.GetValue(0).GetType().Name;
                            break;
                        }
                Reader.Close();
            }

            Connection.Close();
        }

        ///////////////////////////////////////////////
        private void StreamFile()
        ///////////////////////////////////////////////
        {
            string FilePath = Req["filepath"].ToString();

            if (!File.Exists(Req["filepath"].ToString()))
                FilePath = this.GetRootFolder() + "\\" + Req["filepath"].ToString();

            bool Download = Req.ContainsKey("download");

            Context.Response.Clear();

            FileInfo FI = new FileInfo(FilePath);

            string FileName = FI.Name;
            string Ext = FI.Extension.ToLower().Replace(".", "");

            LoadMimeTypes();

            GetSessionVariable("CustomMimeTypes");

            if (CustomMimeTypes != null)
            {
                foreach (string Key in this.CustomMimeTypes.Keys)
                    Shared.MimeTypes[Key] = this.CustomMimeTypes[Key].ToString();
            }

            if (Shared.MimeTypes[Ext] == null)
                Context.Response.ContentType = "text/plain";
            else
                Context.Response.ContentType = Shared.MimeTypes[Ext].ToString();

            if (Download)
            {
                Context.Response.AddHeader("content-disposition", "attachment; filename=\"" + FileName + "\"");
                Context.Response.WriteFile(FilePath);
            }
            else
            {
                FileStream S = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader R = new BinaryReader(S);
                Byte[] Data = R.ReadBytes(Convert.ToInt32(S.Length));
                S.Close();

                System.Drawing.Image Img;

                try
                {
                    MemoryStream MS = new MemoryStream(Data);
                    Img = System.Drawing.Image.FromStream(MS);
                    this.Context.Response.ContentType = "image/jpeg";
                    new System.Drawing.Bitmap(Img, Img.Width, Img.Height).Save(this.Context.Response.OutputStream, ImageFormat.Jpeg);
                }
                catch (Exception)
                {
                    Context.Response.AddHeader("content-disposition", "inline; filename=\"" + FileName + "\"");
                    this.Context.Response.BinaryWrite(Data);
                }

            }
            Context.Response.End();
        }


        ///////////////////////////////////////////////
        public void AddWatermark(string filename, string watermarkText, Stream outputStream)
        ///////////////////////////////////////////////
        {
            string s = Server.MapPath("original.jpg");
            string s2 = Server.MapPath("ikon.gif");

            System.Drawing.Image original = Bitmap.FromFile(s);
            Graphics gra = Graphics.FromImage(original);
            Bitmap logo = new Bitmap(s2);
            gra.DrawImage(logo, new Point(70, 70));

            Response.ContentType = "image/JPEG";
            original.Save(Response.OutputStream, ImageFormat.Jpeg);
        }


        ///////////////////////////////////////////////
        private void GetDocumentSize()
        ///////////////////////////////////////////////
        {
            this.SetSessionVariable("RootFolder");
            this.SetSessionVariable("CustomMimeTypes");

            string FilePath = Req["filepath"].ToString();

            if (!File.Exists(FilePath))
                FilePath = this.GetRootFolder() + "\\" + Req["filepath"].ToString();

            DbNetImage Img = new DbNetImage(FilePath);

            int DocumentHeight = -1;
            int DocumentWidth = -1;

            if (Img.Img != null)
            {
                DocumentHeight = Img.Img.Height;
                DocumentWidth = Img.Img.Width;
            }

            Resp["documentIsImage"] = (Img.Img != null);
            Resp["documentHeight"] = DocumentHeight;
            Resp["documentWidth"] = DocumentWidth;
        }
        
        ///////////////////////////////////////////////
        private void StreamThumbnail()
        ///////////////////////////////////////////////
        {
            DbNetImage Img = new DbNetImage(Req["filepath"].ToString());

            if (Req.ContainsKey("p"))
                Img.Percent = Convert.ToInt32(Req["p"]);
            if (Req.ContainsKey("w"))
                Img.Width = Convert.ToInt32(Req["w"]);
            if (Req.ContainsKey("h"))
                Img.Height = Convert.ToInt32(Req["h"]);

            if (Img.Percent + Img.Height + Img.Width == 0)
                Img.Percent = 10;

            Img.Thumbnail();
        }

        ///////////////////////////////////////////////
        private void DeleteFile()
        ///////////////////////////////////////////////
        {
            FileSystemInfo FSI = GetFileInformation(Req["selectedFile"].ToString());

            string Message = "";

            if (FSI != null)
            {
                try
                {
                    if (FSI is DirectoryInfo)
                    {
                        if ( ((FSI as DirectoryInfo).GetFiles().Length + (FSI as DirectoryInfo).GetDirectories().Length) > 0 )
                            if (!this.AllowFolderDeletion)
                            {
                                Resp["message"] = this.Translate("FolderNotEmpty");
                                return;
                            }
                           
                        Directory.Delete(FSI.FullName);
                    }
                    else
                        File.Delete(FSI.FullName);
                }
                catch (Exception Ex)
                {
                    Message = Ex.Message;
                }
            }

            Resp["message"] = Message;
        }

        ///////////////////////////////////////////////
        private void FileInfo()
        ///////////////////////////////////////////////
        {
            FileSystemInfo FSI = GetFileInformation(Req["selectedFile"].ToString());
            Dictionary<string, object> FileInfo = new Dictionary<string, object>();

            if (FSI != null)
            {
                FileInfo["type"] = FSI.GetType().Name;

                FileInfo["lastAccessTime"] = JsonValue(FSI.LastAccessTime);
                FileInfo["lastWriteTime"] = JsonValue(FSI.LastWriteTime);
                FileInfo["creationTime"] = JsonValue(FSI.CreationTime);
                FileInfo["name"] = JsonValue(FSI.Name);

                if (FSI is DirectoryInfo)
                {
                    FileInfo["files"] = JsonValue((FSI as DirectoryInfo).GetFiles().Length);
                    FileInfo["directories"] = JsonValue((FSI as DirectoryInfo).GetDirectories().Length);
                }
                else
                {
                    FileInfo["extension"] = JsonValue((FSI as FileInfo).Extension);
                    FileInfo["length"] = JsonValue((FSI as FileInfo).Length);
                }
            }

            Resp["fileInfo"] = FileInfo;
        }

        ///////////////////////////////////////////////
        protected void CreateNewFolder()
        ///////////////////////////////////////////////
        {
            string Message = "";
            string FolderName = Req["folderName"].ToString();
            string CurrentFolder = Req["currentFolder"].ToString();

            string Path = this.GetRootFolder();

            if (CurrentFolder != "")
                Path += @"\" + CurrentFolder;

            Path += @"\" + FolderName;

            Regex RE = new Regex("[/:*?\"<>|]");

            if (FolderName.IndexOf("\\") > -1 || RE.IsMatch(FolderName))
                Message = "Folder name cannot contain /\\:*?\"<>| characters";
            else if (Directory.Exists(Path))
                Message = "Folder already exists";
            else
            {
                try
                {
                    CreateDirectory(Path);
                }
                catch (Exception Ex)
                {
                    Message = Ex.Message;
                }
            }

            Resp["message"] = Message;
        }


        ///////////////////////////////////////////////
        public void AddWatermark(System.Drawing.Image WatermarkImg, System.Drawing.Image BackgroundImg, float Transparency)
        ///////////////////////////////////////////////
        {
            using (Graphics G = Graphics.FromImage(BackgroundImg))
            {

                Bitmap TransWatermark = new Bitmap(WatermarkImg.Width, WatermarkImg.Height);

                using (Graphics TG = Graphics.FromImage(TransWatermark))
                {
                    ColorMatrix CM = new ColorMatrix();
                    CM.Matrix33 = Transparency;

                    ImageAttributes TA = new ImageAttributes();
                    TA.SetColorMatrix(CM, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    TG.DrawImage(WatermarkImg, new Rectangle(0, 0, TransWatermark.Width, TransWatermark.Height), 0, 0, TransWatermark.Width, TransWatermark.Height, GraphicsUnit.Pixel, TA);
                }

                G.DrawImage(TransWatermark, 0, 0, BackgroundImg.Width, BackgroundImg.Height);
            }
        }


        ///////////////////////////////////////////////
        private FileSystemInfo GetFileInformation(string FileName)
        ///////////////////////////////////////////////
        {
            string FilePath = this.GetRootFolder();

            if (this.CurrentFolder != "")
                FilePath += "\\" + this.CurrentFolder.Replace("/", "\\");

            FilePath += "\\" + FileName;

            FileSystemInfo FSI = null;

            if (Directory.Exists(FilePath))
                FSI = new DirectoryInfo(FilePath);
            else if (File.Exists(FilePath))
                FSI = new FileInfo(FilePath);
            
            return FSI;
        }
    }


    /////////////////////////////////////////////// 
    public class FileColumn : Column
    ///////////////////////////////////////////////
    {
        internal string ColumnExpression = "";
        public string ColumnDataType;
        public string ColumnID = "";

        private DbNetFile.ColumnTypes _ColumnType;
        [
        CategoryAttribute("Properties"),
        Description("Column Type")
        ]
        public DbNetFile.ColumnTypes ColumnType
        {
            get { return _ColumnType; }
            set { _ColumnType = value; }
        }

        private DbNetFile.WindowsSearchColumnTypes _WindowsSearchColumnType = DbNetFile.WindowsSearchColumnTypes.Unassigned;
        [
        CategoryAttribute("Properties"),
        DefaultValue(DbNetFile.WindowsSearchColumnTypes.Unassigned),
        Description("Windows Search Column Type")
        ]
        public DbNetFile.WindowsSearchColumnTypes WindowsSearchColumnType
        {
            get { return _WindowsSearchColumnType; }
            set { _WindowsSearchColumnType = value; }
        } 

        private DbNetFile.IndexingServiceColumnTypes _IndexingServiceColumnType = DbNetFile.IndexingServiceColumnTypes.Unassigned;
        [
        CategoryAttribute("Properties"),
        DefaultValue(DbNetFile.IndexingServiceColumnTypes.Unassigned),
        Description("Indexing Service Column Type")
        ]
        public DbNetFile.IndexingServiceColumnTypes IndexingServiceColumnType
        {
            get { return _IndexingServiceColumnType; }
            set { _IndexingServiceColumnType = value; }
        } 

        private string _Format = "";
        [
        CategoryAttribute("Properties"),
        DefaultValue(""),
        Description("Format mask for date or numeric columns")
        ]
        public string Format
        {
            get { return _Format; }
            set { _Format = value; }
        } 

        private bool _BrowseDisplay = true;
        [
        CategoryAttribute("Properties"),
        DefaultValue(true),
        Description("Column displyed when browsing file system")
        ]
        public bool BrowseDisplay
        {
            get { return _BrowseDisplay; }
            set { _BrowseDisplay = value; }
        } 
        
        private bool _SearchDisplay = true;
        [
        CategoryAttribute("Properties"),
        DefaultValue(true),
        Description("Column displyed when showing search results")
        ]
        public bool SearchDisplay
        {
            get { return _SearchDisplay; }
            set { _SearchDisplay = value; }
        } 

        private bool _Search = true;
        [
        CategoryAttribute("Properties"),
        DefaultValue(true),
        Description("Column appears in the search dialog")
        ]
        public bool Search
        {
            get { return _Search; }
            set { _Search = value; }
        } 

        private bool _Selectable = false;
        [
        CategoryAttribute("Properties"),
        DefaultValue(false),
        Description("Column is displayed as a selectable link")
        ]
        public bool Selectable
        {
            get { return _Selectable; }
            set { _Selectable = value; }
        } 

        ///////////////////////////////////////////////
        public FileColumn(DbNetFile.ColumnTypes ColumnType)
        ///////////////////////////////////////////////
        {
            this.ColumnType = ColumnType;
        }

        ///////////////////////////////////////////////
        public FileColumn(DbNetFile.ColumnTypes ColumnType, string Label)
        ///////////////////////////////////////////////
        {
            this.ColumnType = ColumnType;
            this.Label = Label;
        }

        ///////////////////////////////////////////////
        public FileColumn()
        ///////////////////////////////////////////////
        {
        }
    }

    ///////////////////////////////////////////////
    public class FileColumnCollection : ColumnCollection
    ///////////////////////////////////////////////
    {
        public FileColumn this[int index]
        {
            get
            {
                return (FileColumn)this.List[index];
            }
            set
            {
                FileColumn column = (FileColumn)value;
                this.List[index] = column;
            }
        }

        public FileColumn this[string index]
        {
            get
            {
                foreach (FileColumn C in this)
                    if (C.ColumnName.ToLower() == index.ToLower())
                        return (FileColumn)C;
                return null;
            }
        }

        ///////////////////////////////////////////////
        public void Add(FileColumn column)
        ///////////////////////////////////////////////
        {
            this.List.Add(column);
        }

        ///////////////////////////////////////////////
        public FileColumn Add(DbNetFile.ColumnTypes ColumnType)
        ///////////////////////////////////////////////
        {
            FileColumn C = new FileColumn(ColumnType);
            base.Add(C);
            return C;
        }


        ///////////////////////////////////////////////
        public int IndexOf(FileColumn column)
        ///////////////////////////////////////////////
        {
            return this.List.IndexOf(column);
        }
    }

    #region DbNetImage

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    internal class DbNetImage
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    {
        private HttpRequest Request;
        private HttpResponse Response;
        private String ImagePath;
        internal System.Drawing.Image Img = null;
        private System.Drawing.Image NewImg;
        private System.Drawing.Imaging.ImageFormat ImgFormat;
        private String Ext;
        private String ImageType;

        public int Percent = 0;
        public int Width = 0;
        public int Height = 0;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public DbNetImage(string FilePath)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            Response = HttpContext.Current.Response;
            Request = HttpContext.Current.Request;
            ImagePath = FilePath;
            Ext = ImagePath.Split('.')[ImagePath.Split('.').GetLength(0) - 1].ToLower();
 
            try
            {
                FileStream S = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader R = new BinaryReader(S);
                Byte[] Buffer = R.ReadBytes(Convert.ToInt32(S.Length));
                S.Close();

                MemoryStream MS = new MemoryStream(Buffer);
                Img = System.Drawing.Image.FromStream(MS);
            }
            catch (Exception)
            {
            }


            /*
            switch (Ext)
            {
                case "gif":
                    ImageType = "gif";
                    ImgFormat = System.Drawing.Imaging.ImageFormat.Gif;
                    break;
                case "jpg":
                case "jpeg":
                    ImageType = "jpeg";
                    ImgFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;
                case "png":
                    ImageType = "png";
                    ImgFormat = System.Drawing.Imaging.ImageFormat.Png;
                    break;
                case "bmp":
                    ImageType = "bmp";
                    ImgFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                    break;
                case "tif":
                case "tiff":
                    ImageType = "tiff";
                    ImgFormat = System.Drawing.Imaging.ImageFormat.Tiff;
                    break;
                default:
                    ImageType = "";
                    ImagePath = null;
                    break;
            }
             */



        //    if (ImagePath != null)
        //        Img = (Bitmap)System.Drawing.Image.FromFile(ImagePath);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void ResizeImage()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            DoResize();
            WriteNewImage();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Thumbnail()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            int NewHeight = 0;
            int NewWidth = 0;

            if (Percent != 0)
            {
                NewHeight = (int)(Img.Height * Percent) / 100;
                NewWidth = (int)(Img.Width * Percent) / 100;
            }

            if (Height != 0)
            {
                NewHeight = Height;
                NewWidth = (int)(Img.Width * (NewHeight / (double)Img.Height));
                if (NewWidth == 0)
                    NewWidth = 1;
            }

            if (Width != 0)
            {
                NewWidth = Width;
                NewHeight = Convert.ToInt32(Img.Height * (NewWidth / (double)Img.Width));
                if (NewHeight == 0)
                    NewHeight = 1;
            }

            if (NewWidth + NewHeight > 0)
                NewImg = new Bitmap(Img, NewWidth, NewHeight);
            else
                NewImg = Img;
            WriteNewImage();
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void RotateImage()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            DoRotate();
            WriteNewImage();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void ResizeAndRotateImage()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            DoResize();
            DoRotate();
            WriteNewImage();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void DoResize()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            int Percent = Convert.ToInt32(Request.QueryString["percent"]);

            int NewHeight = (int)(Img.Height * Percent) / 100;
            int NewWidth = (int)(Img.Width * Percent) / 100;

            NewImg = new Bitmap(Img, NewWidth, NewHeight);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void DoRotate()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (NewImg == null)
                NewImg = new Bitmap(Img);

            int Degree = Convert.ToInt32(Request.QueryString["degree"]);

            switch (Degree)
            {
                case 90:
                    NewImg.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 180:
                    NewImg.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case 270:
                    NewImg.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void WriteNewImage()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            Response.Clear();
            Response.ContentType = "image/jpeg";
            NewImg.Save(Response.OutputStream, ImageFormat.Jpeg);
/*

            Response.ContentType = "image/" + ImageType;

            switch (Ext)
            {
                case "png":
                case "bmp":
                case "gif":
                case "jpg":
                case "jpeg":
                case "tif":
                case "tiff":
                    MemoryStream MS = new MemoryStream();
                    NewImg.Save(MS, ImageFormat.Png);
                    Response.BinaryWrite(MS.ToArray());
                    Response.End();
                    break;
                default:
                    NewImg.Save(Response.OutputStream, ImgFormat);
                    break;
            }
            Img.Dispose();
            NewImg.Dispose();
 */
        }

    #endregion
    }

}