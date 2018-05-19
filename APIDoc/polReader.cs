using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace APIDoc
{
    class PolReader
    {
        private XmlNode _xn;
        private XmlNamespaceManager _man;
        private string _name;
        private string _id;
        private string _type;
        private StringBuilder sA = new StringBuilder(3900);

        public PolReader(XmlNode xn, XmlNamespaceManager man)
        {
            this._xn = xn;
            _man = man;
        }


        public void Analys(string sFolder)
        {
            _name = _xn["l7:Name"].InnerText;
            _id = _xn["l7:Id"].InnerText;
            _type = _xn["l7:Type"].InnerText;

	        Tracker.addName(_id, _name);

			sA.Append("Name:");
            sA.AppendLine(_name);

            sA.Append("Id:");
            sA.AppendLine(_id);

            sA.Append("Type:");
            sA.AppendLine(_type);

            sA.Append("");
            sA.AppendLine("");


            sA.Append("");
            sA.AppendLine("");

	        
			XmlNodeList xnList = _xn.SelectNodes("l7:Resource/l7:Policy/l7:PolicyDetail", _man);
            //go through list of all resource 
            foreach (XmlNode xNode in xnList)
            {
                WriteNodes(xNode);
                sA.AppendLine("---------------");
            }
            xnList = _xn.SelectNodes("l7:Resource/l7:Policy/l7:Resources", _man);
            //go through list of all resource 
            foreach (XmlNode xNode in xnList)
            {
                WriteNodes(xNode);
                sA.AppendLine("---------------");
            }

            //XmlNode xnList = _xn.SelectSingleNode("l7:Resource", _man);
            //WriteNodes(xnList);

            //Console.WriteLine("Name: {0} {1}", _name, _id);


            FileFuncs.WriteTxtFile(sFolder + @"\" + this.fileName(), sA.ToString());
        }


        private string fileName()
        {
            string sF = _name;
            sF = sF.Replace(" ", "_");
            sF = sF.Replace("\\", "_");
            sF = sF.Replace("/", "_");
            sF = sF.Replace("\"", "_");
            sF = sF.Replace("*", "_");
            //sF = sF.Replace("__", "_");

            return sF + ".txt";
        }

        private void WriteNodes(XmlNode node)
        {
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
                        case "l7:Resource":
                            sA.AppendLine("-----------------------------------------------------------------");
                            sA.Append(node.Name);
                            sA.Append(": ");
                            WriteAttributesGeneric(node);
                            sA.AppendLine("-----------------------------------------------------------------");

                            string sP = node.InnerText;
                            sA.Append(sP);
                            policyReader oP = new policyReader(sP);

                            sA.Append(oP.process());
                            break;
                        //print node name & attributes
                        case "l7:ResourceSet":
                            sA.AppendLine("-----------------------------------------------------------------");
                            sA.Append(node.Name);
                            sA.Append(": ");
                            WriteAttributesGeneric(node);
                            sA.AppendLine("-----------------------------------------------------------------");
                            break;
                        //print node name & attributes
                        case "l7:Service":
                        case "l7:ServiceDetail":
                            //sA.AppendLine("");
                            sA.Append(node.Name);
                            sA.Append(": ");
                            WriteAttributesGeneric(node);
                            //sA.AppendLine("");
                            break;

                        case "l7:Verbs":
                        case "l7:Resources":
                            break;
                        case "l7:ServiceMappings":
                        case "l7:HttpMapping":
                        case "l7:Properties":


                            sA.Append("===> ");
                            sA.AppendLine(node.Name);
                            break;
                        case "l7:Name":
                            sA.Append(node.Name);
                            sA.Append(": ");
                            sA.Append(node.InnerText);
                            WriteAttributesGeneric(node);
                            break;
                        case "l7:Property":
                            WriteAttributesGeneric(node, false);
                            sA.Append(", ");
                            sA.AppendLine(node.FirstChild.InnerText);
                            processChildNodes = false;
                            break;


                        default:
                            sA.Append(node.Name);
                            sA.Append(": ");
                            sA.Append(node.InnerText);
                            WriteAttributesGeneric(node);
                            break;
                    }

                    break;
                //skip text
                case XmlNodeType.Text:
                    break;

                default:
                    sA.Append(node.Name);
                    sA.Append(": ");
                    sA.Append(node.Value);
                    WriteAttributesGeneric(node);
                    break;
            }


            ////Print the node type, node name and node value of the node
            //if (node.NodeType == XmlNodeType.Text)
            //{
            //	//sA.Append(node.Name); sA.Append(": "); sA.Append(node.Value);
            //}

            //if (node.NodeType == XmlNodeType.Element)
            //{
            //	if (node.HasChildNodes && node.ChildNodes.Count == 1)
            //	{
            //		sA.Append(node.Name); sA.Append(": "); sA.Append(node.InnerText);
            //	}
            //	else
            //	{
            //		sA.Append(node.Name); sA.Append(": "); sA.Append(node.Value);
            //	}

            //}

            //else
            //{

            //	sLook += node.Name;
            //	Console.WriteLine(sLook);
            //}

            //sA.Append(node.Name); sA.Append(": "); sA.Append(node.Value);
            //Print attributes of the node
            //WriteAttributesGeneric(node);


            ////Print attributes of the node
            //if (node.Attributes != null)
            //{
            //	XmlAttributeCollection attrs = node.Attributes;
            //	foreach (XmlAttribute attr in attrs)
            //	{
            //		Console.WriteLine("Attribute Name = " + attr.Name + "; Attribute Value = " + attr.Value);
            //	}
            //}

            if (processChildNodes)
            {
                //Print individual children of the node, gets only direct children of the node
                XmlNodeList children = node.ChildNodes;
                foreach (XmlNode child in children)
                {
                    WriteNodes(child);
                }
            }
        }

        private void WriteResource(XmlNode node)
        {
            //select all services
            XmlNodeList xnList = node.SelectNodes("l7:Policy", _man);

            //go through list of all resource 
            foreach (XmlNode sNodeService in xnList)
            {
                WriteNodes(sNodeService);
                sA.AppendLine("---------------");
            }
        }

        private void WriteAttributesGeneric(XmlNode xNode, bool newLine = true)
        {
            if (xNode.Attributes != null)
            {
                XmlAttributeCollection attrs = xNode.Attributes;

                if (attrs.Count > 0) sA.Append("  Attributes >>> ");

                foreach (XmlAttribute attr in attrs)
                {
                    sA.Append(" ");
                    sA.Append(attr.Name);
                    sA.Append(":");
                    sA.Append(attr.Value);
                }
            }

            if (newLine) sA.AppendLine("");
        }
    }
}