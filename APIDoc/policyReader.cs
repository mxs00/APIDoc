using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace APIDoc
{
	public class policyReader
	{
		public const int INDENT = 3;
		public const int SUBINDENT = 1;

		private XmlNode _xn;
		private XmlNamespaceManager _man;
		XmlDocument _xmlDoc;
		private StringBuilder sA = new StringBuilder(3900);
		private StringBuilder sC = new StringBuilder(3900);
		private string _xml;
		private int _spaces;
		private bool displayAttributeName = true;

		public policyReader(string spolicy)
		{
			this._xml = spolicy;
		}

		public string process()
		{

			_xmlDoc = new XmlDocument();
			_xmlDoc.LoadXml(_xml);

			_man = new XmlNamespaceManager(new NameTable());
			_man.AddNamespace("L7p", "http://www.layer7tech.com/ws/policy");
			_man.AddNamespace("wsp", "http://schemas.xmlsoap.org/ws/2002/12/policy");
			_man.AddNamespace("l7", "http://ns.l7tech.com/2010/04/gateway-management");
			_spaces = 0;


			XmlNodeList xnList = _xmlDoc.SelectNodes("wsp:Policy", _man);
			foreach (XmlNode xn in xnList)
			{
				itemReader iPol = new itemReader(xn, _man);
				WriteNodes(xn);
			}


			return sA.ToString();

		}

		private void WriteNodes(XmlNode node)
		{

			XmlNodeList children = node.ChildNodes;
			string sLook = node.Name + "_" + node.ChildNodes.Count;
			int cCount = node.ChildNodes.Count;
			bool processChildNodes = true;

			//sA.Append("Child Count: "); sA.Append(cCount); sA.Append(" : ");
			switch (node.NodeType)
			{
				case XmlNodeType.Element:
					switch (node.Name)
					{
						//read and display policy
						case "wsp:All":
							//sA.Append(StrUtil.PrefixSpaces("All assertions must evaluate to true. ", _spaces));
							sA.AppendWithSpaces("All assertions must evaluate to true. ", _spaces);
							//sAppend("All assertions must evaluate to true. ");
							sAppend(node.Name); sAppend(": "); WriteAttributesGeneric(node);
							//Print individual children of the node, gets only direct children of the node
							_spaces += INDENT;
							foreach (XmlNode child in children)
							{
								WriteNodes(child);
							}
							_spaces -= INDENT;
							break;
						case "wsp:OneOrMore":
							sAppend("At least one assertion must evaluate to true");
							sAppend(node.Name); sAppend(": "); WriteAttributesGeneric(node);
							//Print individual children of the node, gets only direct children of the node
							_spaces += INDENT;
							foreach (XmlNode child in children)
							{
								WriteNodes(child);
							}
							_spaces -= INDENT;
							break;

						//print node name & attributes 

						case "L7p:SslAssertion":
							sAppend("SSL Assertion  ----  ");
							sA.Append(node.Name); sA.Append(": "); WriteAttributesGeneric(node);
							_spaces += SUBINDENT;
							foreach (XmlNode child in children)
							{
								WriteNodes(child);
							}
							_spaces -= SUBINDENT;
							break;
						case "L7p:Authentication":
							//sAppend("Authentication  ----  ");
							sAppend(node.Name); sA.Append(": "); WriteAttributesGeneric(node,false);
							Getchilds(children);
							//_spaces += SUBINDENT;
							//foreach (XmlNode child in children)
							//{
							//	WriteNodes(child);
							//}
							//_spaces -= SUBINDENT;
							break;
						//indent child nodes by 1 
						case "L7p:RadiusAuthenticate":
						case "L7p:HttpRoutingAssertion":
						case "L7p:RequestSizeLimit":
							//case "L7p:ResponseHeaderRules":
							//case "L7p:RequestParamRules":
							//case "L7p:RequestHeaderRules":

							sAppend(node.Name); sA.Append(": "); sA.Append(AttributesText(node, true, false));
							displayAttributeName = false;
							Getchilds(children);
							displayAttributeName = true;
							break;
						//skip node name
						case "L7p:CommentAssertion":
							sA.Append(AttributesText(node, true, false));
							foreach (XmlNode child in children)
							{
								WriteNodes(child);
							}
							break;
					    //
					    case "L7p:SetVariable":
					        sAppend("Set Context Variable: ");
                            sA.Append(L7p_SetVariable(node));
					        break;

                        //remove new line characters
                        case "L7p:Comment":
							sAppend("@Comment ");
							sA.Append(AttributesText(node,true,false));
							//WriteAttributesGeneric(node);
							Getchilds(children);
							break;
						case "L7p:Content":
							//sAppend("Authentication  ----  ");
							sAppend(node.Name); sA.Append(": "); sA.Append(AttributesText(node, true, true));
							Getchilds(children);
							break;
						case "L7p:ComparisonAssertion":
							sA.AppendWithSpaces("Compare Variable: ", _spaces);
							sAppend(L7p_ComparisonAssertion(node));
							//Getchilds(children);
							break;


						default://
							if (cCount <= 0)
							{
								sAppend(node.Name); sA.Append(": "); sA.Append(node.InnerText);
								WriteAttributesGeneric(node);
								//foreach (XmlNode child in children)
								//{
								//	WriteNodes(child);
								//}
							}
							else
							{
								sAppend(node.Name); sA.Append(": "); sA.Append(node.Value);
								WriteAttributesGeneric(node);
								foreach (XmlNode child in children)
								{
									WriteNodes(child);
								}
							}

							break;

					}
					break;
				//skip text
				case XmlNodeType.Text:
					break;

				//case XmlNodeType.Document:
				//	foreach (XmlNode child in children)
				//	{
				//		WriteNodes(child);
				//	}
				//	break;

				default:
					sA.Append(node.Name); sA.Append(": "); sA.Append(node.Value);
					WriteAttributesGeneric(node);
					foreach (XmlNode child in children)
					{
						WriteNodes(child);
					}
					break;
			}

		}

		private void WriteAttributesGeneric(XmlNode xNode, bool newLine = true)
		{
			if (xNode.Attributes != null)
			{
				XmlAttributeCollection attrs = xNode.Attributes;

				if (attrs.Count > 0) sAppend(" >>> ");

				foreach (XmlAttribute attr in attrs)
				{
					sA.Append(" "); sA.Append(attr.Name); sA.Append(":");
					string sV = attr.Value;
					sV = System.Text.RegularExpressions.Regex.Replace(sV, @"\t|\n|\r", "");
					sA.Append(sV);
				}
			}
			if (newLine) sA.AppendLine("");
		}




		private void sAppend(string sX)
		{
			sA.Append(new string(' ', _spaces));
			sA.Append(sX);
		}
		private void sAppendLine(string sX)
		{
			sA.Append(new string(' ', _spaces));
			sA.AppendLine(sX);
		}

		private void Getchilds(XmlNodeList children)
		{
			_spaces += SUBINDENT;
			sC.Clear();
			foreach (XmlNode child in children)
			{
				ChildNodes(child);
			}
			sA.Append(sC.ToString());
			_spaces -= SUBINDENT;
		}
		private void ChildNodes(XmlNode node)
		{

			XmlNodeList children = node.ChildNodes;
			int cCount = node.ChildNodes.Count;

			switch (node.NodeType)
			{
				//skip text
				case XmlNodeType.Text:
					break;
				default:
					if (cCount <= 0)
					{
						sC.Append(new string(' ', _spaces));
						sC.Append(node.Name); sC.Append(": "); sC.Append(node.InnerText);
						sC.Append(AttributesText(node));
					}
					else
					{
						sC.Append(new string(' ', _spaces));
						sC.Append(node.Name); sC.Append(": "); sC.Append(node.Value);
						sC.Append(AttributesText(node));
						foreach (XmlNode child in children)
						{
							ChildNodes(child);
						}
					}
					break;
			}

		}
		private string AttributesText(XmlNode xNode, bool newLine = true,bool displayName = true)
		{
			StringBuilder sTemp = new StringBuilder(900);
			bool hasData = false;
			if (xNode.Attributes != null)
			{
				XmlAttributeCollection attrs = xNode.Attributes;

				if (attrs.Count > 0) sTemp.Append("[");

				foreach (XmlAttribute attr in attrs)
				{
					hasData = true;
					sTemp.Append(" ");
					if (displayName && displayAttributeName)
					{
						sTemp.Append(attr.Name); sTemp.Append(":");
					}				
					string sV = attr.Value;
					sV = System.Text.RegularExpressions.Regex.Replace(sV, @"\t|\n|\r", "");
					sTemp.Append(sV);
				}
			}
			
			if (hasData)
			{
				sTemp.Append(" ]");
				if (newLine) sTemp.AppendLine("");
				return sTemp.ToString();
			}

			else
				return string.Empty;
		}

	    private string L7p_SetVariable(XmlNode xNode)
	    {
            StringBuilder sB = new StringBuilder(450);
	        string sVariableToSet = xNode.AA_AttributeValue("L7p:VariableToSet/@stringValue", _man);
	        string sBase64Encoded = xNode.AA_AttributeValueDecodeBase64("L7p:Base64Expression/@stringValue", _man);
	        string sDatatype = xNode.AA_AttributeValue("L7p:DataType/@stringValue", _man);
	        string sContentType = xNode.AA_AttributeValue("L7p:ContentType/@stringValue", _man);
	        string sEnabled = xNode.AA_AttributeValue("L7p:Enabled/@booleanValue", _man);

            sB.Append(sVariableToSet); sB.Append("="); sB.Append(sBase64Encoded);
	        sB.Append("| ");

	        if (sEnabled != string.Empty)
	        {
	            sB.Append("Enabledl"); sB.Append(":"); sB.Append(sEnabled); sB.Append("| ");
	        }

            if (sDatatype != string.Empty)
	        {
	            sB.Append("DataType"); sB.Append(":"); sB.Append(sDatatype); sB.Append("| ");
            }

	        if (sContentType != string.Empty)
	        {
	            sB.Append("ContentType"); sB.Append(":"); sB.Append(sContentType); sB.Append("| ");
            }

	        sB.AppendLine();

            return sB.ToString();
	        //Console.WriteLine(n.InnerText);
	    }

		private string L7p_ComparisonAssertion(XmlNode xNode)
		{
			StringBuilder sB = new StringBuilder(450);
			string sExpression1 = xNode.AA_AttributeValue("L7p:Expression1/@stringValue", _man);
			string sExpression2 = xNode.AA_AttributeValue("L7p:Expression2/@stringValue", _man);
			string sCaseSensitive = xNode.AA_AttributeValue("L7p:CaseSensitive/@booleanValue", _man);

			string sPredicates = xNode.AA_AttributeValueDecodeBase64("L7p:Predicates/@predicates", _man);
			//string sDatatype = xNode.AA_AttributeValue("L7p:DataType/@stringValue", _man);
			//string sContentType = xNode.AA_AttributeValue("L7p:ContentType/@stringValue", _man);
			//string sEnabled = xNode.AA_AttributeValue("L7p:Enabled/@booleanValue", _man);

			sB.Append(sExpression1); sB.Append("=");
			sB.Append(sExpression2);
			if (sCaseSensitive != string.Empty && sCaseSensitive == "true")
			{
				sB.Append(" (case sensetive);"); 
			}
			//sB.Append("| ");
			XmlNodeList xnList = _xmlDoc.SelectNodes("L7p:Predicates/L7p:item", _man);
			foreach (XmlNode xn in xnList)
			{
				//itemReader iPol = new itemReader(xn, _man);
				//WriteNodes(xn);
			}

			if (sPredicates != string.Empty)
			{


				//sB.Append("Enabledl"); sB.Append(":"); sB.Append(sEnabled); sB.Append("| ");
			}

			//if (sDatatype != string.Empty)
			//{
			//	sB.Append("DataType"); sB.Append(":"); sB.Append(sDatatype); sB.Append("| ");
			//}

			//if (sContentType != string.Empty)
			//{
			//	sB.Append("ContentType"); sB.Append(":"); sB.Append(sContentType); sB.Append("| ");
			//}

			sB.AppendLine();

			return sB.ToString();
			//Console.WriteLine(n.InnerText);
		}


	}
}
