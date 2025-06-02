using System;
using System.Web;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;

////////////////////////////////////////////////////////////////////////////
namespace DbNetLink
////////////////////////////////////////////////////////////////////////////
{
	////////////////////////////////////////////////////////////////////////////
	public class Util
	////////////////////////////////////////////////////////////////////////////
	{
		public static bool EncryptionEnabled = false;
        private static bool? _FIPSEnabled = null;
        public static Regex IsEncrypted = new Regex(@"___([a-z0-9A-Z+\/=_]*)___", RegexOptions.Compiled);
		private static string HashKey = HttpContext.Current.Request.ServerVariables["LOCAL_ADDR"];

        ////////////////////////////////////////////////////////////////////////////
        public static bool FIPSEnabled()
        ////////////////////////////////////////////////////////////////////////////
        {
            if (!_FIPSEnabled.HasValue)
            {
                _FIPSEnabled = false;
                try
                {
                    EncDec.Encrypt("test", HashKey);
                }
                catch (Exception)
                {
                    _FIPSEnabled = true;
                }
           
            }
            return _FIPSEnabled.Value;
        }

        ////////////////////////////////////////////////////////////////////////////
        public static string HashString(string inputString, string hashName)
        ////////////////////////////////////////////////////////////////////////////
        {

            HashAlgorithm algorithm = HashAlgorithm.Create(hashName);
            byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

		////////////////////////////////////////////////////////////////////////////
		public static string Encrypt(string Str)
		////////////////////////////////////////////////////////////////////////////
		{
			if ( IsEncrypted.IsMatch( Str ) || ! EncryptionEnabled )
				return Str;

            if ( FIPSEnabled() )
                return XmlConvert.EncodeName( "___" + new DbNetLink.Web.UI.Encryption().Encrypt(Str) + "___" );
             else
    			return XmlConvert.EncodeName( "___" + EncDec.Encrypt( Str, HashKey ) + "___" );

			/*
			TripleDESCryptoServiceProvider DES_SP = new TripleDESCryptoServiceProvider();
			MD5CryptoServiceProvider MD5_SP = new MD5CryptoServiceProvider();
			DES_SP.Key = MD5_SP.ComputeHash(System.Text.Encoding.Unicode.GetBytes(HashKey));
			DES_SP.Mode = CipherMode.ECB;
			ICryptoTransform DESEncryptor = DES_SP.CreateEncryptor();
			byte[] Buf = System.Text.Encoding.Unicode.GetBytes(Str);
			return XmlConvert.EncodeName( "___" + Convert.ToBase64String(DESEncryptor.TransformFinalBlock(Buf, 0, Buf.Length)) + "___" );
			*/
		}

		////////////////////////////////////////////////////////////////////////////
		public static string Encrypt(string[] Str)
		////////////////////////////////////////////////////////////////////////////
		{
			ArrayList Tokens = new ArrayList();

			foreach ( string S in Str )
				Tokens.Add( Encrypt(S) );

			return "[\"" + string.Join("\",\"", (string[]) Tokens.ToArray(typeof(string)) ) + "\"]";
		}

		////////////////////////////////////////////////////////////////////////////
		public static string Decrypt(string strBase64Text)
		////////////////////////////////////////////////////////////////////////////
		{
			string S = strBase64Text;
			if ( ! IsEncrypted.IsMatch( strBase64Text ) )
				return strBase64Text;

			strBase64Text = XmlConvert.DecodeName( IsEncrypted.Match( strBase64Text ).Groups[1].Value );

            if (FIPSEnabled())
                return new DbNetLink.Web.UI.Encryption().Decrypt(strBase64Text);
            else
			    return EncDec.Decrypt( strBase64Text, HashKey );

			/*
			TripleDESCryptoServiceProvider DES_SP = new TripleDESCryptoServiceProvider();
			MD5CryptoServiceProvider MD5_SP = new MD5CryptoServiceProvider();
			DES_SP.Key = MD5_SP.ComputeHash(System.Text.Encoding.Unicode.GetBytes(HashKey));
			DES_SP.Mode = CipherMode.ECB;
			ICryptoTransform DESEncryptor = DES_SP.CreateDecryptor();
			byte[] Buf = Convert.FromBase64String(strBase64Text);
			return System.Text.Encoding.Unicode.GetString(DESEncryptor.TransformFinalBlock( Buf, 0, Buf.Length));
			*/

		}

		////////////////////////////////////////////////////////////////////////////
		public static string DecryptTokens( string S )
		////////////////////////////////////////////////////////////////////////////
		{
			Match Token = null;

			try
			{
				foreach ( Match T in IsEncrypted.Matches( S ) )
				{
					Token = T;
					S = S.Replace( T.Value, DbNetLink.Util.Decrypt( T.Value ) );
				}
			}
			catch( Exception)
			{
				HttpContext.Current.Response.Write( "DecryptTokens: " + Token.Value + ":[" + S + "]");
				HttpContext.Current.Response.End();
			}
			return S;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static NameValueCollection ConfigurationSection( string SectionKey )
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		{
			NameValueCollection _Section = null;

			#if (version2)
				_Section = (NameValueCollection) ConfigurationManager.GetSection( SectionKey );
			#else
				_Section = (NameValueCollection) ConfigurationSettings.GetConfig( SectionKey );
			#endif
			
			if ( _Section == null ) 
				_Section = new NameValueCollection();

			return _Section;
		}

        ////////////////////////////////////////////////////////////////////////////
        public static string FindComponentInstallation(string ComponentName)
        ////////////////////////////////////////////////////////////////////////////
        {
            // Returns the virtual directory for the component specified by 'ComponentName'
            // Returns "" if the component installation cannot be found.
            
            // Check web.config files for installation values
            string ComponentDir = ConfigurationSettings.AppSettings[ComponentName + "VirtualDir"];
            if (ComponentDir != null)
                if (File.Exists(HttpContext.Current.Request.MapPath(ComponentDir) + "\\" + ComponentName + ".js"))
                    return ComponentDir;

            // Check default installation directory for DbNetXXXX.js
            if (File.Exists(HttpContext.Current.Request.MapPath("/" + ComponentName) + "\\" + ComponentName + ".js"))
                return "/" + ComponentName;

            // Check for web.config in default DbNetSuite installation directory
            if (File.Exists(HttpContext.Current.Request.MapPath("/dbnetsuite") + "\\web.config"))
            {
                XmlDocument XmlDoc = new XmlDocument();
                XmlDoc.Load(HttpContext.Current.Request.MapPath("/dbnetsuite") + "\\web.config");
                XmlNodeList Nodes = XmlDoc.DocumentElement.SelectSingleNode("//appSettings").ChildNodes;
                foreach (XmlNode N in Nodes)
                    if (N.Attributes.GetNamedItem("key").InnerText.ToLower().Equals(ComponentName.ToLower() + "virtualdir"))
                        if (File.Exists(HttpContext.Current.Request.MapPath(N.Attributes.GetNamedItem("value").InnerText) + "\\" + ComponentName + ".js"))
                            return N.Attributes.GetNamedItem("value").InnerText;
            }

            return "";
        }
	}


	////////////////////////////////////////////////////////////////////////////
	public class EncDec 
	////////////////////////////////////////////////////////////////////////////
	{ 
		////////////////////////////////////////////////////////////////////////////
		public static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV) 
		////////////////////////////////////////////////////////////////////////////
		{ 
			MemoryStream ms = new MemoryStream(); 
			Rijndael alg = Rijndael.Create(); 
			alg.Key = Key; 

			alg.IV = IV; 

			CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write); 
			cs.Write(clearData, 0, clearData.Length); 

			cs.Close(); 

			byte[] encryptedData = ms.ToArray(); 
			return encryptedData; 
		} 

		////////////////////////////////////////////////////////////////////////////
		public static string Encrypt(string clearText, string Password) 
		////////////////////////////////////////////////////////////////////////////
		{ 
			byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText); 
			PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, 

			new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,  0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76}); 

			byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16)); 

			return Convert.ToBase64String(encryptedData); 
		} 
	    
		////////////////////////////////////////////////////////////////////////////
		public static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV) 
		////////////////////////////////////////////////////////////////////////////
		{ 
			MemoryStream ms = new MemoryStream(); 

			Rijndael alg = Rijndael.Create(); 

			alg.Key = Key; 

			alg.IV = IV; 

			CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write); 
			cs.Write(cipherData, 0, cipherData.Length); 
			cs.Close(); 

			byte[] decryptedData = ms.ToArray(); 
			return decryptedData; 
		} 

		////////////////////////////////////////////////////////////////////////////
		public static string Decrypt(string cipherText, string Password) 
		////////////////////////////////////////////////////////////////////////////
		{ 

			byte[] cipherBytes = Convert.FromBase64String(cipherText); 

			PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, 

			new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,  0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76}); 

			byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16)); 

			return System.Text.Encoding.Unicode.GetString(decryptedData); 
		} 
	}
}