using System;
using System.IO;
using System.Text;
using System.Xml;

namespace APIDoc
{
	public class cmdArg
	{
		private string[] argStr;
		private int[] argInt;
		private uint[] argUint;
		public int len;

		public cmdArg(string value)
		{
			//init lists 
			this.argStr = value.Split('|');
			this.len = this.argStr.Length;

			this.argInt = new int[this.argStr.Length];
			this.argUint = new uint[this.argStr.Length];

			int ival;
			uint uval;
			for (int x = 0; x < this.argStr.Length; x++)
			{
				bool bFlag;

				bFlag = int.TryParse(this.argStr[x], out ival);
				if (bFlag) this.argInt[x] = ival;

				bFlag = uint.TryParse(this.argStr[x], out uval);
				if (bFlag) this.argUint[x] = uval;
			}
		}

		public int intVal(int index)
		{
			if (index < this.argInt.Length)
			{
				return this.argInt[index];
			}
			else
			{
				return 0;
			}
		}

		public uint uintVal(int index)
		{
			if (index < this.argInt.Length)
			{
				return this.argUint[index];
			}
			else
			{
				return 0;
			}
		}

		public string strVal(int index)
		{
			if (index < this.argStr.Length)
			{
				return this.argStr[index];
			}
			else
			{
				return "";
			}
		}

		public char chVal(int index)
		{
			if (index < this.argStr.Length)
			{
				char[] cH = this.argStr[index].ToCharArray();
				if (cH.Length >= 0)
					return cH[0];
				else
					return ' ';
			}
			else
			{
				return ' ';
			}
		}
	}

	public static class StrUtil
	{
		/// <summary>
		/// return multiline string with prefixed spaces
		/// </summary>
		/// <param name="s"></param>
		/// <param name="spaces"></param>
		/// <returns></returns>
		public static string PrefixSpaces(string s, int spaces)
		{
			StringBuilder sbX = new StringBuilder(800);
			// split based on endofline chars.
			string[] partsFromString = s.Split(	new string[] { "\r\n" }, StringSplitOptions.None);

			//if input is multiline then prefix spaces for each line
			if(partsFromString.Length > 1)
			{
				for (int i = 0; i < partsFromString.Length; i++)
				{
					sbX.Append(new string(' ', spaces));
					sbX.AppendLine(partsFromString[i]);
				}
			}
			else
			{
				sbX.Append(new string(' ', spaces));
				sbX.AppendLine(s);
			}
			return sbX.ToString();
		}
	}


	public static class FileFuncs
	{
		public static bool WriteTxtFile(string filename, string oX, bool bAppend = false)
		{
			bool ret = false;
			try
			{
				if (File.Exists(filename))
					File.Delete(filename);

				//string sAfterValidate = JsonConvert.SerializeObject(oX, Formatting.Indented);
				StreamWriter swx = new StreamWriter(filename, bAppend);

				swx.WriteLine(oX);
				swx.Close();
				swx.Dispose();

				ret = true;
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("ERROR: Unable to print to file: " + ex.Message);
			}

			return ret;
		}

		/// <summary>
		/// returns filename that is based on whitelisted file characters only
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static string CleanFilename(string filename)
		{
			string sF = filename;
			sF = sF.Replace(" ", "_");
			sF = sF.Replace("\\", "_");
			sF = sF.Replace("/", "_");
			sF = sF.Replace("\"", "_");
			sF = sF.Replace("*", "_");
			//sF = sF.Replace("__", "_");

			return sF + ".txt";
		}

		public static void CreateFolder(string folderpath)
		{
			try
			{
				if (!Directory.Exists(folderpath))
				{
					Directory.CreateDirectory(folderpath);
				}

			}
			catch (IOException ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
		public static string SanitizeFileName(string s, char subchar, bool skip)
		{
			string admitted = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_#.";
			StringBuilder output = new StringBuilder(s.Length);
			bool found = false;

			foreach (char c in s)
			{
				found = false;
				foreach (char adm in admitted)
				{
					if (c == adm)
					{
						found = true;
						output.Append(c);
					}
				}

				if (found == false)
				{
					if (!skip)
						output.Append(subchar);
				}
			}

			return output.ToString();
		}


	}

	public static class DictExtensions
	{
		/// <summary>
		/// This class is used to implement generic methods that are applicable to all sub classes
		/// </summary>
		public static string AA_AttributeValueXPAth(this XmlNode anyobj, string xpathval, XmlNamespaceManager _man)
		{

			string attrVal = string.Empty;
			try
			{
				attrVal = anyobj.SelectSingleNode(xpathval, _man).Value;
			}
			catch (Exception ex)
			{

			}

			return attrVal;

		}
		public static string AA_AttributeValue(this XmlNode anyobj, string xpathval)
		{

			if (anyobj == null) return string.Empty;

			string attrVal = string.Empty;
			try
			{
				
				attrVal = anyobj.Attributes[xpathval].Value;
			}
			catch 
			{

			}

			return attrVal;

		}
		public static string AA_NodeText(this XmlNode anyobj, string xpathval, XmlNamespaceManager _man)
		{

			string attrVal = string.Empty;
			try
			{
				attrVal = anyobj.SelectSingleNode(xpathval, _man).InnerText;
			}
			catch (Exception ex)
			{

			}

			return attrVal;

		}
		public static string AA_SubNodesTextDelimited(this XmlNode anyobj, string childNodesName, XmlNamespaceManager _man, string sDel = " | " )
		{

			StringBuilder sV = new StringBuilder(200);
			if (anyobj.HasChildNodes)
			{
				//select all services
				XmlNodeList xnList = anyobj.SelectNodes(childNodesName, _man);

				//go through list of all resource 
				foreach (XmlNode sNodeService in xnList)
				{
					sV.Append(sNodeService.InnerText);
					sV.Append(sDel);
				}


			}


			return sV.ToString();

		}
		public static string AA_AttributeValueDecodeBase64(this XmlNode anyobj, string xpathval, XmlNamespaceManager _man)
		{

			string attrVal = string.Empty;
			try
			{
				attrVal = anyobj.SelectSingleNode(xpathval, _man).Value;
			}
			catch (Exception ex)
			{
				return attrVal;
			}
			//string base64Encoded = n.Attributes["stringValue"].Value;
			string base64Decoded;
			try
			{
				//fix string if incorrectly encoded
				if (attrVal.EndsWith("=") && !attrVal.EndsWith("=="))
				{
					attrVal += "=";
				}

				byte[] data = System.Convert.FromBase64String(attrVal);
				base64Decoded = Encoding.ASCII.GetString(data);
			}
			catch (Exception ex)
			{
				base64Decoded = "ERROR_UNABLE_TO_DECODE_BASE64";
			}


			return base64Decoded;

		}

		public static void AppendWithSpaces(this StringBuilder anyobj, string s, int spaces)
		{


			anyobj.Append(StrUtil.PrefixSpaces(s, spaces));

			//anyobj.Append(new string(' ', spaces));
			//anyobj.Append(s);

		}



	}
}