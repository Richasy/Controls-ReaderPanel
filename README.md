<p align="center">
    <img src="https://i.loli.net/2020/11/04/rRldg9FvEpjAuVY.png" align="center" height="80"/>
</p>

<div align="center">

# ReaderPanel

[![Nuget](https://img.shields.io/nuget/v/Richasy.Controls.ReaderPanel)](https://www.nuget.org/packages/Richasy.Controls.ReaderPanel/)

`ReaderPanel` is an integrated local reader control that supports TXT and EPUB

</div>

## Introduction

ReaderPanel is the core of the reader that I stripped from [Clean Reader](https://www.microsoft.com/en-us/p/clean-reader/9mv65l2xfcsk), it contains rich functions. This control designed for desktop applications, you can customize the width of a single column and customize the column reading.

The control was born out of the [ReaderView project](https://github.com/cnbluefire/ReaderView) of [cnbluefire](https://github.com/cnbluefire), and add the column expansion and EPUB support, the EPUB analysis comes from the [EpubSharp project](https://github.com/Asido/EpubSharp/).

***The minimum system version requirement of this control is Windows10 ver 1809***

## Control characteristics

- E-book support in TXT and EPUB format
- Support to customize TXT chapter content splitting through regular expressions
- Support picture display in EPUB, link click
- Support custom selected text menu
- Rich style customization

## Simple start

1. Reference the nuget package in the project:[Richasy.Controls.ReaderPanel](https://www.nuget.org/packages/Richasy.Controls.ReaderPanel/)
2. Add the following code in `App.xaml.cs`

```csharp
public App()
{
    this.InitializeComponent();
    //...
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
}
```

3. Create a page `ReaderPage.xaml`.

**ReaderPage.xaml**

```xml
<Page ...
    xmlns:reader="using:Richasy.Controls.Reader"
    >
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <Button Content="Open book file" Click="Button_Click"/>

        <reader:ReaderPanel Grid.Row="1" x:Name="Reader"
                            ChapterLoaded="Reader_ChapterLoaded"
                            OpenCompleted="Reader_OpenCompleted"
                            />
    </Grid>
</Page>
```

**ReaderPage.xaml.cs**

```csharp
public ObservableCollection<Chapter> ChapterCollection = new ObservableCollection<Chapter>();

public ReaderPage()
{
    this.InitializeComponent();
}

private void Reader_ChapterLoaded(object sender, List<Chapter> e)
{
    ChapterCollection.Clear();
    e.ForEach(p => ChapterCollection.Add(p));
}

private void Reader_OpenCompleted(object sender, EventArgs e)
{
    var chapter = ChapterCollection.First();
    Reader.LoadChapter(chapter);
}

private async void Button_Click(object sender, RoutedEventArgs e)
{
    // From Richasy.Helper.UWP nuget package
    var instance = new Instance();
    var file = await instance.IO.OpenLocalFileAsync(".epub", ".txt");
    if (file != null)
    {
        try
        {
            await Reader.OpenAsync(file, new ReaderStyle());
        }
        catch (Exception ex)
        {
            await new MessageDialog(ex.Message).ShowAsync();
        }

    }
}
```

![](https://i.loli.net/2020/11/04/ywnsEbfALgcMBHR.png)

For more usage methods, please check the **SampleApp** in the project

## IMPORTANT

Generally speaking, only a limited number of text encodings are provided in UWP. If you want to create a novel reader, then you need to support a wide range of text encodings. So you have to add this code in App.xaml.cs: `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);`

## Inside search

The control provides an internal search method to quickly locate matching chapters based on keywords. (NEED BOOK OPENED)

```csharp
var searchResult = await Reader.GetInsideSearchResultAsync(keyword);
if (searchResult.Count > 0)
{
    StringBuilder builder = new StringBuilder();
    foreach (var item in searchResult)
    {
        builder.AppendLine($"Chapter：{item.Chapter.Title}");
        builder.AppendLine($"Match text：{item.SearchText}");
        builder.AppendLine($"Context：{item.DisplayText}");
        builder.AppendLine("------");
    }
    await new MessageDialog(builder.ToString()).ShowAsync();
}
```

*It should be noted that if it is the `Custom` mode, only the chapter name can be matched, but the content cannot be matched. Because in the design, `Custom` mode is used to load uncertain chapters (such as online reading), because the content of the chapter is unknown, it cannot be matched*

If you need to jump to a specified location based on a certain query result, you can call `LoadSearchItem`:

```csharp
Reader.LoadSearchItem(someItem);
```

## How to operate Image in Epub

When rendering the current chapter of the Epub book, I attached a `Tapped` event to each **Image**, you can use this to implement some "click on the picture to enlarge" or "click on the picture to save" operation.

1. Add `ImageTapped` event

```xml
<reader:ReaderPanel x:Name="Reader"
                    ImageTapped="Reader_ImageTapped">
</reader:ReaderPanel>
```

2. Convert the Base64 in the event parameters to `BitmapImage` or the stream you need

```csharp
private async void Reader_ImageTapped(object sender, ImageEventArgs e)
{
    var byteArray = Convert.FromBase64String(e.Base64);
    var stream = byteArray.AsBuffer().AsStream().AsRandomAccessStream();
    using (stream)
    {
        var bitmap = new BitmapImage();
        await bitmap.SetSourceAsync(stream);
        // do other things...
    }
}
```

## How to handle `LinkTapped` events

In Epub, links are mainly divided into two categories, one is internal links (such as navigating to a file or jumping to a certain position in the page), and the other is external links (pointing to a certain URL).

In the parameters of the `LinkTapped` event, **FileName** refers to the corresponding file name (such as "xxx.html"), and **Id** is a position on this page (such as "position1"), you can target To find. The content in **Link** is an external link.

There are several situations here:

1. Only **Id**. This means it is the positioning in the current chapter.
2. Only **FileName**. This indicates that you need to jump to a chapter.
3. Only **Link**. This indicates that the link is an external link, you can use `Launcher.LaunchUriAsync()` to open it in the default browser.
4. There are **FileName** and **Id** at the same time, which means that it will be located to a specific position in a chapter. Currently the control does not provide a method to handle this situation.

```csharp
private async void Reader_LinkTapped(object sender, LinkEventArgs e)
{
    if (!string.IsNullOrEmpty(e.Link))
        await Launcher.LaunchUriAsync(new Uri(e.Link));
    else
    {
        if (!string.IsNullOrEmpty(e.Id))
        {
            var tip = Reader.GetSpecificIdContent(e.Id, e.FileName);
            await new MessageDialog(tip.Description, tip.Title).ShowAsync();
        }
        else if (!string.IsNullOrEmpty(e.FileName))
            Reader.LocateToSpecificFile(e.FileName);
    }
}
```

## Speech synthesis

The control supports the use of SpeechSynthesis API provided by UWP to read the current chapter.

```csharp
private async void SpeechButton_Click(object sender, RoutedEventArgs e)
{
    try
    {
        var source = await Reader.GetChapterVoiceAsync(Reader.CurrentChapter, false, new SpeechSynthesizer());
        var player = new MediaPlayer();
        player.Source = source;
        player.Play();
    }
    catch (Exception ex)
    {
        await new MessageDialog(ex.Message).ShowAsync();
    }
}
```

***If you need to change the speaker’s gender, intonation, or speaking speed, please configure `SpeechSynthesizer` by yourself.***

If you need to make the reader adjust the page according to the current reading progress, you need to handle the `ReaderPanel.SpeechCueChanged` event.

```csharp
private void Reader_SpeechCueChanged(object sender, SpeechCueEventArgs e)
{
    if (e.Type == SpeechCueType.Word)
    {
        Reader.CheckCurrentReaderIndex(e.SpeechCue.StartPositionInInput);
    }
    else
    {
        // This is the sentence currently being read
        SpeechBlock.Text = e.SpeechCue.Text;
    }
}
```

## Known issues

1. Currently does not support more e-book formats
2. Text annotation is not supported temporarily

## Thanks

- [ReaderView](https://github.com/cnbluefire/ReaderView)
- [EpubSharp](https://github.com/Asido/EpubSharp/)
