using System;
using System.Security.Cryptography;
using System.Xml;
using System.Text;
#if (WEB_UI)
using System.Web;
#endif
#if (DBNETSUITEVS)
using System.Web;
#endif

using System.Net;
using System.IO;
using System.Configuration;
using System.Security.Permissions;
using System.Reflection;

#if (WEB_UI)
[assembly: ReflectionPermission(SecurityAction.RequestRefuse, Unrestricted=true)] 
namespace DbNetLink.Web.UI
#else
namespace DbNetLink
#endif

////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////
{
	////////////////////////////////////////////////////////////////////////////
	#if (WEB_UI)
	public class DbNetLicense
	#else
	public class DbNetLicense
	#endif
	////////////////////////////////////////////////////////////////////////////
	{
        protected enum Components
        {
		    DbNetCombo,
		    DbNetEdit,
		    DbNetFile,
		    DbNetGrid,
		    DbNetList,
		    DbNetSpell,
		    DbNetSuite,
		    DbNetCopy,
		    DbNetSuiteVS,
            DbNetTime,
            DbNetOffice
        }

		public string ComputerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
        private Components ComponentType;
        private static string HashSeed = "DbNetLink\tLimited";
        public string ComponentName;
		public string Version = "3";
        private int ProductVersion = 2;
        public string Supported = "0";
		public DateTime ExpiryDate = System.DateTime.MinValue;
		public string LicenseId = "";
		public string LicenseKey = "";
		public string ServerName = "";
		public string LicenseType = "Eval";
		public string LicenseComponentName = "";
		public string OEMCompany = "";
		public string OEMProduct = "";
#if (!WINDOWS)
        public System.Web.HttpRequest Request = null;
#endif
        public XmlDocument KeyDoc = new XmlDocument();

		private Byte[] KEY_64 = {1, 2, 3, 4, 5, 6, 7, 8};
		private Byte[] IV_64 = {8, 7, 6, 5, 4, 3, 2, 1};

		public bool NoLicenseKey = true;
		public bool NoLicenseId = true;
		public bool ValidKey = false;
		public bool Expired = false;
		public bool FIPS = false;
		public bool ServerRegistered = false;
		public bool InvalidProduct = true;
		public bool InvalidConnectionType = false;
		public bool Throttled = true;
		public int Users = 0;

		public string Message = "";
		public string TextMessage = "";
		public string Information = "";
		string WebsiteLink = "";
		string PurchaseLink = "";
		string RegisterLink = "";
        public string EvalKeyLink = ""; 



		////////////////////////////////////////////////////////////////////////////
		public DbNetLicense( string _ComponentName )
		////////////////////////////////////////////////////////////////////////////
		{
			ComponentName = _ComponentName;
            ComponentType = (Components)Enum.Parse(typeof(Components), _ComponentName, true);

			WebsiteLink = "Click <A target=_blank HREF=\"http://www.dbnetlink.net/" + ComponentName.ToLower();
            PurchaseLink = WebsiteLink + "/purchase\">here</A> to purchase a license key.";
			RegisterLink = WebsiteLink + "/license?productname={product}&licenseid={LicenseId}&computername=" + ComputerName +  "\">here</A> to generate the key.";
            EvalKeyLink = "Click <a target=_blank href=\"http://www.dbnetlink.net/dbnetsuite/nuget#licensekey\">here</a> to get a current evaluation key.";
		}
		////////////////////////////////////////////////////////////////////////////
		public void MakeLicenseKey()
		////////////////////////////////////////////////////////////////////////////
		{
			KeyDoc.AppendChild( KeyDoc.CreateElement("root") );

			switch (LicenseType)
			{
				case "Personal":
				case "Server":
				case "Eval":
				case "OEM":
				case "NFR":
				case "Enterprise":
				case "User":
				case "Developer":
					break;
				default:
					LicenseType = "Eval";
					break;
			}

			AddKeyNode( "LicenseId", LicenseId );
			AddKeyNode( "LicenseType", LicenseType );
			AddKeyNode( "ComponentName", ComponentName );
			AddKeyNode( "Version", Version );
			AddKeyNode( "Supported", Supported );
			AddKeyNode( "Users", Users.ToString() );

			switch ( LicenseType )
			{
					case "OEM":
					case "Enterprise":
						AddKeyNode( "OEMCompany", OEMCompany );
						AddKeyNode( "OEMProduct", OEMProduct );
						ServerName = "";
						break;
			}

			if ( LicenseType != "Eval" )
				ExpiryDate = System.DateTime.MaxValue;
			else
				ServerName = "";
	
			AddKeyNode( "ExpiryDate", ExpiryDate.Ticks.ToString() );
			AddKeyNode( "ServerName", ServerName );

			LicenseKey = Encrypt2( KeyDoc.OuterXml );
		}

		////////////////////////////////////////////////////////////////////////////
        public void ApplyLicenseKey()
        ////////////////////////////////////////////////////////////////////////////
        {
            ApplyLicenseKey(Assembly.GetExecutingAssembly().GetName().Version.Major);
        }

		////////////////////////////////////////////////////////////////////////////
		public void ApplyLicenseKey(int _ProductVersion)
		////////////////////////////////////////////////////////////////////////////
		{
            this.ProductVersion = _ProductVersion;

			if ( LicenseKey == "" )
				LicenseKey = ConfigValue( "LicenseKey" );

			if ( LicenseId == "" )
				LicenseId = ConfigValue( "LicenseId" );

			if ( LicenseKey == "" )
					if ( ApplicationLicenseEnabled() )
						return;

			if ( LicenseKey != "" )
			{
				NoLicenseKey = false;
	
				ValidKey = false;

                string DecryptedKey = "";

				try
				{
                    DecryptedKey = Decrypt2(LicenseKey);
                    KeyDoc.LoadXml(DecryptedKey);

					LicenseId = GetKeyNodeValue( "LicenseId" );
					LicenseType = GetKeyNodeValue( "LicenseType" );
					LicenseComponentName = GetKeyNodeValue( "ComponentName" );
					Version = GetKeyNodeValue( "Version" );
					Supported =  GetKeyNodeValue( "Supported" );
					ExpiryDate = new DateTime( Convert.ToInt64( GetKeyNodeValue( "ExpiryDate" ) ) );

					ServerName = GetKeyNodeValue( "ServerName" );
					OEMCompany = GetKeyNodeValue( "OEMCompany" );
					OEMProduct = GetKeyNodeValue( "OEMProduct" );

					if  ( GetKeyNodeValue( "Users" ) != "" )
						Users = Convert.ToInt32( GetKeyNodeValue( "Users" ) );

				}
				catch (Exception E)
				{
                    FormatMessage(E.Message + "[" + DecryptedKey + "]");
					return;
				}
			}
			else
			{
				if ( LicenseId == "" )
					NoLicenseId = false;

				FormatMessage("No License Key");
				return;
			}

			ValidKey = true;

#if (WEB_UI)
			if ( LicenseType == "Personal" )
			{
				String RemoteIP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
				String LocalIP = HttpContext.Current.Request.ServerVariables["LOCAL_ADDR"];

				InvalidConnectionType = ( RemoteIP != LocalIP );
			}
#endif

			InvalidProduct = ( ( LicenseComponentName != "DbNetSuite" ) && ( ComponentName != LicenseComponentName ) );
			ServerRegistered = false;
			Expired = true;

			switch ( LicenseType )
			{
				case "OEM":
				case "NFR":
				case "Enterprise":
					ServerRegistered = true;
					Expired = false;
					break;
				case "Server":
				case "Personal":
                case "Developer":
                    ServerRegistered = IsServerRegistered();
					Expired = false;
					break;
				case "Eval":
				case "User":
					Expired = ( ExpiryDate < System.DateTime.Today );
					ServerRegistered = true;
					break;
			}

			FormatMessage();
			FormatInformation();
		}

        ////////////////////////////////////////////////////////////////////////////
        public bool IsServerRegistered()
        ////////////////////////////////////////////////////////////////////////////
        {
            bool Registered = true;

            switch(ComponentType)
            {
                case Components.DbNetSuiteVS:
                    if ( IsLocalHost() )
                        Registered = (ComputerName.ToLower() == ServerName.ToLower());
                    break;
                case Components.DbNetSuite:
                    if ( ! IsLocalHost() )
                        Registered = (ComputerName.ToLower() == ServerName.ToLower());
                    break;
                default:
                    Registered =(ComputerName.ToLower() == ServerName.ToLower());
                    break;
            }

            return Registered;
        }

        ///////////////////////////////////////////////
        protected bool IsLocalHost()
        ///////////////////////////////////////////////
        {
#if (DBNETSUITEVS)
            this.Request = HttpContext.Current.Request;
#endif

#if (!WINDOWS)
            if (this.Request == null)
                return false;

            System.Collections.Specialized.NameValueCollection SV = this.Request.ServerVariables;
            String RemoteIP = this.Request.ServerVariables["REMOTE_ADDR"];
            String LocalIP = this.Request.ServerVariables["LOCAL_ADDR"];

            return (SV["REMOTE_ADDR"] == SV["LOCAL_ADDR"]);
#else
            return true;
#endif
        }

		////////////////////////////////////////////////////////////////////////////
		public bool ApplicationLicenseEnabled()
		////////////////////////////////////////////////////////////////////////////
		{

            if (ComponentType == Components.DbNetSuiteVS)
                return false;

#if (WEB_UI)
			if ( HttpContext.Current.Session == null )
				return false;	
	
			if ( HttpContext.Current.Session["DbNetTimeLicenseEnabled"] == null && HttpContext.Current.Session["DbNetOfficeLicenseEnabled"] == null )
				return false;

			ValidKey = true;
			Throttled = false;
			InvalidProduct = false;
			ServerRegistered = true;
			Expired = false;
#endif

			return true;
		}


		////////////////////////////////////////////////////////////////////////////
		private void FormatMessage()
		////////////////////////////////////////////////////////////////////////////
		{
			FormatMessage("");
		}

		////////////////////////////////////////////////////////////////////////////
		private void FormatMessage( string Msg )
		////////////////////////////////////////////////////////////////////////////
		{
			Throttled = ( NoLicenseKey || ! ValidKey || Expired || InvalidConnectionType );

			string BackgroundColor = "";

			if ( LicenseType == "NFR" )
			{
				Message = "<a href=\"http://www." + ComponentName.ToLower() + ".com\" style=\"font-weight:bold\">www." + ComponentName.ToLower() + ".com</a>";
				BackgroundColor = "#FFB007";
			}
			else if ( NoLicenseKey )
			{
                Message = "Please apply your {component} license key to the <b>{component}LicenseKey</b> element in the ";
                switch (ComponentType)
                {
                    case Components.DbNetSuiteVS:
                    case Components.DbNetCopy:
                        Message += "application";
                        break;
                    default:
                        Message += "{component}";
                        break;
                }

                Message += " <b>web.config</b> file.";
				BackgroundColor = "#bbff44";
			}
			else if ( ! ValidKey )
			{
				Message = "{component} license key is not valid";
				if ( Msg != "" )
					Message += "(" + Msg + ")";
				BackgroundColor = "#bbff44";
			}
			else if ( Expired )
			{
				Message = "{component} evaluation expired on " + ExpiryDate.ToString("d") + ". {purchase}";
                if (ComponentName == "DbNetSuite")
                    Message += "<br/><br/>" + this.EvalKeyLink;
				BackgroundColor = "#bbff44";
			}
			else if ( InvalidProduct )
			{
				Message = "This is a <b>" + LicenseComponentName + "</b> license key";
				BackgroundColor = "#bbff44";
			}
			else if ( InvalidConnectionType )
			{
				Message = "{component} Personal license cannot be used with remote connections";
				BackgroundColor = "#bbff44";
			}
            else if (ProductVersion > Int32.Parse(Version) && LicenseType != "Eval")
            {
                Message = "{component} major version number is greater than license major version number";
                BackgroundColor = "#bbff44";
            }
			else if ( ! ServerRegistered )
			{
				Message = "{component} not registered for this server[" + System.Environment.GetEnvironmentVariable("COMPUTERNAME") + "].";
				if ( ComponentName != "DbNetCombo" )
					Message += " {register}";
				BackgroundColor = "#bbff44";
			}

			if ( Message != "" )
			{
				Message = Message.Replace( "{purchase}", PurchaseLink );
				Message = Message.Replace( "{register}", RegisterLink );
				Message = Message.Replace( "{LicenseId}", LicenseId );
				Message = Message.Replace( "{component}", "<b>" + ComponentName + "</b>" );
				Message = Message.Replace( "{product}", ComponentName );
				TextMessage = Message;
                Message = "<div class=\"dbnetlicense\" style=\"text-align:center;padding:5px;border:1pt solid #ccc;background-color:" + BackgroundColor + ";color:dimgray;\">" + Message + "</div>";
			}
		}


		////////////////////////////////////////////////////////////////////////////
		protected void FormatInformation()
		////////////////////////////////////////////////////////////////////////////
		{
			if ( LicenseType != "NFR" )
				if ( Message != "" )
				{
					Information = Message;
					return;
				}

			string BackgroundColor = "#bbff44";

			switch ( LicenseType )
			{
				case "Eval":
					Information = "Evaluation license [ Expires " + ExpiryDate.ToString("d") + " ]. " + PurchaseLink;
					break;
				case "OEM":
				case "Enterprise":
					Information = OEMCompany + " " + LicenseType + " license applied";
					break;
				default:
					Information = LicenseType + " license applied";
					break;
			}

			Information = Information.Replace( "{component}", "<b>" + ComponentName + "</b>" );
            Information = "<div class=\"dbnetlicense\" style=\"text-align:center;background-color:" + BackgroundColor + ";padding:2px;color:dimgray;\">" + Information + "</div>";
		}

		////////////////////////////////////////////////////////////////////////////
		protected string Encrypt(string value)
		////////////////////////////////////////////////////////////////////////////
		{
		   DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
		   MemoryStream ms = new MemoryStream();
		   CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateEncryptor(KEY_64, IV_64), CryptoStreamMode.Write);
		   StreamWriter sw = new StreamWriter(cs);

		   sw.Write(value);
		   sw.Flush();
		   cs.FlushFinalBlock();
		   ms.Flush();

		   // convert back to a string
		   return Convert.ToBase64String(ms.GetBuffer(), 0, Convert.ToInt32(ms.Length));
		}

		////////////////////////////////////////////////////////////////////////////
		protected string Encrypt2(string value)
		////////////////////////////////////////////////////////////////////////////
		{
			if (this.FIPS)
				return Encrypt3(value);

			try
			{
				TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();
				MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
				DES.Key = MD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes( HashSeed ));
				DES.Mode = CipherMode.ECB;
				ICryptoTransform DESEncrypt = DES.CreateEncryptor();
				byte[] Buffer = ASCIIEncoding.ASCII.GetBytes(value);
				return Convert.ToBase64String(DESEncrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));
			}
			catch(Exception E)
			{
				if ( E.Message.IndexOf("FIPS") > -1 )
					return Encrypt3(value);
				else
					return 	E.Message;
			}
		}

		////////////////////////////////////////////////////////////////////////////
		protected string Decrypt(string value)
		////////////////////////////////////////////////////////////////////////////
		{
		   DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
		   Byte[] buffer = Convert.FromBase64String(value);
		   MemoryStream ms = new MemoryStream(buffer);
		   CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateDecryptor(KEY_64, IV_64), CryptoStreamMode.Read);
		   StreamReader sr = new StreamReader(cs);

		   return sr.ReadToEnd();
		}

		////////////////////////////////////////////////////////////////////////////
		protected string Decrypt2(string value)
		////////////////////////////////////////////////////////////////////////////
		{
			try
			{
				TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();
				MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
				DES.Key = MD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes( HashSeed ));
				DES.Mode = CipherMode.ECB;
				ICryptoTransform DESEncrypt = DES.CreateDecryptor();
				byte[] Buffer = Convert.FromBase64String( value );
				return ASCIIEncoding.ASCII.GetString(DESEncrypt.TransformFinalBlock( Buffer, 0, Buffer.Length));
			}
			catch(Exception E)
			{
				return Decrypt3(value, E.Message);
			}
		}

		////////////////////////////////////////////////////////////////////////////
		protected string Decrypt3(string value, string ExceptionMessage)
		////////////////////////////////////////////////////////////////////////////
		{
			try
			{
				Encryption enc = new Encryption();
				return enc.Decrypt(value);
			}
			catch(Exception E)
			{
				return 	E.Message + " [" + ExceptionMessage + "]";
			}
		}

        ////////////////////////////////////////////////////////////////////////////
        protected string Encrypt3(string value)
        ////////////////////////////////////////////////////////////////////////////
        {
            Encryption enc = new Encryption();
            return enc.Encrypt(value);
        }

		////////////////////////////////////////////////////////////////////////////
		public string KeyToXML()
		////////////////////////////////////////////////////////////////////////////
		{
			return Decrypt2(LicenseKey);
		}

		////////////////////////////////////////////////////////////////////////////
		private XmlNode AddKeyNode(string NodeName, string NodeText )
		////////////////////////////////////////////////////////////////////////////
		{
			XmlNode N = KeyDoc.DocumentElement.AppendChild( KeyDoc.CreateElement( NodeName ) );
			N.InnerText = NodeText;
			return N;
		}

		////////////////////////////////////////////////////////////////////////////
		private string GetKeyNodeValue(string Pattern )
		////////////////////////////////////////////////////////////////////////////
		{
			try
			{
				return KeyDoc.DocumentElement.SelectSingleNode(Pattern).InnerText;
			}
			catch
			{
				return "";
			}
		}

		////////////////////////////////////////////////////////////////////////////
		public void RegisterServer()
		////////////////////////////////////////////////////////////////////////////
		{
			try
			{
				if ( LicenseId == "" )
					LicenseId = ConfigValue( "LicenseId" );

				if ( ServerName == "" )
					ServerName = ComputerName;

				KeyDoc.AppendChild( KeyDoc.CreateElement("root") );

				AddKeyNode( "LicenseId", LicenseId );
				AddKeyNode( "ServerName", ServerName );
				AddKeyNode( "FIPS", FIPS.ToString().ToLower() );

				byte[] Bytes = Encoding.UTF8.GetBytes( KeyDoc.OuterXml); 

				Uri U = new Uri("http://www.dbnetlink.co.uk/shared/register_server.aspx");
				HttpWebRequest Request = (HttpWebRequest) WebRequest.Create(U);

				Request.Method = "POST";
				Request.ContentLength = Bytes.Length; 
				Request.ContentType = "text/xml"; 
				using ( Stream RequestStream = Request.GetRequestStream() ) 
				{ 
					RequestStream.Write(Bytes, 0, Bytes.Length); 
				}

				HttpWebResponse Response = (HttpWebResponse) Request.GetResponse();

//				Message = new System.IO.StreamReader(Response.GetResponseStream()).ReadToEnd();

				KeyDoc.Load( Response.GetResponseStream() );
				Message = GetKeyNodeValue("Message");
			
				if (Message == "")
					LicenseKey = GetKeyNodeValue("LicenseKey");
			}
			catch(Exception E)
			{
				Message = "Error:" + E.Message;
			}
		}

		////////////////////////////////////////////////////////////////////////////
		protected string ConfigValue( string Key )
		////////////////////////////////////////////////////////////////////////////
		{
			#if (version2)
				string V =  ConfigurationManager.AppSettings[ ComponentName + Key ];
			#else
				string V =  ConfigurationSettings.AppSettings[ ComponentName + Key ];
			#endif

			if ( V == null )
				V = AppConfigValue(Key);

			return V;
		}

		////////////////////////////////////////////////////////////////////////////
		protected string AppConfigValue( string Key )
		////////////////////////////////////////////////////////////////////////////
		{
			if ( ComponentName != "DbNetCopy" )
				return "";

			XmlDocument XmlDoc = new XmlDocument();

			if ( ! File.Exists(ComponentName + ".config") )
				return "";

			XmlDoc.Load ( ComponentName + ".config" );

			XmlNode Setting = XmlDoc.SelectSingleNode("//appSettings/add[@key='" + ComponentName + Key + "']");

			if ( Setting != null )
				return Setting.Attributes.GetNamedItem("value").InnerText;
			else
				return "";
		}
	}

	internal class Encryption
    {

        private TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
        private UTF8Encoding utf8 = new UTF8Encoding();

        private byte[] keyValue = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
        private byte[] iVValue = { 8, 7, 6, 5, 4, 3, 2, 1 };

        /// <summary>
        /// Key to use during encryption and decryption
        /// </summary>
        /// <remarks>
        /// <example>
        /// byte[] key = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
        /// </example>
        /// </remarks>
        public byte[] Key
        {
            get { return keyValue; }
            set { keyValue = value; }
        }

        /// <summary>
        /// Initialization vetor to use during encryption and decryption
        /// </summary>
        /// <remarks>
        /// <example>
        /// byte[] iv = { 8, 7, 6, 5, 4, 3, 2, 1 };
        /// </example>
        /// </remarks>
        public byte[] iV
        {
            get { return iVValue; }
            set { iVValue = value; }
        }

        /// <summary>
        /// Constructor, allows the key and initialization vetor to be provided
        /// </summary>
        public Encryption()
        {
        }

        /// <summary>
        /// Decrypt bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>Decrypted data as bytes</returns>
        public byte[] Decrypt(byte[] bytes)
        {
            return Transform(bytes, des.CreateDecryptor(this.keyValue, this.iVValue));
        }

        /// <summary>
        /// Encrypt bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>Encrypted data as bytes</returns>
        public byte[] Encrypt(byte[] bytes)
        {
            return Transform(bytes, des.CreateEncryptor(this.keyValue, this.iVValue));
        }

        /// <summary>
        /// Decrypt a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Decrypted data as string</returns>
        public string Decrypt(string text)
        {
            byte[] input = Convert.FromBase64String(text);
            byte[] output = Transform(input, des.CreateDecryptor(this.keyValue, this.iVValue));
            return utf8.GetString(output);
        }

        /// <summary>
        /// Encrypt a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Encrypted data as string</returns>
        public string Encrypt(string text)
        {
            byte[] input = utf8.GetBytes(text);
            byte[] output = Transform(input, des.CreateEncryptor(this.keyValue, this.iVValue));
            return Convert.ToBase64String(output);
        }

        /// <summary>
        /// Encrypt or Decrypt bytes.
        /// </summary>
        /// <remarks>
        /// This is used by the public methods
        /// </remarks>
        /// <param name="input">Data to be encrypted/decrypted</param>
        /// <param name="cryptoTransform">
        /// <example>des.CreateEncryptor(this.keyValue, this.iVValue)</example>
        /// </param>
        /// <returns>Byte data containing result of opperation</returns>
        private byte[] Transform(byte[] input, ICryptoTransform cryptoTransform)
        {
            // Create the necessary streams
            MemoryStream memory = new MemoryStream();
            CryptoStream stream = new CryptoStream(memory, cryptoTransform, CryptoStreamMode.Write);

            // Transform the bytes as requesed
            stream.Write(input, 0, input.Length);
            stream.FlushFinalBlock();

            // Read the memory stream and convert it back into byte array
            memory.Position = 0;
            byte[] result = new byte[memory.Length];
            memory.Read(result, 0, result.Length);

            // Clean up
            memory.Close();
            stream.Close();

            // Return result
            return result;
        }

    }



}