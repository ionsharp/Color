using Imagin.Core;
using Imagin.Core.Collections.ObjectModel;
using Imagin.Core.Config;
using Imagin.Core.Controls;
using Imagin.Core.Media;
using Imagin.Core.Models;
using Imagin.Core.Reflection;
using Imagin.Core.Storage;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Imagin.Apps.Color;

[Serializable, View(MemberView.Tab, typeof(Tab))]
public class Options : FileDockMainViewOptions
{
    enum Category { General, Models, ToolTip }

    enum Tab { Color }

    #region General

    [Category(Category.General), Editable, Horizontal, HideName, Tab(Tab.Color)]
    public ColorViewOptions ColorViewOptions { get => Get<ColorViewOptions>(null, false); set => Set(value, false); }

    #endregion

    #region Models

    [Hide]
    public NormalColorViewModel DefaultColorModel { get => Get<NormalColorViewModel>(null); set => Set(value); }

    [Category(Category.Models)]
    [Name("Group direction"), Tab(Tab.Color)]
    public System.ComponentModel.ListSortDirection ModelGroupDirection { get => Get(System.ComponentModel.ListSortDirection.Ascending); set => Set(value); }

    [Hide]
    public string ModelGroupName => ModelGroupNames.Count > ModelGroupNameIndex && ModelGroupNameIndex >= 0 ? ModelGroupNames[ModelGroupNameIndex] : null;

    [Category(Category.Models), Int32Style(Int32Style.Index, nameof(ModelGroupNames))]
    [Name("Group name"), Tab(Tab.Color)]
    public virtual int ModelGroupNameIndex { get => Get(0); set => Set(value); }

    [Hide]
    public StringCollection ModelGroupNames { get; private set; } = new() { nameof(NormalColorViewModel.Category), nameof(NormalColorViewModel.FirstLetter) };

    [Category(Category.Models)]
    [Name("Sort direction"), Tab(Tab.Color)]
    public System.ComponentModel.ListSortDirection ModelSortDirection { get => Get(System.ComponentModel.ListSortDirection.Ascending); set => Set(value); }

    [Hide]
    public string ModelSortName => ModelSortNames?.Count > ModelSortNameIndex && ModelSortNameIndex >= 0 ? (string)ModelSortNames[ModelSortNameIndex] : null;

    [Category(Category.Models), Int32Style(Int32Style.Index, nameof(ModelSortNames))]
    [Name("Sort name"), Tab(Tab.Color)]
    public virtual int ModelSortNameIndex { get => Get(1); set => Set(value); }

    [Hide]
    public StringCollection ModelSortNames { get; private set; } = new() { nameof(NormalColorViewModel.Category), nameof(NormalColorViewModel.Name) };

    #endregion

    #region Templates

    [Hide]
    public PathCollection Templates { get => Get<PathCollection>(null, false); set => Set(value, false); }

    #endregion

    #region ToolTip

    [Category(Category.ToolTip), Name("Models"), Tab(Tab.Color), StringStyle(StringStyle.Tokens)]
    public string ColorToolTip { get => Get("RGB;CMYK"); set => Set(value); }

    [Category(Category.ToolTip), Name("Models (Normalize)"), Tab(Tab.Color)]
    public bool ColorToolTipNormalize { get => Get(false); set => Set(value); }

    [Category(Category.ToolTip), Name("Models (Precision)"), Range(0, 6, 1, Style = RangeStyle.Both), Tab(Tab.Color)]
    public int ColorToolTipPrecision { get => Get(2); set => Set(value); }

    #endregion

    ///

    #region Methods

    protected override void OnLoaded()
    {
        var defaultOptions = new ColorViewOptions();
        defaultOptions.Load(out ColorViewOptions existingOptions);
        ColorViewOptions = existingOptions ?? defaultOptions;

        base.OnLoaded();

        Templates = new($@"{Current.Get<BaseApplication>().DataFolderPath}\Templates", new Filter(ItemType.File, "color"));
        Templates.Subscribe();
        _ = Templates.RefreshAsync();
    }

    ///

    public override void OnPropertyChanged(PropertyEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(ModelGroupNameIndex):
                Update(() => ModelGroupName);
                break;

            case nameof(ModelSortNameIndex):
                Update(() => ModelSortName);
                break;
        }
    }

    public override int GetDefaultLayout() => ColorViewOptions?.GetDefaultLayout() ?? 0;

    public override IEnumerable<Uri> GetDefaultLayouts() => ColorViewOptions?.GetDefaultLayouts();

    #endregion
}