using Imagin.Core;
using Imagin.Core.Colors;
using Imagin.Core.Controls;
using Imagin.Core.Input;
using Imagin.Core.Linq;
using Imagin.Core.Models;
using Imagin.Core.Serialization;
using Imagin.Core.Storage;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Imagin.Apps.Color;

public class MainViewModel : MainViewModel<MainWindow>, IFrameworkReference
{
    public static readonly Type[] DefaultModels 
        = new[] { typeof(HSB) /*typeof(RCA), typeof(RGB), typeof(RGV)*/ };

    public const string FileExtension = "color";

    #region Properties

    public static readonly ReferenceKey<ColorControl> ColorControlReferenceKey = new();

    ColorDocument activeDocument = null;
    public ColorDocument ActiveDocument
    {
        get => activeDocument;
        set => this.Change(ref activeDocument, value);
    }

    ColorControl colorControl = null;
    public ColorControl ColorControl
    {
        get => colorControl;
        private set => this.Change(ref colorControl, value);
    }

    public DocumentCollection Documents => Get.Current<Options>().Documents;

    ListCollectionView models = ColorDocument.GetDefaultModels();
    public ListCollectionView Models
    {
        get => models;
        private set => models = value;
    }

    PanelCollection panels = null;
    public PanelCollection Panels
    {
        get => panels;
        private set => this.Change(ref panels, value);
    }

    #endregion

    #region MainViewModel

    public MainViewModel() : base()
    {
        Documents.CollectionChanged += OnDocumentsChanged; DefaultModels.ForEach(i => New(i));
    }

    #endregion

    #region Methods

    void OnDocumentsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                ((Document)e.NewItems[0]).Subscribe();
                break;

            case NotifyCollectionChangedAction.Remove:
                ((Document)e.OldItems[0]).Unsubscribe();
                break;
        }
    }

    void New<T>() where T : ColorModel => New(typeof(T));

    void New(Type type) => NewCommand.Execute(type);

    void Save(string folderPath)
    {
        if (StorageWindow.Show(out string path, "Save", StorageWindowModes.SaveFile, new[] { FileExtension }, folderPath))
        {
            if (BinarySerializer.Serialize(path, ActiveDocument))
                ActiveDocument.Path = path;
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

    void IFrameworkReference.SetReference(IFrameworkKey key, FrameworkElement element)
    {
        if (key == ColorControlReferenceKey)
        {
            ColorControl = element as ColorControl;
            
            Panels = ColorControl.Panels;
            Panels.Add(new LogPanel(Get.Current<App>().Log));
            Panels.Add(new NotificationsPanel(Get.Current<App>().Notifications));
        }
    }

    #endregion

    #region Commands

    ICommand cloneCommand;
    public ICommand CloneCommand
        => cloneCommand ??= new RelayCommand(() => Documents.Add(ActiveDocument.Clone()), () => ActiveDocument != null);

    ICommand closeCommand;
    public ICommand CloseCommand 
        => closeCommand ??= new RelayCommand(() => Documents.Remove(ActiveDocument), () => ActiveDocument != null);

    ICommand closeAllCommand;
    public ICommand CloseAllCommand 
        => closeAllCommand ??= new RelayCommand(() => Documents.Clear(), () => Documents.Count > 0);

    ICommand colorCommand;
    public ICommand ColorCommand 
        => colorCommand ??= new RelayCommand<System.Windows.Media.Color>(i => ActiveDocument.NewColor = i, i => ActiveDocument != null);

    ICommand newCommand;
    public ICommand NewCommand
        => newCommand ??= new RelayCommand<Type>(i => Documents.Add(new ColorDocument(Colors.White, i ?? Get.Current<Options>().DefaultColorModel?.Value ?? ColorDocument.DefaultModel, Get.Current<Options>().ColorControlOptions.Profiles)));

    ICommand newFromCommand;
    public ICommand NewFromCommand => newFromCommand ??= new RelayCommand<string>(i =>
    {
        if (BinarySerializer.Deserialize(i, out ColorDocument result))
        {
            result.Path = null;
            Documents.Add(result);
        }
    }, 
    i => System.IO.File.Exists(i));

    ICommand openCommand;
    public ICommand OpenCommand => openCommand ??= new RelayCommand(() =>
    {
        if (StorageWindow.Show(out string[] paths, "Open", StorageWindowModes.OpenFile, new[] { FileExtension }, ActiveDocument?.Path))
        {
            if (paths?.Length > 0)
            {
                foreach (var i in paths)
                {
                    if (BinarySerializer.Deserialize(i, out ColorDocument j))
                        Documents.Add(j);
                }
            }
        }
    });

    ICommand saveCommand;
    public ICommand SaveCommand => saveCommand ??= new RelayCommand(() => Save(ActiveDocument), () => ActiveDocument != null);

    ICommand saveAllCommand;
    public ICommand SaveAllCommand => saveAllCommand ??= new RelayCommand(() => Documents.ForEach(i => Save(i)), () => Documents?.Count > 0);

    ICommand saveTemplateCommand;
    public ICommand SaveTemplateCommand => saveTemplateCommand ??= new RelayCommand(() =>
    {
        if (!System.IO.Directory.Exists(Options.TemplatesFolder))
        {
            if (!Try.Invoke(() => System.IO.Directory.CreateDirectory(Options.TemplatesFolder)))
                return;
        }

        Save(Options.TemplatesFolder);
    }, 
    () => ActiveDocument != null);

    #endregion
}