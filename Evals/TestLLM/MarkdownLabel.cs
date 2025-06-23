using Markdig;
using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace TestLLM
{
    public class MarkdownLabel : Label
    {
        public static readonly BindableProperty MarkdownTextProperty =
            BindableProperty.Create(nameof(MarkdownText), typeof(string), typeof(MarkdownLabel), string.Empty, propertyChanged: OnMarkdownTextChanged);

        public string MarkdownText
        {
            get => (string)GetValue(MarkdownTextProperty);
            set => SetValue(MarkdownTextProperty, value);
        }

        private static void OnMarkdownTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MarkdownLabel label)
            {
                label.UpdateFormattedText();
            }
        }

        private void UpdateFormattedText()
        {
            if (string.IsNullOrEmpty(MarkdownText))
            {
                Text = string.Empty;
                return;
            }

            try
            {
                // Convert markdown to HTML first
                var pipeline = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .Build();
                
                var html = Markdown.ToHtml(MarkdownText, pipeline);
                
                // Convert HTML to FormattedString for MAUI
                var formattedString = ConvertHtmlToFormattedString(html);
                FormattedText = formattedString;
            }
            catch (Exception)
            {
                // Fallback to plain text if markdown parsing fails
                Text = MarkdownText;
            }
        }

        private FormattedString ConvertHtmlToFormattedString(string html)
        {
            var formattedString = new FormattedString();
            
            // Simple HTML to FormattedString conversion
            // This is a basic implementation - you might want to enhance it for more complex HTML
            
            // Remove HTML tags and convert basic formatting
            var text = html
                .Replace("<strong>", "**")
                .Replace("</strong>", "**")
                .Replace("<em>", "*")
                .Replace("</em>", "*")
                .Replace("<code>", "`")
                .Replace("</code>", "`")
                .Replace("<pre>", "```\n")
                .Replace("</pre>", "\n```")
                .Replace("<p>", "")
                .Replace("</p>", "\n")
                .Replace("<br>", "\n")
                .Replace("<br/>", "\n")
                .Replace("<br />", "\n")
                .Replace("<h1>", "# ")
                .Replace("</h1>", "\n")
                .Replace("<h2>", "## ")
                .Replace("</h2>", "\n")
                .Replace("<h3>", "### ")
                .Replace("</h3>", "\n")
                .Replace("<h4>", "#### ")
                .Replace("</h4>", "\n")
                .Replace("<h5>", "##### ")
                .Replace("</h5>", "\n")
                .Replace("<h6>", "###### ")
                .Replace("</h6>", "\n")
                .Replace("<ul>", "")
                .Replace("</ul>", "\n")
                .Replace("<ol>", "")
                .Replace("</ol>", "\n")
                .Replace("<li>", "• ")
                .Replace("</li>", "\n")
                .Replace("<blockquote>", "> ")
                .Replace("</blockquote>", "\n")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&")
                .Replace("&quot;", "\"")
                .Replace("&#39;", "'");

            // Split by lines and process each line
            var lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    // Check for headings
                    if (line.StartsWith("# "))
                    {
                        formattedString.Spans.Add(new Span
                        {
                            Text = line.Substring(2),
                            TextColor = TextColor,
                            FontSize = FontSize + 8,
                            FontFamily = FontFamily,
                            FontAttributes = FontAttributes.Bold
                        });
                    }
                    else if (line.StartsWith("## "))
                    {
                        formattedString.Spans.Add(new Span
                        {
                            Text = line.Substring(3),
                            TextColor = TextColor,
                            FontSize = FontSize + 4,
                            FontFamily = FontFamily,
                            FontAttributes = FontAttributes.Bold
                        });
                    }
                    else if (line.StartsWith("### "))
                    {
                        formattedString.Spans.Add(new Span
                        {
                            Text = line.Substring(4),
                            TextColor = TextColor,
                            FontSize = FontSize + 2,
                            FontFamily = FontFamily,
                            FontAttributes = FontAttributes.Bold
                        });
                    }
                    // Check for blockquotes
                    else if (line.StartsWith("> "))
                    {
                        formattedString.Spans.Add(new Span
                        {
                            Text = line,
                            TextColor = Color.FromHex("#6B7280"),
                            FontSize = FontSize,
                            FontFamily = FontFamily,
                            FontAttributes = FontAttributes.Italic
                        });
                    }
                    // Check for code blocks
                    else if (line.StartsWith("```"))
                    {
                        formattedString.Spans.Add(new Span
                        {
                            Text = line,
                            TextColor = Color.FromHex("#059669"),
                            FontSize = FontSize - 1,
                            FontFamily = "Courier New",
                            FontAttributes = FontAttributes.None
                        });
                    }
                    // Check for inline code
                    else if (line.Contains("`"))
                    {
                        var spans = ProcessInlineCode(line);
                        foreach (var span in spans)
                        {
                            formattedString.Spans.Add(span);
                        }
                    }
                    // Check for bold text (converted from **text** or __text__)
                    else if (line.Contains("**") || line.Contains("__"))
                    {
                        var spans = ProcessBoldText(line);
                        foreach (var span in spans)
                        {
                            formattedString.Spans.Add(span);
                        }
                    }
                    else
                    {
                        formattedString.Spans.Add(new Span
                        {
                            Text = line,
                            TextColor = TextColor,
                            FontSize = FontSize,
                            FontFamily = FontFamily
                        });
                    }
                    
                    // Add line break if not the last line
                    if (i < lines.Length - 1)
                    {
                        formattedString.Spans.Add(new Span { Text = "\n" });
                    }
                }
                else if (i < lines.Length - 1)
                {
                    // Add line break for empty lines
                    formattedString.Spans.Add(new Span { Text = "\n" });
                }
            }

            return formattedString;
        }

        private List<Span> ProcessInlineCode(string text)
        {
            var spans = new List<Span>();
            var parts = text.Split('`');
            
            for (int i = 0; i < parts.Length; i++)
            {
                if (i % 2 == 0)
                {
                    // Regular text
                    if (!string.IsNullOrEmpty(parts[i]))
                    {
                        spans.Add(new Span
                        {
                            Text = parts[i],
                            TextColor = TextColor,
                            FontSize = FontSize,
                            FontFamily = FontFamily
                        });
                    }
                }
                else
                {
                    // Code text
                    if (!string.IsNullOrEmpty(parts[i]))
                    {
                        spans.Add(new Span
                        {
                            Text = parts[i],
                            TextColor = Color.FromHex("#059669"),
                            FontSize = FontSize - 1,
                            FontFamily = "Courier New",
                            FontAttributes = FontAttributes.None
                        });
                    }
                }
            }
            
            return spans;
        }

        private List<Span> ProcessBoldText(string text)
        {
            var spans = new List<Span>();
            var parts = text.Split(new[] { "**", "__" }, StringSplitOptions.None);
            
            for (int i = 0; i < parts.Length; i++)
            {
                if (i % 2 == 0)
                {
                    // Regular text
                    if (!string.IsNullOrEmpty(parts[i]))
                    {
                        spans.Add(new Span
                        {
                            Text = parts[i],
                            TextColor = TextColor,
                            FontSize = FontSize,
                            FontFamily = FontFamily
                        });
                    }
                }
                else
                {
                    // Bold text
                    if (!string.IsNullOrEmpty(parts[i]))
                    {
                        spans.Add(new Span
                        {
                            Text = parts[i],
                            TextColor = TextColor,
                            FontSize = FontSize,
                            FontFamily = FontFamily,
                            FontAttributes = FontAttributes.Bold
                        });
                    }
                }
            }
            
            return spans;
        }
    }
} 