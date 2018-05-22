using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace APIDoc
{
	class ItemReader
	{
		private XmlNode _xn;
		private XmlNamespaceManager _man;
		private string _name;
		private string _id;
		private string _type;
		private StringBuilder sA = new StringBuilder(3900);

		public ItemReader(XmlNode xn, XmlNamespaceManager man)
		{
			this._xn = xn;
			_man = man;
		}

		public void AddService(string sFolder)
		{
			XmlNode nSD = _xn.SelectSingleNode("l7:Resource/l7:Service/l7:ServiceDetail", _man);

			SrvProp oS = new SrvProp();

			oS.Name = _xn.AA_NodeText("l7:Name", _man);
			oS.Id = _xn.AA_NodeText("l7:Id", _man);
			oS.Type = _xn.AA_NodeText("l7:Type", _man);

			oS.FolderId = nSD.AA_AttributeValue("folderId");
			oS.Version = nSD.AA_AttributeValue("version");
			oS.Enabled = nSD.AA_NodeText("l7:Enabled", _man);
			//oS.Url = nSrvMap.AA_NodeText("l7:ServiceMappings/l7:HttpMapping/l7:UrlPattern", _man);

			//get node for service mapping
			XmlNode nSrvMap = nSD.SelectSingleNode("l7:ServiceMappings/l7:HttpMapping", _man);
			oS.Url = nSrvMap.AA_NodeText("l7:UrlPattern", _man);
			oS.Verbs = nSrvMap.AA_SubNodesTextDelimited("l7:Verbs/l7:Verb", _man);
			//get all properties
			XmlNodeList xlProp = nSD.SelectNodes("l7:Properties/l7:Property", _man);
			foreach (XmlNode xNode in xlProp)
			{
				string sKey = xNode.AA_AttributeValue("key");
				string sVal = xNode.InnerText;
				switch (sKey)
				{
					case "internal":
						oS.Sinternal = sVal;
						break;
					case "policyRevision":
						oS.PolicyRevision = sVal;
						break;
					case "soap":
						oS.Soap = sVal;
						break;
					case "tracingEnabled":
						oS.TracingEnabled = sVal;
						break;
					case "wssProcessingEnabled":
						oS.WssProcessingEnabled = sVal;
						break;
				}
			}

			//get node for service mapping
			XmlNodeList nPolNodes = _xn.SelectNodes("l7:Resource/l7:Service/l7:Resources/l7:ResourceSet/l7:Resource", _man);
			foreach (XmlNode xNode in nPolNodes)
			{
				string sType = xNode.AA_AttributeValue("type");
				if (sType == "policy")
				{
					oS.PolicyRevision = xNode.AA_AttributeValue("version");
					oS.PolicyXml = xNode.InnerText;
				}
			}

			Tracker.addObject(oS.Id, oS);
			//if (_type == "SERVICE" )
			//   {
			sA.Append("");
			sA.AppendLine(oS.HeaderText());
			sA.AppendLine("====================================");
			sA.AppendLine("======Policy Text====================");
			sA.AppendLine("====================================");
			sA.Append("Policy version: "); sA.AppendLine(oS.PolicyRevision);
			sA.AppendLine();
			policyReader oP = new policyReader(oS.PolicyXml);

			sA.Append(oP.process());

			sA.AppendLine("====================================");
			sA.AppendLine("======Policy XML====================");
			sA.AppendLine("====================================");
			sA.AppendLine(oS.PolicyXml);

			//FileFuncs.WriteTxtFile(sFolder + @"\" + this.fileName(), sA.ToString());
			FileFuncs.WriteTxtFile(sFolder + @"\" + oS.FileName(), sA.ToString());

			//}


		}


		//private void WriteNodes(XmlNode node)
		//{
		//	string sLook = node.Name + "_" + node.ChildNodes.Count;
		//	int cCount = node.ChildNodes.Count;
		//	bool processChildNodes = true;

		//	//sA.Append("Child Count: "); sA.Append(cCount); sA.Append(" : ");
		//	switch (node.NodeType)
		//	{
		//		case XmlNodeType.Element:
		//			switch (node.Name)
		//			{
		//				//read and display policy
		//				case "l7:Resource":
		//					sA.AppendLine("-----------------------------------------------------------------");
		//					sA.Append(node.Name); sA.Append(": "); WriteAttributesGeneric(node);
		//					sA.AppendLine("-----------------------------------------------------------------");

		//					string sP = node.InnerText;
		//					sA.Append(sP);
		//					policyReader oP = new policyReader(sP);

		//					sA.Append(oP.process());
		//					break;
		//				//print node name & attributes
		//				case "l7:ResourceSet":
		//					sA.AppendLine("-----------------------------------------------------------------");
		//					sA.Append(node.Name); sA.Append(": "); WriteAttributesGeneric(node);
		//					sA.AppendLine("-----------------------------------------------------------------");
		//					break;
		//				//print node name & attributes
		//				case "l7:Service":
		//				case "l7:ServiceDetail":
		//					//sA.AppendLine("");
		//					sA.Append(node.Name); sA.Append(": "); WriteAttributesGeneric(node);
		//					//sA.AppendLine("");
		//					break;

		//				case "l7:Verbs":
		//				case "l7:Resources":
		//					break;
		//				case "l7:ServiceMappings":
		//				case "l7:HttpMapping":
		//				case "l7:Properties":


		//					sA.Append("===> "); sA.AppendLine(node.Name);
		//					break;
		//				case "l7:Name":
		//					sA.Append(node.Name); sA.Append(": "); sA.Append(node.InnerText);
		//					WriteAttributesGeneric(node);
		//					break;
		//				case "l7:Property":
		//					WriteAttributesGeneric(node, false); sA.Append(", "); sA.AppendLine(node.FirstChild.InnerText);
		//					processChildNodes = false;
		//					break;



		//				default:
		//					sA.Append(node.Name); sA.Append(": "); sA.Append(node.InnerText);
		//					WriteAttributesGeneric(node);
		//					break;
		//			}
		//			break;
		//		//skip text
		//		case XmlNodeType.Text:
		//			break;

		//		default:
		//			sA.Append(node.Name); sA.Append(": "); sA.Append(node.Value);
		//			WriteAttributesGeneric(node);
		//			break;
		//	}


		//	////Print the node type, node name and node value of the node
		//	//if (node.NodeType == XmlNodeType.Text)
		//	//{
		//	//	//sA.Append(node.Name); sA.Append(": "); sA.Append(node.Value);
		//	//}

		//	//if (node.NodeType == XmlNodeType.Element)
		//	//{
		//	//	if (node.HasChildNodes && node.ChildNodes.Count == 1)
		//	//	{
		//	//		sA.Append(node.Name); sA.Append(": "); sA.Append(node.InnerText);
		//	//	}
		//	//	else
		//	//	{
		//	//		sA.Append(node.Name); sA.Append(": "); sA.Append(node.Value);
		//	//	}

		//	//}

		//	//else
		//	//{

		//	//	sLook += node.Name;
		//	//	Console.WriteLine(sLook);
		//	//}

		//	//sA.Append(node.Name); sA.Append(": "); sA.Append(node.Value);
		//	//Print attributes of the node
		//	//WriteAttributesGeneric(node);


		//	////Print attributes of the node
		//	//if (node.Attributes != null)
		//	//{
		//	//	XmlAttributeCollection attrs = node.Attributes;
		//	//	foreach (XmlAttribute attr in attrs)
		//	//	{
		//	//		Console.WriteLine("Attribute Name = " + attr.Name + "; Attribute Value = " + attr.Value);
		//	//	}
		//	//}

		//	if (processChildNodes)
		//	{
		//		//Print individual children of the node, gets only direct children of the node
		//		XmlNodeList children = node.ChildNodes;
		//		foreach (XmlNode child in children)
		//		{
		//			WriteNodes(child);
		//		}

		//	}
		//}


		//private void WriteAttributesGeneric(XmlNode xNode, bool newLine = true)
		//{
		//	if (xNode.Attributes != null)
		//	{
		//		XmlAttributeCollection attrs = xNode.Attributes;

		//		if (attrs.Count > 0) sA.Append("  Attributes >>> ");

		//		foreach (XmlAttribute attr in attrs)
		//		{
		//			sA.Append(" "); sA.Append(attr.Name); sA.Append(":"); sA.Append(attr.Value);
		//		}
		//	}
		//	if (newLine) sA.AppendLine("");
		//}

	}
}
