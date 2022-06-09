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
        [Hidden]
        public bool AutoSaveLayout => ColorControlOptions.AutoSaveLayout;

        [Hidden]
        IGroupWriter IColorControlOptions.Colors => ColorControlOptions.Colors;

        [Hidden]
        ObservableCollection<Namable<WorkingProfile>> IColorControlOptions.Profiles => ColorControlOptions.Profiles;

        ColorControlOptions colorControlOptions = new();
        [Category(nameof(ColorControl))]
        [DisplayName("Options")]
        public ColorControlOptions ColorControlOptions
        {
            get => colorControlOptions;
            set => this.Change(ref colorControlOptions, value);
        }

        [NonSerialized]
        Type defaultColorSpace = typeof(LCHabh);
        [Hidden]
        public Type DefaultColorModel
        {
            get => defaultColorSpace ??= typeof(LCHabh);
            set => this.Change(ref defaultColorSpace, value);
        }

        DocumentCollection documents = new();
        [Hidden]
        public DocumentCollection Documents
        {
            get => documents;
            set => this.Change(ref documents, value);
        }

        [Hidden]
        public Layouts Layouts => ColorControlOptions.Layouts;

        //...

        protected override void OnSaving()
        {
            base.OnSaving();
            ColorControlOptions.OnSaved();
        }

        public void OnLoaded(ColorControl colorPicker) => ColorControlOptions?.OnLoaded(colorPicker);
    }
}