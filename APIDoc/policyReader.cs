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
							sA.AppendWithSpaces("All assertions must evaluate to true. ", _spaces);//TODO handle required attribute value
							//sAppend("All assertions must evaluate to true. ");
							//sA.Append(node.Name); sA.Append(": "); WriteAttributesGeneric(node);
							//Print individual children of the node, gets only direct children of the node
							_spaces += INDENT;
							foreach (XmlNode child in children)
							{
								WriteNodes(child);
							}
							_spaces -= INDENT;
							break;
						case "wsp:OneOrMore":
							sA.AppendWithSpaces("At least one assertion must evaluate to true", _spaces);//TODO handle required attribute value
							//sA.Append(node.Name); sA.Append(": "); WriteAttributesGeneric(node);
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
                            //L7PAssertionComment oC = new L7PAssertionComment(node, _man);
                            //oC.Process();
                            //sA.AppendLine(oC.ToString());

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
	                        sA.AppendWithSpaces("@Comment " + AttributesText(node, true, false), _spaces);
							//sA.Append(AttributesText(node,true,false));
							//WriteAttributesGeneric(node);
							Getchilds(children);
							break;
						case "L7p:Content":
							//sAppend("Authentication  ----  ");
							sAppend(node.Name); sA.Append(": "); sA.Append(AttributesText(node, true, true));
							Getchilds(children);
							break;
						case "L7p:ComparisonAssertion":
							sA.AppendWithSpaces("Compare Variable: " + L7p_ComparisonAssertion(node), _spaces);
							break;
						case "L7p:Encapsulated":
							L7PEncapsulated oE = new L7PEncapsulated(node,_man); oE.Process();
							sA.AppendWithSpaces(oE.ToString(), _spaces);
							break;
					    case "L7p:CustomizeErrorResponse":
					        L7PCustomizeErrorResponse oCerr = new L7PCustomizeErrorResponse(node, _man); oCerr.Process();
					        sA.AppendWithSpaces(oCerr.ToString(), _spaces);
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
				if (newLine) sTemp.Append("");
				return sTemp.ToString();
			}

			return string.Empty;
		}

	    private string L7p_SetVariable(XmlNode xNode)
	    {
            StringBuilder sB = new StringBuilder(450);
	        string sVariableToSet = xNode.AA_AttributeValueXPAth("L7p:VariableToSet/@stringValue", _man);
	        string sBase64Encoded = xNode.AA_AttributeValueDecodeBase64("L7p:Base64Expression/@stringValue", _man);
	        string sDatatype = xNode.AA_AttributeValueXPAth("L7p:DataType/@stringValue", _man);
	        string sContentType = xNode.AA_AttributeValueXPAth("L7p:ContentType/@stringValue", _man);
	        string sEnabled = xNode.AA_AttributeValueXPAth("L7p:Enabled/@booleanValue", _man);

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

		/// <summary>
		/// analyse and format comparision assertion
		/// TODO: fix when multiple items are part of the predicates
		/// </summary>
		/// <param name="xNode"></param>
		/// <returns></returns>
		private string L7p_ComparisonAssertion(XmlNode xNode)
		{
			StringBuilder sB = new StringBuilder(450);
			StringBuilder sTemp = new StringBuilder(450);
			string sExpression1 = xNode.AA_AttributeValueXPAth("L7p:Expression1/@stringValue", _man);

			XmlNodeList xnList = xNode.SelectNodes("L7p:Predicates/L7p:item", _man);
			foreach (XmlNode xn in xnList)
			{
				for (int i = 0; i < xn.Attributes.Count; i++)
				{
					string sName = xn.Attributes[i].Name;
					string sVal = xn.Attributes[i].Value;
					switch (sName)
					{
						case "dataType":
							break;
						case "binary":
							string sCaseSen = "(case sensitive)";
							string sRValue = string.Empty;
							string sNegated = string.Empty;

							string sEq = "is equal to";
							for (int j = 0; j < xn.ChildNodes.Count; j++)
							{
								string sWname = xn.ChildNodes[j].Name;
								string sWval = xn.Attributes[i].Value;
								switch (sWname)
								{
									case "L7p:CaseSensitive":
										sCaseSen = xn.ChildNodes[j].AA_AttributeValue("booleanValue");
										if (sCaseSen == "false") sCaseSen = String.Empty;
										break;
									case "L7p:RightValue":
										sRValue = xn.ChildNodes[j].AA_AttributeValue("stringValue");
										break;
									case "L7p:Negated":
										sNegated = xn.ChildNodes[j].AA_AttributeValue("booleanValue");
										if (sNegated == "true") sEq = "is not equal to";							
										break;
								}
							}

							sTemp.Append(sEq); sTemp.Append(" "); sTemp.Append(sRValue); sTemp.Append(" "); sTemp.Append(sCaseSen); sTemp.Append(" ");
							break;
						default:
							break;
					}
				}
			}

			sB.Append(sExpression1); sB.Append(" ");
			sB.Append(sTemp.ToString()); sB.Append(" ;If Multivalued all values must pass ");
			return sB.ToString();
		}


	}
	/// <summary>
	/// handle Encapsulated Assertions
	/// TODO lookup for Encapsulated assertion and insert the logic tree -- as a reference to associated logic
	/// TODO add to dependency matrix to generate dependency tree
	/// TODO handle Parameters sub nodes
	/// 
	/// </summary>
	public class L7PEncapsulated
	{
		private StringBuilder sbX = new StringBuilder(450);
		private StringBuilder _sbTemp = new StringBuilder(450);
		private readonly XmlNode _xNode;
		private readonly XmlNamespaceManager _man;
		private bool _hasKeys = false;

		private string _sName = string.Empty;
		private string _sGuid = string.Empty;

		public L7PEncapsulated(XmlNode xn, XmlNamespaceManager man)
		{
			this._man = man;
			this._xNode = xn;
		}

		public void Process()
		{
			//get alll comments
			XmlNodeList xnList = _xNode.SelectNodes("L7p:AssertionComment", _man);
			foreach (XmlNode xn in xnList)
			{
				L7PAssertionComment oC = new L7PAssertionComment(xn, _man);
				oC.Process();
				sbX.AppendLine(oC.ToString());
			}
			_sName = _xNode.AA_AttributeValueXPAth("L7p:EncapsulatedAssertionConfigName/@stringValue", _man);
			_sGuid = _xNode.AA_AttributeValueXPAth("L7p:EncapsulatedAssertionConfigGuid/@stringValue", _man);
			//get alll comments
			XmlNodeList xnItems = _xNode.SelectNodes("L7p:Parameters/L7p:entry", _man);
			foreach (XmlNode xi in xnItems)
			{
				_hasKeys = true;
				string sKey = xi.AA_AttributeValueXPAth("L7p:key/@stringValue", _man);
				string sValue = xi.AA_AttributeValueXPAth("L7p:value/@stringValue", _man);
				_sbTemp.Append("  #"); _sbTemp.Append(sKey); _sbTemp.Append(":"); _sbTemp.Append(sValue); _sbTemp.Append("; ");
			}
		}

		public override string ToString()
		{
			sbX.Append(_sName);
			if (_hasKeys)
			{
				sbX.AppendLine();
				sbX.Append(_sbTemp.ToString());
			}
			//sbX.AppendLine();
			return sbX.ToString();
		}

	}

	public class L7PAssertionComment
	{
		private StringBuilder sbX = new StringBuilder(450);
		private StringBuilder _sbTemp = new StringBuilder(450);
		private readonly XmlNode _xNode;
		private readonly XmlNamespaceManager _man;

		string _sLeftComment = string.Empty;
		string _sRightComment = string.Empty;

		public L7PAssertionComment(XmlNode xn, XmlNamespaceManager man)
		{
			this._man = man;
			this._xNode = xn;
		}

		public void Process()
		{
			XmlNodeList xnList = _xNode.SelectNodes("L7p:Properties/L7p:entry", _man);
			foreach (XmlNode xn in xnList)
			{
				string sKey = xn.AA_AttributeValueXPAth("L7p:key/@stringValue", _man);
				string sValue = xn.AA_AttributeValueXPAth("L7p:value/@stringValue", _man);
				switch (sKey)
				{
					case "LEFT.COMMENT":
						_sLeftComment = sValue;
						break;
					case "RIGHT.COMMENT":
						_sRightComment = sValue;
						break;
				}
				sbX.Append("@AssertionComment:"); sbX.Append(_sLeftComment); sbX.Append(" - "); sbX.Append(_sRightComment);
				//sbX.AppendLine();
			}
		}

		public override string ToString()
		{
			return sbX.ToString();
		}

	}

    public class L7PCustomizeErrorResponse
    {
        private StringBuilder sbX = new StringBuilder(450);
        private StringBuilder _sbTemp = new StringBuilder(450);
        private readonly XmlNode _xNode;
        private readonly XmlNamespaceManager _man;
	    private bool _hasKeys = false;

        private string _sContent = string.Empty;
        private string _sContentType = string.Empty;
        private string _sHttpStatus = string.Empty;
	    private string _includePolicyDownloadUrl = string.Empty;
		private string _errorLevel = string.Empty;

		public L7PCustomizeErrorResponse(XmlNode xn, XmlNamespaceManager man)
        {
            this._man = man;
            this._xNode = xn;
        }

        public void Process()
        {
            //get alll comments
            XmlNodeList xnList = _xNode.SelectNodes("L7p:ExtraHeaders/L7p:item", _man);
            foreach (XmlNode xn in xnList)
            {
	            _hasKeys = true;
				string sKey = xn.AA_AttributeValueXPAth("L7p:Key/@stringValue", _man);
                string sValue = xn.AA_AttributeValueXPAth("L7p:Value/@stringValue", _man);
				sbX.Append("  #"); sbX.Append(sKey); sbX.Append(":"); sbX.Append(sValue); sbX.Append("; ");
			}
            _sContent = _xNode.AA_AttributeValueXPAth("L7p:Content/@stringValue", _man);
            _sContentType = _xNode.AA_AttributeValueXPAth("L7p:ContentType/@stringValue", _man);
            _sHttpStatus = _xNode.AA_AttributeValueXPAth("L7p:HttpStatus/@stringValue", _man);
	        _includePolicyDownloadUrl = _xNode.AA_AttributeValueXPAth("L7p:IncludePolicyDownloadURL/@booleanValue", _man);
			_errorLevel = _xNode.AA_AttributeValueXPAth("L7p:ErrorLevel/@errorLevel", _man);
		}

		public override string ToString()
		{
			_sbTemp.Append("Custom Error Response:");
			if (_sContent != string.Empty)
			{
				_sbTemp.Append(" Content:"); _sbTemp.Append(_sContent);
			}
			if (_sContentType != string.Empty)
			{
				_sbTemp.Append(" Type:"); _sbTemp.Append(_sContentType);
			}


			if (_sHttpStatus != string.Empty)
			{
				_sbTemp.Append(" HTTP Status:"); _sbTemp.Append(_sHttpStatus);
			}
			if (_includePolicyDownloadUrl != string.Empty)
			{
				_sbTemp.Append(" Download Policy:"); _sbTemp.Append(_includePolicyDownloadUrl);
			}

			if (_hasKeys)
			{
				_sbTemp.AppendLine();
				_sbTemp.Append(sbX.ToString());
			}
			
            //sbX.AppendLine();
            return _sbTemp.ToString();
        }

    }

}
