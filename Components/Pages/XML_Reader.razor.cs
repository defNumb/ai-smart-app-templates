using Microsoft.AspNetCore.Components.Forms;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DBTransferProject.Models;
namespace DBTransferProject.Components.Pages
{
    public partial class XML_Reader
    {

        private IBrowserFile? selectedFile;
        private List<string> userMessages = new List<string>();
        private string? filePreview;
        private string sqlPreview = string.Empty;

        // TODO ADD DESCRIPTION
        private async Task HandleXML(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;
            if (selectedFile != null)
            {
                var fileExtension = Path.GetExtension(selectedFile.Name).ToLowerInvariant();
                switch (fileExtension)
                {
                    case ".xml":
                        userMessages.Clear();
                        userMessages.Add("> XML file detected. Preparing preview...");
                        await GenerateXmlPreviewFromFile();
                        var rootNode = await ParseXmlToTree(selectedFile);
                        var generatedSql = GenerateSqlFromXmlTree(rootNode);
                        sqlPreview = generatedSql;
                        userMessages.Add("> SQL preview generated successfully.");
                        break;
                    default:
                        userMessages.Add("> Unsupported file type for preview.");
                        break;
                }
                await InvokeAsync(StateHasChanged); // Ensure UI updates with the message
            }
        }
        // TODO ADD DESCRIPTION
        private async Task GenerateXmlPreviewFromFile()
        {
            if (selectedFile == null)
            {
                userMessages.Add("> No file selected.");
                return;
            }

            try
            {
                using (var stream = selectedFile.OpenReadStream(maxAllowedSize: 1024 * 1024)) // Set a limit to file size, e.g., 1MB
                {
                    var settings = new XmlReaderSettings
                    {
                        Async = true,
                        IgnoreWhitespace = true // Ignore whitespace to simplify the preview
                    };

                    var previewHtml = new StringBuilder("<pre class='xml-preview'>"); // Use <pre> for preformatted text

                    using (var reader = XmlReader.Create(stream, settings))
                    {
                        while (await reader.ReadAsync())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    previewHtml.Append($"<span class='xml-element'>{new string(' ', reader.Depth * 2)}&lt;{reader.Name}");

                                    if (reader.HasAttributes)
                                    {
                                        previewHtml.Append("<span class='xml-attribute'>");
                                        while (reader.MoveToNextAttribute())
                                        {
                                            previewHtml.Append($" {reader.Name}=\"{reader.Value}\"");
                                        }
                                        previewHtml.Append("</span>");
                                        // Move the reader back to the element node.
                                        reader.MoveToElement();
                                    }

                                    if (reader.IsEmptyElement)
                                    {
                                        previewHtml.AppendLine("/&gt;</span>");
                                    }
                                    else
                                    {
                                        previewHtml.AppendLine("&gt;</span>");
                                    }
                                    break;

                                case XmlNodeType.Text:
                                    previewHtml.AppendLine($"<span class='xml-text'>{new string(' ', (reader.Depth + 1) * 2)}{reader.Value}</span>");
                                    break;

                                case XmlNodeType.EndElement:
                                    previewHtml.AppendLine($"<span class='xml-element'>{new string(' ', reader.Depth * 2)}&lt;/{reader.Name}&gt;</span>");
                                    break;
                            }
                        }
                    }

                    previewHtml.Append("</pre>");
                    filePreview = previewHtml.ToString();
                    userMessages.Add("> XML preview generated successfully.");
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error generating XML preview: {ex.Message}";
                filePreview = errorMessage;
                userMessages.Add(errorMessage);
            }

            await InvokeAsync(StateHasChanged); // Ensure UI updates with the preview and messages
        }

        // TODO ADD DESCRIPTION
        public async Task<XmlTreeNode> ParseXmlToTree(IBrowserFile xmlFile)
        {
            XmlTreeNode rootNode = null;
            XmlTreeNode currentNode = null;

            // OpenReadStream provides a Stream to access the file's content.
            // Note: Consider specifying a maximum allowed size to prevent large files from causing memory issues.
            using (var stream = xmlFile.OpenReadStream(maxAllowedSize: 1024 * 1024)) // Adjust the size limit as needed
            {
                var settings = new XmlReaderSettings
                {
                    Async = true,
                    IgnoreComments = true,
                    IgnoreWhitespace = true // Adjust according to your needs
                };

                using (var reader = XmlReader.Create(stream, settings))
                {
                    while (await reader.ReadAsync())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                var newNode = new XmlTreeNode(reader.Name, currentNode);
                                if (currentNode != null)
                                {
                                    currentNode.Children.Add(newNode);
                                }
                                else
                                {
                                    rootNode = newNode; // First element, set as root
                                }

                                if (reader.HasAttributes)
                                {
                                    reader.MoveToFirstAttribute();
                                    do
                                    {
                                        newNode.Attributes.Add(reader.Name, reader.Value);
                                    } while (reader.MoveToNextAttribute());
                                    reader.MoveToElement(); // Move back to the element node
                                }

                                if (!reader.IsEmptyElement)
                                {
                                    currentNode = newNode; // Update current node if not an empty element
                                }
                                break;

                            case XmlNodeType.Text:
                                if (currentNode != null)
                                {
                                    currentNode.Content = reader.Value.Trim();
                                }
                                break;

                            case XmlNodeType.EndElement:
                                if (currentNode != null)
                                {
                                    currentNode = currentNode.Parent; // Move back up to the parent node
                                }
                                break;
                        }
                    }
                }
            }

            return rootNode; // Return the root of the constructed tree
        }

     public string GenerateSqlFromXmlTree(XmlTreeNode rootNode)
{
    var uniqueAttributes = new HashSet<string>();

    // Collects unique attribute names from the entire XML structure
    void CollectAttributes(XmlTreeNode node)
    {
        if (node == null) return;

        foreach (var attribute in node.Attributes)
        {
            uniqueAttributes.Add(attribute.Key); // Add attribute name to HashSet
        }

        // Recursively collect attributes from child nodes
        foreach (var child in node.Children)
        {
            CollectAttributes(child);
        }
    }

    // Call CollectAttributes to populate uniqueAttributes
    CollectAttributes(rootNode);

    // Declare SQL variables for each unique attribute found
    var variableDeclarations = new StringBuilder();
    foreach (var attribute in uniqueAttributes)
    {
        variableDeclarations.AppendLine($"DECLARE @{attribute} NVARCHAR(MAX);");
    }

    var sqlBuilder = new StringBuilder($"{variableDeclarations.ToString()}DECLARE @ItemsXML XML;\nSET @ItemsXML = (\n");

    // Recursive method to traverse the tree and generate SQL with placeholders for all nodes
    void TraverseAndGenerateSql(XmlTreeNode node, string indent = "  ")
    {
        if (node == null) return;

        // Start the SELECT statement for this node with a placeholder for attributes or content
        sqlBuilder.Append($"{indent}SELECT");

        // If the node has attributes, include them in the SQL statement
        if (node.Attributes.Any())
        {
            foreach (var attribute in node.Attributes)
            {
                sqlBuilder.Append($" @{attribute.Key} AS '{node.Name}/@{attribute.Value}'");
            }
        }

        // If the node has content, use it as a placeholder value
        else if (!string.IsNullOrWhiteSpace(node.Content))
        {
            sqlBuilder.Append($" '{node.Content}' AS [{node.Name}]");
        }

        // If there are no attributes or content, insert a dummy placeholder to avoid empty SELECT statements
        else
        {
            sqlBuilder.Append($" NULL AS [{node.Name}]");
        }

        // Recursively process child nodes, if any
        if (node.Children.Any())
        {
            sqlBuilder.Append(",");
            foreach (var child in node.Children)
            {
                sqlBuilder.Append("\n");
                sqlBuilder.AppendLine($"{indent} (");
                TraverseAndGenerateSql(child, indent + "  ");
                sqlBuilder.AppendLine($"{indent} ),");
            }
            sqlBuilder.Length -= 2; // Remove the trailing comma and line break
            sqlBuilder.AppendLine($"{indent}FOR XML PATH('{node.Name}'), TYPE");
        }

        // Close the SELECT statement for this node with the correct FOR XML PATH statement
        else
        {
            sqlBuilder.Append($" FOR XML PATH('{node.Name}'), TYPE");

            // If it's not the root node, add a new line after the closing tag
            if (node != rootNode)
            {
                sqlBuilder.AppendLine();
            }
        }
    }

    // Start the recursive traversal from the root node
    TraverseAndGenerateSql(rootNode, "  ");

    // Close the main SQL block with FOR XML PATH to wrap everything in the specified root element
    sqlBuilder.Append("\n) FOR XML PATH('Root'), TYPE -- Adjust 'Root' as necessary to match your XML structure's root element name");

    return sqlBuilder.ToString();
}
    }
}
