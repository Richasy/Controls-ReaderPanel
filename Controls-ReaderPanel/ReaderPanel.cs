using Richasy.Controls.Reader.Models;
using Richasy.Controls.Reader.Models.Epub;
using Richasy.Controls.Reader.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Richasy.Controls.Reader
{
    [TemplatePart(Name = "MainPresenter", Type = typeof(ContentPresenter))]
    public partial class ReaderPanel : Control
    {
        public ReaderPanel()
        {
            this.DefaultStyleKey = typeof(ReaderPanel);
            ChapterDivisionRegex = new Regex(@"第(.{1,9})[章节卷集部篇回幕计](\s{0})(.*)($\s*)");
            ChapterEndKey = new string[] { "部", "卷", "章", "节", "集", "回", "幕", "计", " " };
            ChapterExtraKey = new string[] { "序", "前言", "后记", "楔子", "附录", "外传" };
        }

        protected override void OnApplyTemplate()
        {
            _mainPresenter = GetTemplateChild("MainPresenter") as ContentPresenter;
            base.OnApplyTemplate();
        }

        public async Task OpenAsync(StorageFile bookFile, ReaderStyle style)
        {
            if (bookFile == null)
                throw new ArgumentNullException();

            string extension = Path.GetExtension(bookFile.Path).ToLower();
            if(extension!=".epub" && extension != ".txt")
            {
                throw new NotSupportedException("File type not support (Currently only support txt and epub file)");
            }

            if (extension == ".txt")
            {
                if (!(style is TxtViewStyle))
                    throw new ArgumentException("Open txt file need TxtViewStyle argument");
                ReaderType = Enums.ReaderType.Txt;
                Chapters = await GetTxtChapters(bookFile);
                if(_mainPresenter.Content==null || !(_mainPresenter.Content is TxtView))
                {
                    if (_txtView == null)
                    {
                        _txtView = new TxtView();
                        _txtView.PrevPageSelected += OnPrevPageSelected;
                        _txtView.NextPageSelected += OnNextPageSelected;
                        _txtView.LoadingChanged += (_s, _e) => { LoadingChanged?.Invoke(_s, _e); };
                        _txtView.ProgressChanged += (_s, _e) => { ProgressChanged?.Invoke(_s, _e); };
                        _txtView.TouchHolding += (_s, _e) => { TouchHolding?.Invoke(_s, _e); };
                        _txtView.TouchTapped += (_s, _e) => { TouchTapped?.Invoke(_s, _e); };
                    }
                    _mainPresenter.Content = _txtView;
                }
                _txtContent = await GetTxtContent(bookFile);
                _txtView.ViewStyle = style as TxtViewStyle;
            }
            else
            {
                if (!(style is EpubViewStyle))
                    throw new ArgumentException("Open txt file need TxtViewStyle argument");
                ReaderType = Enums.ReaderType.Epub;
                _epubContent = await EpubReader.Read(bookFile, Encoding.Default);
                Chapters = GetEpubChapters(_epubContent);
                if (_mainPresenter.Content == null || !(_mainPresenter.Content is EpubView))
                {
                    if (_epubView == null)
                    {
                        _epubView = new EpubView();
                        _epubView.PrevPageSelected += OnPrevPageSelected;
                        _epubView.NextPageSelected += OnNextPageSelected;
                        _epubView.LoadingChanged += (_s, _e) => { LoadingChanged?.Invoke(_s, _e); };
                        _epubView.ProgressChanged += (_s, _e) => { ProgressChanged?.Invoke(_s, _e); };
                        _epubView.TouchHolding += (_s, _e) => { TouchHolding?.Invoke(_s, _e); };
                        _epubView.TouchTapped += (_s, _e) => { TouchTapped?.Invoke(_s, _e); };
                    } 
                    _mainPresenter.Content = _epubView;
                }
                _epubView.Init(_epubContent, style as EpubViewStyle);
            }
        }
    }
}
