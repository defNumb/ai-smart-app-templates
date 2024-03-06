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
        public XmlTreeNode ParseXmlToTree(string filePath)
        {
            XmlTreeNode rootNode = null;
            XmlTreeNode currentNode = null;

            using (var reader = XmlReader.Create(filePath))
            {
                while (reader.Read())
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

            return rootNode; // Return the root of the constructed tree
        }


    }
}
