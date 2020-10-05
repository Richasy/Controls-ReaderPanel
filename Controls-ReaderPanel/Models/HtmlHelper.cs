using HtmlAgilityPack;
using Richasy.Controls.Reader.Models.Epub;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Gaming.Input.ForceFeedback;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace Richasy.Controls.Reader.Models
{
    internal class HtmlHelper
    {
        private string HtmlContent { get; set; }
        private EpubViewStyle Style { get; set; }

        private List<Block> RenderBlocks { get; set; }

        private HtmlDocument HtmlDocument = new HtmlDocument();

        public HtmlHelper(string html, List<EpubByteFile> images, EpubViewStyle style)
        {
            HtmlContent = GetBodyString(html);
            Style = style;
            RenderBlocks = new List<Block>();
            HtmlDocument.LoadHtml(HtmlContent);

            foreach (HtmlNode img in HtmlDocument.DocumentNode.Descendants("img"))
            {
                try
                {
                    string src = img.Attributes["src"].Value;
                    var sp = src.Split('/');
                    src = sp.Last();
                    var image = images.Where(i => i.AbsolutePath.IndexOf(src, StringComparison.OrdinalIgnoreCase) != -1).FirstOrDefault();
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
        }
        string GetBodyString(string html)
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
                        if (parent == null)
                        {
                            var p = new Paragraph();
                            p.TextIndent = Style.TextIndent * Style.FontSize;
                            p.Inlines.Add(new Run() { Text = text });
                            RenderBlocks.Add(p);
                        }
                        else
                        {
                            var p = parent as Paragraph;
                            p.Inlines.Add(new Run() { Text = text });
                        }
                    }
                }
                else
                {
                    if (node.ChildNodes.Count > 0)
                    {
                        foreach (var child in node.ChildNodes)
                        {
                            var block = await CreateElementFromNode(child, null);
                            RenderBlocks.Add(block);
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
            bool hasParent = parent != null;
            if (parent == null)
                parent = new Paragraph();
            var p = parent as Paragraph;
            switch (node.Name.ToLower())
            {
                case "hr":
                    p.Inlines.Add(CreateLineThrough());
                    break;
                case "img":
                case "image":
                    var image = await CreateImageAsync(node, hasParent);
                    if (image != null)
                        p.Inlines.Add(image);
                    break;
                case "p":
                case "div":
                case "section":
                case "nav":
                case "aside":
                case "article":
                case "header":
                    await RenderAsync(node, parent);
                    break;
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    p.Inlines.Add(CreateHeader(node));
                    break;
                case "blockquote":
                    p.Inlines.Add(CreateBlockquote(node));
                    break;
                case "ol":
                case "ul":
                    CreateList(node, p, node.Name.ToLower() == "ol");
                    break;
                default:
                    break;
            }
            return parent;
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

        private async Task<Inline> CreateImageAsync(HtmlNode node, bool isInline = false)
        {
            var container = new InlineUIContainer();
            var image = new Image();
            string base64 = node.Attributes["src"].Value;
            bool isEmptyImage = false;
            BitmapImage bitmap = null;
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
            image.Tapped += (_s, _e) =>
            {
                _e.Handled = true;
                //ImageTapped?.Invoke(_s, new ImageEventArgs(base64));
            };
            container.Child = image;
            return container;
        }

        private Inline CreateHeader(HtmlNode node)
        {
            double xi = 1;
            xi += Convert.ToInt32(node.Name.ToLower().Replace("h", "")) / 10.0;
            return new Run()
            {
                Text = node.InnerText,
                FontSize = xi * Style.FontSize
            };
        }

        private Inline CreateBlockquote(HtmlNode node)
        {
            return new Run()
            {
                Text = node.InnerText,
                FontStyle = FontStyle.Italic,
                FontWeight = FontWeights.Bold
            };
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
                while (temp.ParentNode.Name.ToLower() != parentList)
                {
                    temp = temp.ParentNode;
                    gutter += 1;
                }
                temp = node;
                while (temp.PreviousSibling.Name.ToLower() == "li")
                {
                    order += 1;
                    temp = temp.PreviousSibling;
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
}
