using Imagin.Core;
using Imagin.Core.Collections.Serialization;
using Imagin.Core.Controls;
using Imagin.Core.Models;
using System;

namespace Imagin.Apps.Color;

[Serializable]
public class Options : MainViewOptions, IColorControlOptions
{
    enum Category { Component, Window }

    #region Properties

    #region Other

    [Hidden]
    IGroupWriter IColorControlOptions.Colors => ColorControlOptions.Colors;

    [Hidden]
    IGroupWriter IColorControlOptions.Illuminants => ColorControlOptions.Illuminants;

    [Hidden]
    IGroupWriter IColorControlOptions.Profiles => ColorControlOptions.Profiles;

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

    #endregion

    #endregion

    #region Methods

    protected override void OnSaving()
    {
        base.OnSaving();
        ColorControlOptions.Save();
    }

    public void OnLoaded(ColorControl colorPicker) => ColorControlOptions?.OnLoaded(colorPicker);

    #endregion
}