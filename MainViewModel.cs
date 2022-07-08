using Imagin.Core;
using Imagin.Core.Colors;
using Imagin.Core.Controls;
using Imagin.Core.Input;
using Imagin.Core.Models;
using Imagin.Core.Numerics;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Imagin.Apps.Color;

public class MainViewModel : MainViewModel<MainWindow>, IFrameworkReference
{
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

    ListCollectionView models = ColorControl.GetModels();
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
        Documents.CollectionChanged += OnDocumentsChanged;

        New<RCA>(); //New<RGB>(); //New<RGV>(); //New<RYB>();
        //New<CMY>();
        //New<HSM>(); //New<HSB>(); //New<HSL>(); //New<HSP>();
        //New<HPLuv>(); //New<HSLuv>();
        //New<Lab>(); //New<Labh>(); //New<Labi>(); //New<Labj>(); 
        //New<LCHxy>(); //New<LCHab>(); //New<LCHabh>(); //New<LCHabj>();
        //New<Labk>(); //New<HSBk>(); //New<HSLk>(); //New<HWBk>();
        //New<CMYK>(); //New<CMYW>(); //New<RGBK>(); //New<RGBW>();
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

    ICommand closeCommand;
    public ICommand CloseCommand 
        => closeCommand ??= new RelayCommand(() => Documents.Remove(ActiveDocument), () => ActiveDocument != null);

    ICommand closeAllCommand;
    public ICommand CloseAllCommand 
        => closeAllCommand ??= new RelayCommand(() => Documents.Clear(), () => Documents.Count > 0);

    ICommand colorCommand;
    public ICommand ColorCommand 
        => colorCommand ??= new RelayCommand<System.Windows.Media.Color>(i => ActiveDocument.Color.ActualColor = i, i => ActiveDocument?.Color != null);

    ICommand newCommand;
    public ICommand NewCommand 
        => newCommand ??= new RelayCommand<Type>(i => Documents.Add(new ColorDocument(Colors.White, i ?? Get.Current<Options>().DefaultColorModel?.Value, Get.Current<Options>().ColorControlOptions.Profiles)));

    ICommand openIlluminantCommand;
    public ICommand OpenIlluminantCommand
        => openIlluminantCommand ??= new RelayCommand<Vector2>(i => ActiveDocument.Color.Profile = new WorkingProfile(ActiveDocument.Color.Profile.Primary, i, ActiveDocument.Color.Profile.Compress, ActiveDocument.Color.Profile.Adapt, ActiveDocument.Color.Profile.ViewingConditions), i => ActiveDocument != null);

    ICommand openProfileCommand;
    public ICommand OpenProfileCommand
        => openProfileCommand ??= new RelayCommand<WorkingProfile>(i => ActiveDocument.Color.Profile = i, i => ActiveDocument != null);

    #endregion
}