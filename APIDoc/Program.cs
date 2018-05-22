using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Xml;


namespace APIDoc
{
	class Program
	{
		public static string sOutPath = @"D:\temp\pub";
		public static Dictionary<string, string> dictCheck = new Dictionary<string, string>();

		static void Main(string[] args)
		{

			//read commandline arguments by position
			// filename|output_diff_folder|output_report_folder
			cmdArg aX = new cmdArg(args[0]);


			//Console.WriteLine("count of | delimited argumnets: " + aX.len);

			if (aX.len < 1)
			{
				Console.WriteLine("Invalid commandline options, please specify policy file to analyse. ");
				return;
			}
			string sOutputFolder = aX.strVal(1);
			string sFile = sOutputFolder + @"\" + aX.strVal(0);


			//TODO check if file exists 

			//TODO check if folder exists and create if it does not exist 

			//TODO append date_time_sub folder
			string sOutFolder = aX.strVal(1) + @"\" + aX.strVal(0).Replace(".xml", "");// + @"\" + DateTime.Now.ToString("yyyy_MM_dd");
			sOutFolder = sOutFolder.Replace(@"\\", @"\");



			string sOutServiceFolder = sOutFolder + "\\SERVICE";
			string sOutPolicyFolder = sOutFolder + "\\POLICY";
			string sOutEnAssertFolder = sOutFolder + "\\ENCAPSULATED_ASSERTION";
			FileFuncs.CreateFolder(sOutServiceFolder);
			FileFuncs.CreateFolder(sOutPolicyFolder);
			FileFuncs.CreateFolder(sOutEnAssertFolder);

			//String strDate = DateTime.Now.ToString("yyyy_MM_dd");

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(sFile);

			var manager = new XmlNamespaceManager(new NameTable());
			manager.AddNamespace("l7", "http://ns.l7tech.com/2010/04/gateway-management");
			//XNamespace ns = XNamespace.Get("XXXX");

			//read all nodes from the bundle xml
			//XmlNodeList xnList = xmlDoc.SelectNodes("l7:List/l7:Item", manager);
			XmlNodeList xnList = xmlDoc.SelectNodes("l7:Item/l7:Resource/l7:Bundle/l7:References/l7:Item", manager);


			foreach (XmlNode xn in xnList)
			{
				string sType = xn["l7:Type"].InnerText;
				string sName = xn["l7:Name"].InnerText;
				string sID = xn["l7:Id"].InnerText;

				if (!dictCheck.ContainsKey(sType))
				{
					string sCFold = sOutFolder + @"\" + sType;
					FileFuncs.CreateFolder(sCFold);
					dictCheck.Add(sType, sCFold);
					FileFuncs.CreateFolder(sOutEnAssertFolder);
				}


				Item oI = new Item();
				oI.Id = xn["l7:Id"].InnerText;
				oI.Name = xn["l7:Name"].InnerText;
				oI.Type = xn["l7:Type"].InnerText;
				oI.ItemXML = xn.InnerXml;
				Tracker.addItem(oI.Id, oI);

			}

			foreach (KeyValuePair<string, Item> oI in Tracker.oItems)
			{

			}



			StringBuilder sX = new StringBuilder(3900);
			StringBuilder sCluster = new StringBuilder(300);
			sCluster.AppendLine("----------------------------------");
			sCluster.AppendLine("   Cluster Properties");
			sCluster.AppendLine("----------------------------------");

			sX.AppendLine("id|name|type|url|enabled");
			foreach (XmlNode xn in xnList)
			{

				string sType = xn["l7:Type"].InnerText;
				string sName = xn["l7:Name"].InnerText;
				string sID = xn["l7:Id"].InnerText;

				string sUrlPattern = string.Empty;
				string sSerEnabled = string.Empty;

				sX.Append(sID); sX.Append("|");sX.Append(sName); sX.Append("|");sX.Append(sType); sX.Append("|");

				switch (sType)
				{
					case "ASSERTION_ACCESS":
					case "AUDIT_CONFIG":
						break;
					case "CLUSTER_PROPERTY":
						sCluster.Append(xn.AA_NodeText("l7:Resource/l7:ClusterProperty/l7:Name", manager));
						sCluster.Append("=");
						sCluster.AppendLine(xn.AA_NodeText("l7:Resource/l7:ClusterProperty/l7:Value", manager));
						break;
					case "ENCAPSULATED_ASSERTION":
					case "FIREWALL_RULE":
					case "FOLDER":
					case "ID_PROVIDER_CONFIG":
					case "INTERFACE_TAG":
					case "JDBC_CONNECTION":
					case "LOG_SINK":
						break;
					case "POLICY":
						PolReader iP = new PolReader(xn, manager);
						iP.Analys(sOutPolicyFolder);
						break;
					case "RBAC_ROLE":
					case "RESOURCE_ENTRY":
					case "SCHEDULED_TASK":
					case "SECURE_PASSWORD":
					case "SECURITY_ZONE":
					case "SERVER_MODULE_FILE":
						break;
					case "SERVICE":
						//sUrlPattern = xn.AA_NodeText("l7:Resource/l7:Service/l7:ServiceDetail/l7:ServiceMappings/l7:HttpMapping/l7:UrlPattern", manager);
						//sSerEnabled = xn.AA_NodeText("l7:Resource/l7:Service/l7:ServiceDetail/l7:Enabled", manager);
						//sUrlPattern = xn["l7:Resource/l7:Service/l7:ServiceDetail/l7:ServiceMappings/l7:HttpMapping/l7:UrlPattern"].InnerText;
						ItemReader iSrv = new ItemReader(xn, manager);
						iSrv.AddService(sOutServiceFolder);
						SrvProp oS = null;
						if (Tracker.GetServiceObject(sID, out oS))
						{
							sUrlPattern = oS.Url;
							sSerEnabled = oS.Enabled;
						}
						break;
					case "SSG_CONNECTOR":
					case "TRUSTED_CERT":
						break;
					case "USER":
						break;
					default:
						//errpr log for future
						break;
				}


				sX.Append(sUrlPattern); sX.Append("|");
				sX.Append(sSerEnabled); sX.AppendLine("|");
			}
			FileFuncs.WriteTxtFile(sOutFolder + "\\stats.txt", sX.ToString());
			FileFuncs.WriteTxtFile(sOutFolder + "\\CLUSTER_PROPERTY\\cluster_properties.txt", sCluster.ToString());
			//XmlNode n = xmlDoc.SelectSingleNode("l7:List/l7:Item", manager);
			//Console.WriteLine(n.InnerText);
			//Console.WriteLine("done");


			//XmlNode rootNode = xmlDoc.DocumentElement;
			////DisplayNodes(rootNode);


			//XmlNodeList xnList = xmlDoc.SelectNodes("//l7:List/l7:Item");
			//foreach (XmlNode xn in xnList)
			//{
			//	string firstName = xn["FirstName"].InnerText;
			//	string lastName = xn["LastName"].InnerText;
			//	Console.WriteLine("Name: {0} {1}", firstName, lastName);
			//}




			//foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes[)
			//{
			//	Console.WriteLine(xmlNode.Attributes["currency"].Value + ": " + xmlNode.Attributes["rate"].Value);
			//}




			//// Create an isntance of XmlTextReader and call Read method to read the file
			//XmlTextReader textReader = new XmlTextReader("D:\\temp\\services.xml");
			//textReader.Read();



			//while (textReader.Read())
			//{

			//	switch (textReader.NodeType)
			//	{ case XmlNodeType.Element:
			//			//listBox1.Items.Add("<" + xmlReader.Name + ">");
			//			break;
			//		case XmlNodeType.Text:
			//			//listBox1.Items.Add(xmlReader.Value);
			//			break;
			//		case XmlNodeType.EndElement:
			//			//listBox1.Items.Add("");
			//			break;
			//	}

			//	if (textReader.IsStartElement())
			//	{
			//		Console.WriteLine(textReader.Name.ToString());
			//		//return only when you have START tag
			//		switch (textReader.Name.ToString())
			//		{
			//			case "Name":
			//				Console.WriteLine("Name of the Element is : " + textReader.ReadString());
			//				break;
			//			case "Location":
			//				Console.WriteLine("Your Location is : " + textReader.ReadString());
			//				break;
			//		}
			//	}
			//	Console.WriteLine("");
			//}


			//// If the node has value
			//while (textReader.Read())
			//{

			//	// Move to fist element
			//	textReader.MoveToElement();
			//	Console.WriteLine("XmlTextReader Properties Test");
			//	Console.WriteLine("===================");
			//	// Read this element's properties and display them on console
			//	Console.WriteLine("Name:" + textReader.Name);
			//	Console.WriteLine("Base URI:" + textReader.BaseURI);
			//	Console.WriteLine("Local Name:" + textReader.LocalName);
			//	Console.WriteLine("Attribute Count:" + textReader.AttributeCount.ToString());
			//	Console.WriteLine("Depth:" + textReader.Depth.ToString());
			//	Console.WriteLine("Line Number:" + textReader.LineNumber.ToString());
			//	Console.WriteLine("Node Type:" + textReader.NodeType.ToString());
			//	Console.WriteLine("Attribute Count:" + textReader.Value.ToString());
			//}
		}

		private static void DisplayNodes(XmlNode node)
		{
			//Print the node type, node name and node value of the node
			if (node.NodeType == XmlNodeType.Text)
			{
				Console.WriteLine("Type = [" + node.NodeType + "] Value = " + node.Value);
			}
			else
			{
				Console.WriteLine("Type = [" + node.NodeType + "] Name = " + node.Name);
			}

			//Print attributes of the node
			if (node.Attributes != null)
			{
				XmlAttributeCollection attrs = node.Attributes;
				foreach (XmlAttribute attr in attrs)
				{
					Console.WriteLine("Attribute Name = " + attr.Name + "; Attribute Value = " + attr.Value);
				}
			}

			//Print individual children of the node, gets only direct children of the node
			XmlNodeList children = node.ChildNodes;
			foreach (XmlNode child in children)
			{
				DisplayNodes(child);
			}
		}


	}

	public static class Tracker
	{
		public static Dictionary<string, string> dpath = new Dictionary<string, string>();
		public static Dictionary<string, object> oStore = new Dictionary<string, object>();
		public static Dictionary<string, Item> oItems = new Dictionary<string, Item>();

		public static void addName(string key,string sPath)
		{
			if (!dpath.ContainsKey(key))
			{
				dpath.Add(key, sPath);
			}
		}
		public static void addObject(string key, object sValue)
		{
			if (!oStore.ContainsKey(key))
			{
				oStore.Add(key, sValue);
			}
		}
		public static void addItem(string key, Item sValue)
		{
			if (!oItems.ContainsKey(key))
			{
				oItems.Add(key, sValue);
			}
		}

		public static bool GetServiceObject(string key,out SrvProp oS)
		{
			object o = null;
			oS = null;
			if (oStore.ContainsKey(key))
			{
				oStore.TryGetValue(key, out o);
				oS = o as SrvProp;
				if (oS == null) return false;
				return true;
			}

			return false;
		}
	}

	public class SrvProp
	{
		public string Id;
		public string Name;
		public string Type;
		public string Enabled;
		public string FolderId;
		public string Version;
		public string Verbs;
		public string Url;

		public string Sinternal;
		public string PolicyRevision;
		public string Soap;
		public string TracingEnabled;
		public string WssProcessingEnabled;
		public string PolicyVer;
		public string PolicyXml;
		public string PolicyText;




		public string HeaderText()
		{
			StringBuilder sX = new StringBuilder(800);
			sX.AppendLine("=====================================================================================================");
			sX.AppendLine("=================== Service Header ==================================================================");
			sX.AppendLine("=====================================================================================================");
			sX.Append("Service Name: "); sX.AppendLine(this.Name);
			sX.Append("Id: "); sX.AppendLine(this.Id);
			sX.Append("Folder Id: "); sX.AppendLine(this.FolderId);
			sX.Append("Version: "); sX.AppendLine(this.Version);
			sX.Append("Type: "); sX.AppendLine(this.Type);
			sX.Append("Is Enabled: "); sX.AppendLine(this.Enabled);
			sX.Append("URL: "); sX.AppendLine(this.Url);
			sX.Append("HTTP Verbs: "); sX.AppendLine(this.Verbs);

			sX.Append("Policy Revision: "); sX.AppendLine(this.PolicyRevision);
			sX.Append("Internal: "); sX.AppendLine(this.Sinternal);
			sX.Append("Soap: "); sX.AppendLine(this.Soap);
			sX.Append("Tracing Enabled: "); sX.AppendLine(this.TracingEnabled);
			sX.Append("WSS Processing Enabled: "); sX.AppendLine(this.WssProcessingEnabled);
			sX.AppendLine("=====================================================================================================");

			return sX.ToString();
		}

		/// <summary>
		/// return filename based on the name attribute of the service
		/// </summary>
		/// <returns></returns>
		public string FileName()
		{
			string fName = FileFuncs.SanitizeFileName(this.Name, '_', false) + ".txt";

			return fName;
		}


	}

	public class Item
	{
		public string Id;
		public string Name;
		public string Type;
		public string ItemXML;

		/// <summary>
		/// return filename based on the name attribute of the service
		/// </summary>
		/// <returns></returns>
		public string FileName()
		{
			string fName = FileFuncs.SanitizeFileName(this.Name, '_', false) + ".txt";

			return fName;
		}


	}

}
