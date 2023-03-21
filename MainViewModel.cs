using Imagin.Core;
using Imagin.Core.Analytics;
using Imagin.Core.Colors;
using Imagin.Core.Controls;
using Imagin.Core.Conversion;
using Imagin.Core.Input;
using Imagin.Core.Linq;
using Imagin.Core.Media;
using Imagin.Core.Models;
using Imagin.Core.Reflection;
using Imagin.Core.Serialization;
using Imagin.Core.Storage;
using System;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Imagin.Apps.Color;

[Menu(typeof(Menu))]
public class MainViewModel : DockMainViewModel<MainWindow, ColorDocument>
{
    enum Category { Custom, Default, Dock, Documents, Group, Layouts, Manage, New, Open, Panels, Save }

    public static readonly Type[] DefaultModels 
        = new[] { typeof(HSB) /*typeof(RCA), typeof(RGB), typeof(RGV)*/ };

    public static readonly ResourceKey DefaultModelTemplateKey = new();

    [Menu]
    enum Menu 
    {
        [MenuItem(Icon = SmallImages.Color)]
        Color,
    }

    #region Properties

    new public ColorViewOptions Options => (ColorViewOptions)base.Options;

    public Options _Options => Current.Get<Options>();

    new ColorFileDockViewModel ViewModel => (ColorFileDockViewModel)base.ViewModel;

    #endregion

    #region MainViewModel

    public MainViewModel() : base(Current.Get<Options>().ColorViewOptions)
    {
        NewCommand.Execute();
    }

    #endregion

    #region Menu

    #region Color

    ICommand newCommand;
    [MenuItem(Parent = Menu.Color, Category = Category.New,
        Header = "New",
        Icon = SmallImages.Plus,
        InputGestureTextSource = typeof(MainViewModel),
        InputGestureTextKey = nameof(DefaultModelTemplateKey))]
    public ICommand NewCommand => newCommand ??= new RelayCommand<Type>(i =>
    {
        var result = new ColorDocument(Colors.White, i ?? Current.Get<Options>().DefaultColorModel?.Value ?? ColorDocument.DefaultModel, Options.Profiles);
        result.Depth
            = Options.DefaultDepth;
        result.Dimension
            = Options.DefaultDimension;

        ViewModel.Documents.Add(result);
    });

    ICommand newFromCurrentCommand;
    [MenuItem(Parent = Menu.Color, Category = Category.New,
        Header = "NewFromCurrent",
        Icon = SmallImages.PlusCurrent)]
    public ICommand NewFromCurrentCommand
        => newFromCurrentCommand ??= new RelayCommand(() => ViewModel.Documents.Add(ViewModel.ActiveDocument.DeepClone(new CloneHandler())), () => ViewModel.ActiveDocument != null);

    ICommand newFromTemplateCommand;
    public ICommand NewFromTemplateCommand => newFromTemplateCommand ??= new RelayCommand<string>(i =>
    {
        var result = BinarySerializer.Deserialize(i, out ColorDocument document);
        if (result is Error error)
        {
            Dialog.ShowError("New from template", error, Buttons.Ok);
        }
        else
        {
            document.Path = null;
            ViewModel.Documents.Add(document);
        }
    },
    i => System.IO.File.Exists(i));

    [MenuItemCollection(Parent = Menu.Color, Category = Category.New,
        Header = "NewFromTemplate",

        Icon = SmallImages.PlusTemplate,

        IsInline = false,

        ItemCommandName = nameof(NewFromTemplateCommand),
        ItemCommandParameterPath = ".",

        ItemHeaderConverter = typeof(FileNameConverter),
        ItemHeaderPath = ".",

        ItemIconPath = ".",
        ItemIconTemplateSource = typeof(XExplorer),
        ItemIconTemplateKey = nameof(XExplorer.IconTemplateKey),

        ItemToolTipPath = ".",
        ItemToolTipTemplateSource = typeof(XExplorer),
        ItemToolTipTemplateKey = nameof(XExplorer.ToolTipTemplateKey),

        ItemType = typeof(string))]
    public PathCollection Templates => Current.Get<Options>().Templates;

    ICommand openCommand;
    [MenuItem(Parent = Menu.Color, Category = Category.Open,
        Header = "Open",
        Icon = SmallImages.Open,
        InputGestureText = "Ctrl + O")]
    public ICommand OpenCommand => openCommand ??= new RelayCommand(() => _ = ViewModel.Open());

    ICommand openRecentCommand;
    public ICommand OpenRecentCommand => openRecentCommand ??= new RelayCommand<string>(i => _ = ViewModel.Open(i), System.IO.File.Exists);

    [MenuItemCollection(Parent = Menu.Color, Category = Category.Open,
        Header = "OpenRecent",

        Icon = SmallImages.OpenRecent,

        IsInline = false,

        ItemCommandName = nameof(OpenRecentCommand),
        ItemCommandParameterPath = ".",

        ItemHeaderConverter = typeof(FileNameConverter),
        ItemHeaderPath = ".",

        ItemIconPath = ".",
        ItemIconTemplateSource = typeof(XExplorer),
        ItemIconTemplateKey = nameof(XExplorer.IconTemplateKey),

        ItemToolTipPath = ".",
        ItemToolTipTemplateSource = typeof(XExplorer),
        ItemToolTipTemplateKey = nameof(XExplorer.ToolTipTemplateKey),

        ItemType = typeof(string))]
    public object RecentDocuments => Current.Get<Options>().RecentFiles;

    ICommand saveCommand;
    [MenuItem(Parent = Menu.Color, Category = Category.Save,
        Header = "Save",
        Icon = SmallImages.Save,
        InputGestureText = "Ctrl + S")]
    public ICommand SaveCommand => saveCommand ??= new RelayCommand(() => Save(ViewModel.ActiveDocument), 
        () => ViewModel.ActiveDocument != null);

    ICommand saveAsTemplateCommand;
    [MenuItem(Parent = Menu.Color, Category = Category.Save,
        Header = "SaveAs",
        Icon = SmallImages.SaveAs,
        InputGestureText = "Ctrl + Shift + S")]
    public ICommand SaveAsTemplateCommand => saveAsTemplateCommand ??= new RelayCommand(() =>
    {
        if (!System.IO.Directory.Exists(Current.Get<Options>().Templates.Path))
        {
            if (!Try.Invoke(() => System.IO.Directory.CreateDirectory(Current.Get<Options>().Templates.Path), e => Core.Analytics.Log.Write<MainViewModel>(e)))
                return;
        }

        Save(Current.Get<Options>().Templates.Path);
    },
        () => ViewModel.ActiveDocument != null);

    [MenuItem(Parent = Menu.Color, Category = Category.Save,
        Header = "SaveAll",
        Icon = SmallImages.SaveAll)]
    public override ICommand SaveAllCommand => base.SaveAllCommand;

    #endregion

    #region Model

    ListCollectionView models = ColorDocument.GetDefaultModels();
    //GroupDirectionPath = nameof(Imagin.Apps.Color.Options.ModelGroupDirection),
    //GroupNamePath = nameof(Imagin.Apps.Color.Options.ModelGroupName),
    //GroupSource = nameof(_Options),

    //SortDirectionPath = nameof(Imagin.Apps.Color.Options.ModelSortDirection),
    //SortNamePath = nameof(Imagin.Apps.Color.Options.ModelSortName),
    //SortSource = nameof(Color.Options)
    [MenuItemCollection(Header = "Models", Icon = SmallImages.XYZ,
        IsInline = false,

        ItemCommandName = nameof(NewCommand),
        ItemCommandParameterPath = nameof(NormalColorViewModel.Value),
        ItemIcon = SmallImages.SmallPeriod,
        ItemHeaderPath = $"{nameof(NormalColorViewModel.Value)}.{nameof(Type.Name)}",
        ItemToolTipPath = nameof(NormalColorViewModel.Value),
        ItemToolTipTemplateKey = nameof(XColor.ModelToolTipTemplateKey),
        ItemToolTipTemplateSource = typeof(XColor),
        ItemType = typeof(NormalColorViewModel))]
    public ListCollectionView Models { get => models; private set => models = value; }

    #endregion

    #endregion

    #region Methods

    void New(Type type) => NewCommand.Execute(type);

    void Save(string folderPath)
    {
        if (StorageDialog.Show(out string path, "Save", StorageDialogMode.SaveFile, ViewModel.ReadableFileExtensions, folderPath))
        {
            if (BinarySerializer.Serialize(path, ViewModel.ActiveDocument))
                ViewModel.ActiveDocument.As<ColorDocument>().Path = path;
        }
    }

    void Save(Document input)
    {
        if (input is ColorDocument document)
        {
            if (!System.IO.File.Exists(document.Path))
            {
                Save(document.Path);
                return;
            }
            BinarySerializer.Serialize(document.Path, document);
        }
    }

    protected override DockViewModel GetModel(IDockViewOptions options) => new ColorFileDockViewModel((IFileDockViewOptions)options);

    #endregion

    #region Commands

    ICommand colorCommand;
    public ICommand ColorCommand 
        => colorCommand ??= new RelayCommand<System.Windows.Media.Color>(i => ViewModel.ActiveDocument.As<ColorDocument>().NewColor = i, i => ViewModel.ActiveDocument != null);

    #endregion
}