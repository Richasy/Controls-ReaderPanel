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
            if (Path.GetExtension(file.Path) == ".epub")
            {
                await Reader.OpenAsync(file, new EpubViewStyle());
            }
            else
            {
                await Reader.OpenAsync(file, new TxtViewStyle());
            }
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

## Known issues

1. Currently does not support more e-book formats
2. Text annotation is not supported temporarily
3. Voice reading is not supported temporarily

## Thanks

- [ReaderView](https://github.com/cnbluefire/ReaderView)
- [EpubSharp](https://github.com/Asido/EpubSharp/)
