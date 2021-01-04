using HtmlAgilityPack;
using Richasy.Controls.Reader.Models.Epub;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Richasy.Controls.Reader.Models
{
    internal class HtmlHelper
    {
        public string HtmlContent { get; set; }
        private ReaderStyle Style { get; set; }

        public List<Block> RenderBlocks { get; private set; }
        public List<IdIndex> IdList { get; private set; }

        public EventHandler<LinkEventArgs> LinkTapped;
        public EventHandler<ImageEventArgs> ImageTapped;

        private HtmlDocument HtmlDocument = new HtmlDocument();
        private List<EpubByteFile> Images;

        private int _readPointer = 0;

        public string TotalInnerText { get; private set; }


        public HtmlHelper(List<EpubByteFile> images, ReaderStyle style)
        {
            Images = images;
            Style = style;
        }

        public async Task InitAsync(string html)
        {
            HtmlContent = GetBodyString(html);
            RenderBlocks = new List<Block>();
            IdList = new List<IdIndex>();
            _readPointer = 0;
            HtmlDocument.LoadHtml(HtmlContent);
            TotalInnerText = HtmlDocument.DocumentNode.InnerText;

            foreach (HtmlNode img in HtmlDocument.DocumentNode.Descendants("img"))
            {
                try
                {
                    string src = img.Attributes["src"].Value;
                    var sp = src.Split('/');
                    src = sp.Last();
                    var image = Images.Where(i => i.AbsolutePath.IndexOf(src, StringComparison.OrdinalIgnoreCase) != -1).FirstOrDefault();
                    if (image == null)
                        continue;
                    string base64 = Convert.ToBase64String(image.Content);
                    img.Attributes["src"].Value = base64;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            await RenderAsync(HtmlDocument.DocumentNode.FirstChild, null);
        }

        public static string GetBodyString(string html)
        {
            var regex = new Regex(@"<body(.|\s|\r|\n|\f)*</body>");
            var content = regex.Match(html).Value;
            return content ?? "";
        }

        public async Task RenderAsync(HtmlNode node, Block parent)
        {

            if (node != null)
            {
                if (node.Name == "#text")
                {
                    string text = node.InnerText?.Trim() ?? "";
                    if (string.IsNullOrEmpty(text))
                        return;
                    else
                    {
                        _readPointer += text.Length;
                        if (parent == null)
                        {
                            var p = new Paragraph();
                            p.Margin = new Thickness(0, 0, 0, Style.SegmentSpacing);
                            p.TextIndent = Style.TextIndent * Style.FontSize;
                            p.Inlines.Add(new Run() { Text = text });
                            RenderBlocks.Add(p);
                        }
                        else
                        {
                            var p = parent as Paragraph;
                            p.TextIndent = Style.TextIndent * Style.FontSize;
                            p.Margin = new Thickness(0, 0, 0, Style.SegmentSpacing);
                            p.Inlines.Add(new Run() { Text = text });
                        }
                        _readPointer += Convert.ToInt32(Style.TextIndent);
                    }
                }
                else
                {
                    CheckNodeId(node);
                    if (node.ChildNodes.Count > 0)
                    {
                        foreach (var child in node.ChildNodes)
                        {
                            try
                            {
                                var block = await CreateElementFromNode(child, parent);
                                if (parent == null)
                                    RenderBlocks.Add(block);
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    }
                }
            }
        }

        public bool IsBlockNode(HtmlNode node)
        {
            string[] blockNames = new string[]
            {
                "div",
                "p",
                "hr",
                "img",
                "section",
                "nav",
                "aside",
                "blockquote",
                "article",
                "header",
                "ol",
                "ul",
                "table",
                "pre"
            };
            return blockNames.Contains(node.Name.ToLower());
        }

        private async Task<Block> CreateElementFromNode(HtmlNode node, Block parent)
        {
            if (parent == null)
                parent = new Paragraph();
            var p = parent as Paragraph;
            p.Margin = new Thickness(0, 0, 0, Style.SegmentSpacing);
            
            switch (node.Name.ToLower())
            {
                case "#text":
                    if (!string.IsNullOrEmpty(node.InnerText.Trim()))
                    {
                        string text = node.GetDirectInnerText();
                        if (!string.IsNullOrEmpty(text))
                        {
                            text = text.Trim();
                            _readPointer += text.Length;
                            p.TextIndent = Style.TextIndent * Style.FontSize;
                            p.Inlines.Add(new Run() { Text = WebUtility.HtmlDecode(text) });
                            _readPointer += Convert.ToInt32(Style.TextIndent);
                        }
                    }
                    break;
                case "hr":
                    p.Inlines.Add(CreateLineThrough());
                    break;
                case "img":
                case "image":
                    var image = await CreateImageAsync(node, p.Inlines.Count != 0);
                    if (image != null)
                        p.Inlines.Add(image);
                    break;
                case "div":
                case "section":
                case "nav":
                case "aside":
                case "article":
                case "header":
                    RenderBlocks.Add(parent);
                    await RenderAsync(node, null);
                    break;
                case "p":
                case "span":
                    await RenderAsync(node, p);
                    break;
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    p.Inlines.Add(CreateHeader(node));
                    _readPointer += Convert.ToInt32(Style.TextIndent);
                    break;
                case "blockquote":
                    p.Inlines.Add(CreateBlockquote(node));
                    _readPointer += Convert.ToInt32(Style.TextIndent);
                    break;
                case "ol":
                case "ul":
                    //CreateList(node, p, node.Name.ToLower() == "ol");
                    if (!string.IsNullOrEmpty(node.InnerText.Trim()))
                    {
                        p.TextIndent = Style.TextIndent * Style.FontSize;
                        p.Inlines.Add(new Run() { Text = WebUtility.HtmlDecode(node.GetDirectInnerText().Trim()) });
                        _readPointer += Convert.ToInt32(Style.TextIndent);
                    }
                    break;
                case "b":
                case "bold":
                case "strong":
                    p.Inlines.Add(CreateBold(node));
                    break;
                case "a":
                    if (node.ChildNodes.Count >= 1 && (node.FirstChild.Name.Equals("img", StringComparison.OrdinalIgnoreCase)))
                    {
                        var hyperImage = await CreateImageAsync(node.FirstChild, true, node);
                        if (hyperImage != null)
                            p.Inlines.Add(hyperImage);
                    }
                    else
                    {
                        var link = CreateHyperLink(node);
                        if (link != null)
                            p.Inlines.Add(link);
                        _readPointer += Convert.ToInt32(Style.TextIndent);
                    }

                    break;
                case "sup":
                    p.Inlines.Add(CreateSuperscript(node));
                    break;
                case "sub":
                    p.Inlines.Add(CreateSubscript(node));
                    break;
                case "em":
                case "i":
                    p.Inlines.Add(CreateItalic(node));
                    break;
                case "u":
                    p.Inlines.Add(CreateUnderline(node));
                    break;
                case "s":
                case "del":
                    p.Inlines.Add(CreateStrikeThrough(node));
                    break;
                case "code":
                    p.Inlines.Add(CreateCodeInline(node));
                    break;
                default:
                    string other = WebUtility.HtmlDecode(node.GetDirectInnerText().Trim());
                    _readPointer += other.Length;
                    p.Inlines.Add(new Run() { Text = other });
                    _readPointer += Convert.ToInt32(Style.TextIndent);
                    break;
            }
            CheckNodeId(node);
            return p;
        }

        private Inline CreateCodeInline(HtmlNode node)
        {
            if (string.IsNullOrEmpty(node.InnerText))
                return null;
            var span = new Span();
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Style.Foreground),
                Padding = new Thickness(5, 3, 5, 3),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(Style.FontSize / 2.0, 0, Style.FontSize / 2.0, 0)
            };
            string text = WebUtility.HtmlDecode(node.InnerText);
            _readPointer += text.Length;
            var textBlock = new TextBlock
            {
                FontSize = Style.FontSize / 1.5,
                Text = text,
                Foreground = new SolidColorBrush(Style.Foreground)
            };
            border.Child = textBlock;
            var inlineContainer = new InlineUIContainer();
            inlineContainer.Child = border;
            span.Inlines.Add(inlineContainer);
            return span;
        }
        private Inline CreateItalic(HtmlNode node)
        {
            string text = node.InnerText.Trim();
            if (string.IsNullOrEmpty(text))
                return null;
            Span s = new Span();
            _readPointer += text.Length + 1;
            s.Inlines.Add(new Run() { Text = text + " ", FontStyle = FontStyle.Italic });
            return s;
        }
        private Inline CreateUnderline(HtmlNode node)
        {
            if (string.IsNullOrEmpty(node.InnerText.Trim()))
                return null;
            Span s = new Span();
            string text = node.InnerText.Trim();
            _readPointer += text.Length;
            s.Inlines.Add(new Run() { Text = text, TextDecorations = TextDecorations.Underline });
            return s;
        }
        private Inline CreateStrikeThrough(HtmlNode node)
        {
            if (string.IsNullOrEmpty(node.InnerText.Trim()))
                return null;
            Span s = new Span();
            string text = node.InnerText.Trim();
            _readPointer += text.Length;
            s.Inlines.Add(new Run() { Text = text, TextDecorations = TextDecorations.Strikethrough });
            return s;
        }

        private Inline CreateSubscript(HtmlNode node)
        {
            if (string.IsNullOrEmpty(node.InnerText.Trim()))
                return null;
            string text = node.InnerText.Trim();
            _readPointer += text.Length;
            var run = new Run() { Text = text };
            Typography.SetVariants(run, FontVariants.Subscript);
            return run;
        }

        private Inline CreateSuperscript(HtmlNode node)
        {
            if (node.Descendants().Any(p => p.Name == "a"))
            {
                var linkNode = node.Descendants().Where(p => p.Name == "a").FirstOrDefault();
                var hyp = CreateHyperLink(linkNode, true);
                return hyp;
            }
            else
            {
                if (string.IsNullOrEmpty(node.InnerText.Trim()))
                    return null;
                string text = node.InnerText.Trim();
                _readPointer += text.Length;
                var run = new Run() { Text = text };
                Typography.SetVariants(run, FontVariants.Superscript);
                return run;
            }
        }



        private Inline CreateBold(HtmlNode node)
        {
            if (string.IsNullOrEmpty(node.InnerText.Trim()))
                return null;
            Bold bold = new Bold();
            string text = node.InnerText.Trim();
            _readPointer += text.Length;
            bold.Inlines.Add(new Run() { Text = text });
            return bold;
        }

        private Inline CreateLineThrough()
        {
            var container = new InlineUIContainer();
            var grid = new Grid();
            var line = new StackPanel();
            line.Width = 100;
            line.Height = 2;
            line.CornerRadius = new CornerRadius(2);
            line.Background = new SolidColorBrush(Style.Foreground);
            line.HorizontalAlignment = HorizontalAlignment.Center;
            line.Margin = new Thickness(0, Style.FontSize, 0, Style.FontSize);
            grid.Children.Add(line);
            container.Child = grid;
            return container;
        }

        private Inline CreateHyperLink(HtmlNode node, bool isSup = false)
        {
            if (string.IsNullOrEmpty(node.InnerText))
                return null;
            var args = CreateLinkArgs(node);
            var hyp = new Hyperlink();
            string text = " " + node.InnerText.Trim() + " ";
            _readPointer += text.Length;
            var run = new Run() { Text = text };
            if (isSup)
            {
                run.FontSize = Style.FontSize / 1.5;
            }
            hyp.Inlines.Add(run);
            hyp.Click += (_s, _e) =>
            {
                LinkTapped?.Invoke(_s, args);
            };
            return hyp;
        }

        private LinkEventArgs CreateLinkArgs(HtmlNode node)
        {
            string link = node.GetAttributeValue("href", "none");
            var args = new LinkEventArgs();
            if (link.Contains('#'))
            {
                var sp = link.Split('#', StringSplitOptions.RemoveEmptyEntries);
                if (sp.Length > 1)
                    args.FileName = sp[0];
                args.Id = sp.Last();
            }
            else if (!link.Contains("://"))
            {
                string name = link.Split('/').Last();
                args.FileName = name;
            }
            else
                args.Link = link;
            return args;
        }

        private async Task<Inline> CreateImageAsync(HtmlNode node, bool isInline = false, HtmlNode linkNode = null)
        {
            var container = new InlineUIContainer();
            var image = new Image();
            string base64 = node.Attributes["src"].Value;
            string alt = node.Attributes["alt"]?.Value ?? "";
            bool isEmptyImage = false;
            BitmapImage bitmap = new BitmapImage();
            try
            {
                bitmap = await Base64ToImg(base64);
            }
            catch (Exception ex)
            { Debug.WriteLine(ex.Message); }

            if (bitmap.PixelWidth == 0)
                return null;
            image.MaxWidth = bitmap.PixelWidth;
            image.MaxHeight = bitmap.PixelHeight;
            image.Stretch = Stretch.Uniform;
            _readPointer++;
            if (!isInline)
                image.Margin = new Thickness(0, Style.FontSize, 0, Style.FontSize);
            else
            {
                image.MaxWidth = Style.FontSize * 1.2;
                bitmap.DecodePixelWidth = Convert.ToInt32(image.MaxWidth);
            }
            image.Source = bitmap;
            if (isEmptyImage)
            {
                image.Width = 82.5;
                image.Height = 100;
            }
            if (linkNode == null)
            {
                image.Tapped += (_s, _e) =>
                {
                    _e.Handled = true;
                    ImageTapped?.Invoke(_s, new ImageEventArgs(base64, alt));
                };
            }
            else
            {
                var args = CreateLinkArgs(node);
                image.Tapped += (_s, _e) =>
                {
                    _e.Handled = true;
                    LinkTapped?.Invoke(_s, args);
                };
            }
            container.Child = image;
            return container;
        }

        private Inline CreateHeader(HtmlNode node)
        {
            var container = new InlineUIContainer();
            var border = new Border();
            border.BorderBrush = new SolidColorBrush(Style.HeaderLineColor);
            border.BorderThickness = new Thickness(0, 0, 0, 2);
            border.Padding = new Thickness(0, 0, 0, Style.FontSize / 2.0);
            int addon = Convert.ToInt32(node.Name.Replace("h", "", StringComparison.OrdinalIgnoreCase));
            string text = node.InnerText.Trim();
            _readPointer += text.Length;
            border.Child = new TextBlock()
            {
                Text = text,
                Foreground = new SolidColorBrush(Style.Foreground),
                FontSize = Style.FontSize + addon,
                TextWrapping = TextWrapping.Wrap,
                FontWeight = FontWeights.Bold
            };
            container.Child = border;
            return container;
        }

        private Inline CreateBlockquote(HtmlNode node)
        {
            string text = node.InnerText.Trim();
            _readPointer += text.Length;
            return new Run
            {
                Text = text,
                FontSize = Style.FontSize / 1.1,
                FontStyle = FontStyle.Oblique
            };

        }

        private void CheckNodeId(HtmlNode node)
        {
            if (!string.IsNullOrEmpty(node.Id))
            {
                IdList.Add(new IdIndex(node.Id, _readPointer));
            }
        }

        private void CreateList(HtmlNode node, Block parent, bool isOrder = false)
        {
            var p = parent as Paragraph;
            if (node.Name.ToLower() == "li")
            {
                int gutter = 0;
                int order = 1;
                HtmlNode temp = node;
                string parentList = isOrder ? "ol" : "ul";
                if (temp.ParentNode != null)
                {
                    while (temp.ParentNode.Name.ToLower() != parentList)
                    {
                        temp = temp.ParentNode;
                        gutter += 1;
                        if (temp.ParentNode == null)
                            break;
                    }
                }
                temp = node;
                if (temp.PreviousSibling != null)
                {
                    while (temp.PreviousSibling.Name.ToLower() == "li")
                    {
                        order += 1;
                        temp = temp.PreviousSibling;
                        if (temp.PreviousSibling == null)
                            break;
                    }
                }
                p.Inlines.Add(new LineBreak());
                if (node.HasChildNodes)
                {
                    if (node.ChildNodes.Any(l => l.Name == "#text"))
                    {
                        var container = new InlineUIContainer();
                        var grid = new Grid();
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Style.ListGutterWidth) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        grid.Margin = new Thickness(gutter * Style.FontSize, Style.FontSize / 2.0, 0, Style.FontSize / 2.0);
                        var sign = new TextBlock();
                        sign.FontSize = Style.FontSize;
                        sign.Foreground = new SolidColorBrush(Style.Foreground);
                        sign.FontFamily = new FontFamily(Style.FontFamily);
                        sign.Text = order.ToString();
                        var content = new TextBlock();
                        content.Text = node.ChildNodes.Where(j => j.Name == "#text").FirstOrDefault().InnerText;
                        content.FontSize = Style.FontSize;
                        content.Foreground = new SolidColorBrush(Style.Foreground);
                        content.FontFamily = new FontFamily(Style.FontFamily);
                        grid.Children.Add(sign);
                        grid.Children.Add(content);
                        Grid.SetColumn(sign, 0);
                        Grid.SetColumn(content, 1);
                        container.Child = grid;
                        p.Inlines.Add(container);
                    }

                    if (node.ChildNodes.Any(l => l.Name.ToLower() == "li"))
                    {
                        var list = node.ChildNodes.Where(l => l.Name.ToLower() == "li").ToList();
                        foreach (var item in list)
                        {
                            CreateList(item, p, isOrder);
                        }
                    }
                }
            }
            else if (node.Name.ToLower() == "ul")
            {
                foreach (var item in node.ChildNodes)
                {
                    CreateList(item, parent, false);
                }
            }
            else if (node.Name.ToLower() == "ol")
            {
                foreach (var item in node.ChildNodes)
                {
                    CreateList(item, parent, false);
                }
            }
        }

        private async Task<BitmapImage> Base64ToImg(string b)
        {
            if (!string.IsNullOrEmpty(b))
            {
                var bt = Convert.FromBase64String(b);
                var image = bt.AsBuffer().AsStream().AsRandomAccessStream();
                var result = new BitmapImage();
                await result.SetSourceAsync(image);
                return result;
            }
            else
            {
                return null;
            }
        }
    }

    internal class IdIndex
    {
        public string Id { get; set; }
        public int ContentStart { get; private set; }
        public IdIndex()
        {

        }
        public IdIndex(string id, int start)
        {
            Id = id;
            ContentStart = start;
        }
    }
}
