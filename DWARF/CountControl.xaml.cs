using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DWARF
{
    /// <summary>
    /// Interaction logic for CountControl.xaml
    /// </summary>
    public partial class CountControl : UserControl
    {
        public CountControl()
        {
            InitializeComponent();
            InitializeBitmapGeneration();
            DisplayCount = 40;
        }

        public static readonly DependencyProperty DisplayCountProperty = DependencyProperty.Register(
            "DisplayCount",
            typeof(int),
            typeof(CountControl),
            new UIPropertyMetadata(0,
                (d, e) => ((CountControl)d)._OnDisplayChanged()));

        public static readonly DependencyProperty HasIssueProperty = DependencyProperty.Register(
         "HasIssue",
         typeof(bool),
         typeof(CountControl),
         new UIPropertyMetadata(false,
             (d, e) => ((CountControl)d)._OnDisplayChanged()));

        public static readonly DependencyProperty IsCheckingProperty = DependencyProperty.Register(
      "IsChecking",
      typeof(bool),
      typeof(CountControl),
      new UIPropertyMetadata(false,
          (d, e) => ((CountControl)d)._OnDisplayChanged()));

        public static readonly DependencyProperty HasVolumeProperty = DependencyProperty.Register(
      "HasVolume",
      typeof(bool),
      typeof(CountControl),
      new UIPropertyMetadata(false,
          (d, e) => ((CountControl)d)._OnDisplayChanged()));

        public int DisplayCount
        {
            get { return (int)GetValue(DisplayCountProperty); }
            set { SetValue(DisplayCountProperty, value); }
        }

        public bool HasIssue
        {
            get { return (bool)GetValue(HasIssueProperty); }
            set { SetValue(HasIssueProperty, value); }
        }

        public bool HasVolume
        {
            get { return (bool)GetValue(HasVolumeProperty); }
            set { SetValue(HasVolumeProperty, value); }
        }

        public bool IsChecking
        {
            get { return (bool)GetValue(IsCheckingProperty); }
            set { SetValue(IsCheckingProperty, value); }
        }

        private void _OnDisplayChanged()
        {
            ImageSource = null;
            if (IsChecking)
            {
                ColourBorder.Background = (Brush)Resources["CheckingBrush"];
                TextBlock.Foreground = Brushes.Black;
                return;
            }

            if (HasIssue)
            {
                ColourBorder.Background = (Brush)Resources["IssueBrush"];
                TextBlock.Foreground = Brushes.White;
                return;
            }

            if (HasVolume == false)
            {
                ColourBorder.Background = (Brush)Resources["MutedBrush"];
                TextBlock.Foreground = Brushes.Black;
                return;
            }

            if (DisplayCount == 0)
            {
                ColourBorder.Background = (Brush)Resources["EmptyBrush"];
                TextBlock.Foreground = Brushes.Black;
            }
            else
            {
                ColourBorder.Background = (Brush)Resources["HighlightBrush"];
                TextBlock.Foreground = Brushes.White;
            }


        }

        #region Should be in base class

        protected void InitializeBitmapGeneration()
        {
            LayoutUpdated += (sender, e) => _UpdateImageSource();
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
           "ImageSource",
           typeof(ImageSource),
           typeof(CountControl),
           new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ImageSource property.  This dependency property 
        /// indicates ....
        /// </summary>
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        private void _UpdateImageSource()
        {
            if (ActualWidth == 0 || ActualHeight == 0)
            {
                return;
            }
            ImageSource = GenerateBitmapSource(this, 16, 16);
        }

        public static BitmapSource GenerateBitmapSource(ImageSource img)
        {
            return GenerateBitmapSource(img, img.Width, img.Height);
        }

        public static BitmapSource GenerateBitmapSource(ImageSource img, double renderWidth, double renderHeight)
        {
            var dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawImage(img, new Rect(0, 0, renderWidth, renderHeight));
            }
            var bmp = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            return bmp;
        }

        public static BitmapSource GenerateBitmapSource(Visual visual, double renderWidth, double renderHeight)
        {
            var bmp = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawRectangle(new VisualBrush(visual), null, new Rect(0, 0, renderWidth, renderHeight));
            }
            bmp.Render(dv);
            return bmp;
        }
        #endregion

    }
}
