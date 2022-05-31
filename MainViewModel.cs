using Imagin.Core;
using Imagin.Core.Analytics;
using Imagin.Core.Colors;
using Imagin.Core.Controls;
using Imagin.Core.Input;
using Imagin.Core.Models;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Imagin.Apps.Color;

public class MainViewModel : MainViewModel<MainWindow>, IFrameworkReference
{
    public static readonly ReferenceKey<ColorControl> ColorControlReferenceKey = new();

    Document activeDocument = null;
    public Document ActiveDocument
    {
        get => activeDocument;
        set => this.Change(ref activeDocument, value);
    }

    public DocumentCollection Documents => Get.Current<Options>().Documents;

    PanelCollection panels = null;
    public PanelCollection Panels
    {
        get => panels;
        private set => this.Change(ref panels, value);
    }

    //...

    public MainViewModel() : base() 
    {
        var w = WorkingProfile.Default.sRGB;

        foreach (var i in ColorVector.Type)
            ColorVector.LogAccuracy(i.Value, w);
        /*
        */
        /*
        foreach (var i in ColorVector.Types)
            ColorVector.LogRange(i.Value, w, false);
        */

        //New<LAB>(); New<LABh>();
        New<HUVuv>();
        //New<HUVab>(); New<HUVabh>(); New<HzUzVz>();
        //New<HPLuv>(); New<HSLuv>();
        //New<OKLab>();
        //New<JzAzBz>(); New<JzCzHz>();
    }

    //...

    void New<T>() where T : ColorVector3 => NewCommand.Execute(typeof(T));

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

    ICommand closeCommand;
    public ICommand CloseCommand => closeCommand ??= new RelayCommand(() => Documents.Remove(ActiveDocument), () => ActiveDocument != null);

    ICommand closeAllCommand;
    public ICommand CloseAllCommand => closeAllCommand ??= new RelayCommand(() => Documents.Clear(), () => Documents.Count > 0);

    ICommand colorCommand;
    public ICommand ColorCommand => colorCommand ??= new RelayCommand<System.Windows.Media.Color>(i => (ActiveDocument as ColorDocument).Color.ActualColor = i, i => (ActiveDocument as ColorDocument)?.Color != null);

    ICommand newCommand;
    public ICommand NewCommand => newCommand ??= new RelayCommand<System.Type>(i => Documents.Add(new ColorDocument(Colors.White, i)));
}