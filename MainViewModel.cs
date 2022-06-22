using Imagin.Core;
using Imagin.Core.Collections.ObjectModel;
using Imagin.Core.Colors;
using Imagin.Core.Controls;
using Imagin.Core.Input;
using Imagin.Core.Media;
using Imagin.Core.Models;
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

        //...

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
            Panels = (element as ColorControl).Panels;
            Panels.Add(new LogPanel(Get.Current<App>().Log));
            Panels.Add(new NotificationsPanel(Get.Current<App>().Notifications));
            Panels.Add(new ThemePanel());
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
        => colorCommand ??= new RelayCommand<System.Windows.Media.Color>(i => (ActiveDocument as ColorDocument).Color.ActualColor = i, i => (ActiveDocument as ColorDocument)?.Color != null);

    ICommand newCommand;
    public ICommand NewCommand 
        => newCommand ??= new RelayCommand<Type>(i => Documents.Add(new ColorDocument(Colors.White, i ?? Get.Current<Options>().DefaultColorModel?.Value ?? typeof(LCHabh), Get.Current<Options>().ColorControlOptions)));

    ICommand deleteProfileCommand;
    public ICommand DeleteProfileCommand 
        => deleteProfileCommand ??= new RelayCommand<Namable<WorkingProfile>>(i => Get.Current<Options>().ColorControlOptions.Profiles.Remove(i), i => i != null);

    ICommand openProfileCommand;
    public ICommand OpenProfileCommand => openProfileCommand ??= new RelayCommand<WorkingProfile>(i =>
    {
        ActiveDocument.Color.Chromacity = i.Chromacity;
        ActiveDocument.Color.Compress = i.Compress;
        ActiveDocument.Color.Primary = i.Primary;
    },
    i => ActiveDocument != null);
    
    ICommand newColorCommand;
    public ICommand NewColorCommand => newColorCommand ??= new RelayCommand(() =>
    {
        var colors = MemberWindow.ShowDialog("New colors", new GroupCollection<StringColor>("Untitled colors"), out int result, i =>
        {
            i.GroupName = MemberGroupName.None; i.HeaderVisibility = Visibility.Collapsed;
        },
        Buttons.SaveCancel);
        if (result == 0)
            Get.Current<Options>().ColorControlOptions.Colors.Add(colors);
    },
    () => true);

    ICommand newProfileCommand;
    public ICommand NewProfileCommand => newProfileCommand ??= new RelayCommand(() =>
    {
        var profile = MemberWindow.ShowDialog("New profile", new NamableProfile("Untitled profile", WorkingProfile.Default), out int result, i =>
        {
            i.GroupName = MemberGroupName.None; i.HeaderVisibility = Visibility.Collapsed; i.NameColumnVisibility = Visibility.Collapsed;
        },
        Buttons.SaveCancel);
        if (result == 0)
            Get.Current<Options>().ColorControlOptions.Profiles.Add(profile);
    },
    () => ActiveDocument != null);

    #endregion
}