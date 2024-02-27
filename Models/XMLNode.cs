using System.Xml;

namespace DBTransferProject.Models
{
    public class XmlTreeNode
    {
        public string Name { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
        public List<XmlTreeNode> Children { get; set; }
        public XmlTreeNode Parent { get; set; }
        public string Content { get; set; }

        public XmlTreeNode(string name, XmlTreeNode parent)
        {
            Name = name;
            Attributes = new Dictionary<string, string>();
            Children = new List<XmlTreeNode>();
            Parent = parent;
            Content = string.Empty;
        }
    }
}
