using Imagin.Core;
using Imagin.Core.Collections.ObjectModel;
using Imagin.Core.Collections.Serialization;
using Imagin.Core.Config;
using Imagin.Core.Controls;
using Imagin.Core.Models;
using Imagin.Core.Reflection;
using Imagin.Core.Storage;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Imagin.Apps.Color;

[Serializable]
public class Options : MainViewOptions, IColorControlOptions
{
    enum Category { Documents, Models, ToolTip, Window }

    #region Properties

    #region Other

    [Hidden]
    IGroupWriter IColorControlOptions.Colors 
        => ColorControlOptions.Colors;

    [Hidden]
    IGroupWriter IColorControlOptions.Illuminants 
        => ColorControlOptions.Illuminants;

    [Hidden]
    IGroupWriter IColorControlOptions.Matrices 
        => ColorControlOptions.Matrices;

    [Hidden]
    IGroupWriter IColorControlOptions.Profiles
        => ColorControlOptions.Profiles;

    ColorControlOptions colorControlOptions = new();
    [Hidden]
    public ColorControlOptions ColorControlOptions
    {
        get => colorControlOptions;
        set => this.Change(ref colorControlOptions, value);
    }

    [NonSerialized]
    NamableCategory<Type> defaultColorModel = null;
    [Hidden]
    public NamableCategory<Type> DefaultColorModel
    {
        get => defaultColorModel;
        set => this.Change(ref defaultColorModel, value);
    }

    DocumentCollection documents = new();
    [Hidden]
    public DocumentCollection Documents
    {
        get => documents;
        set => this.Change(ref documents, value);
    }

    [NonSerialized]
    PathCollection templates = null;
    [Hidden]
    public PathCollection Templates
    {
        get => templates;
        set => this.Change(ref templates, value);
    }
    
    #endregion

    #region Color

    string colorToolTip = "RGB;CMYK";
    [Category(Category.ToolTip), DisplayName("Models"), Tokens]
    public string ColorToolTip
    {
        get => colorToolTip;
        set => this.Change(ref colorToolTip, value);
    }

    bool colorToolTipNormalize = false;
    [Category(Category.ToolTip), DisplayName("Models (Normalize)")]
    public bool ColorToolTipNormalize
    {
        get => colorToolTipNormalize;
        set => this.Change(ref colorToolTipNormalize, value);
    }

    int colorToolTipPrecision = 2;
    [Category(Category.ToolTip), DisplayName("Models (Precision)"), Range(0, 6, 1), SliderUpDown]
    public int ColorToolTipPrecision
    {
        get => colorToolTipPrecision;
        set => this.Change(ref colorToolTipPrecision, value);
    }

    #endregion

    #region Documents

    [Category(Category.Documents)]
    [DisplayName("Auto save")]
    public bool AutoSaveDocuments => ColorControlOptions.AutoSaveDocuments;

    [Hidden]
    bool IColorControlOptions.RememberDocuments => true;

    [Hidden]
    IList IColorControlOptions.Documents => Documents;

    #endregion

    #region Models

    System.ComponentModel.ListSortDirection modelGroupDirection = System.ComponentModel.ListSortDirection.Ascending;
    [Category(Category.Models)]
    [DisplayName("Group direction")]
    public System.ComponentModel.ListSortDirection ModelGroupDirection
    {
        get => modelGroupDirection;
        set => this.Change(ref modelGroupDirection, value);
    }

    [Hidden]
    public string ModelGroupName => ModelGroupNames.Count > ModelGroupNameIndex && ModelGroupNameIndex >= 0 ? ModelGroupNames[ModelGroupNameIndex] : null;

    int modelGroupNameIndex = 0;
    [Category(Category.Models), SelectedIndex]
    [DisplayName("Group name")]
    [Trigger(nameof(MemberModel.ItemSource), nameof(ModelGroupNames))]
    public virtual int ModelGroupNameIndex
    {
        get => modelGroupNameIndex;
        set => this.Change(ref modelGroupNameIndex, value);
    }

    [Hidden]
    public StringCollection ModelGroupNames { get; private set; } = new() { "Name", "Category" };

    System.ComponentModel.ListSortDirection modelSortDirection = System.ComponentModel.ListSortDirection.Ascending;
    [Category(Category.Models)]
    [DisplayName("Sort direction")]
    public System.ComponentModel.ListSortDirection ModelSortDirection
    {
        get => modelSortDirection;
        set => this.Change(ref modelSortDirection, value);
    }

    [Hidden]
    public string ModelSortName => ModelSortNames?.Count > ModelSortNameIndex && ModelSortNameIndex >= 0 ? (string)ModelSortNames[ModelSortNameIndex] : null;

    int modelSortNameIndex = 0;
    [Category(Category.Models), SelectedIndex]
    [DisplayName("Sort name")]
    [Trigger(nameof(MemberModel.ItemSource), nameof(ModelSortNames))]
    public virtual int ModelSortNameIndex
    {
        get => modelSortNameIndex;
        set => this.Change(ref modelSortNameIndex, value);
    }

    [Hidden]
    public StringCollection ModelSortNames { get; private set; } = new() { "Name" };

    #endregion

    #region Window

    [Category(Category.Window)]
    [DisplayName("Auto save")]
    public bool AutoSaveLayout => ColorControlOptions.AutoSaveLayout;

    [Category(Category.Window)]
    [DisplayName("Layout")]
    public Layouts Layouts => ColorControlOptions.Layouts;

    [Category(Category.Window)]
    public PanelCollection Panels => ColorControlOptions.Panels;

    [Hidden]
    IList IColorControlOptions.Panels => Panels;

    #endregion

    #endregion

    #region Methods

    protected override void OnSaving()
    {
        base.OnSaving();
        ColorControlOptions.Save();
    }

    public static readonly string TemplatesFolder = $@"{ApplicationProperties.GetFolderPath(DataFolders.Documents)}\{nameof(Color)}\Templates";

    public void OnLoaded(ColorControl colorPicker)
    {
        Templates = new(TemplatesFolder, new Filter(ItemType.File, MainViewModel.FileExtension));
        ColorControlOptions?.OnLoaded(colorPicker);
    }

    public override void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        base.OnPropertyChanged(propertyName);
        switch (propertyName)
        {
            case nameof(ModelGroupNameIndex):
                this.Changed(() => ModelGroupName);
                break;

            case nameof(ModelSortNameIndex):
                this.Changed(() => ModelSortName);
                break;
        }
    }

    #endregion
}