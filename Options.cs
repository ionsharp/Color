using Imagin.Core;
using Imagin.Core.Collections.Serialization;
using Imagin.Core.Colors;
using Imagin.Core.Controls;
using Imagin.Core.Models;
using System;
using System.Collections.ObjectModel;

namespace Imagin.Apps.Color
{
    [Serializable]
    public class Options : MainViewOptions, IColorControlOptions
    {
        enum Category { Component, Layouts, Window }

        [Category(Category.Layouts)]
        [DisplayName("Auto save")]
        public bool AutoSaveLayout => ColorControlOptions.AutoSaveLayout;

        [Hidden]
        IGroupWriter IColorControlOptions.Colors => ColorControlOptions.Colors;

        [Hidden]
        IWriter IColorControlOptions.Profiles => ColorControlOptions.Profiles;

        ColorControlOptions colorControlOptions = new();
        [Hidden]
        public ColorControlOptions ColorControlOptions
        {
            get => colorControlOptions;
            set => this.Change(ref colorControlOptions, value);
        }

        [Category(Category.Component)]
        [DisplayName("Normalize")]
        public bool ComponentNormalize
        {
            get => ColorControlOptions.ComponentNormalize;
            set
            {
                ColorControlOptions.ComponentNormalize = value;
                this.Changed(() => ComponentNormalize);
            }
        }

        [Category(Category.Component)]
        [DisplayName("Precision")]
        public int ComponentPrecision
        {
            get => ColorControlOptions.ComponentPrecision;
            set
            {
                ColorControlOptions.ComponentPrecision = value;
                this.Changed(() => ComponentPrecision);
            }
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

        [Category(Category.Layouts)]
        [DisplayName("Layout")]
        public Layouts Layouts => ColorControlOptions.Layouts;

        [Category(Category.Window)]
        public PanelCollection Panels => ColorControlOptions.Panels;

        //...

        protected override void OnSaving()
        {
            base.OnSaving();
            ColorControlOptions.OnSaved();
        }

        public void OnLoaded(ColorControl colorPicker) => ColorControlOptions?.OnLoaded(colorPicker);
    }
}