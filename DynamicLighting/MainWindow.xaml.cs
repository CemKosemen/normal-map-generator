using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace DynamicLighting
{
    public partial class MainWindow
    {
        private PixelMap _imgMap;
        private PixelMap _normalMap;
        private PixelMap _lightedImgMap;
        private Vector3D _l; // light vector
        private Vector3D _v; // eye vector

        public MainWindow()
        {
            InitializeComponent();

            _l = new Vector3D(0, 0, 0);
            _v = new Vector3D(0, 0, 0);

            StrengthSlider.ValueChanged += NormalStrengthSliderChanged;
            StrengthLabel.Content = StrengthSlider.Value;

            LightVectorXSlider.ValueChanged += LightVectorXSliderChanged;
            LightXLabel.Content = LightVectorXSlider.Value;

            LightVectorYSlider.ValueChanged += LightVectorYSliderChanged;
            LightYLabel.Content = LightVectorYSlider.Value;

            LightVectorZSlider.ValueChanged += LightVectorZSliderChanged;
            LightZLabel.Content = LightVectorZSlider.Value;

            EyeVectorXSlider.ValueChanged += EyeVectorXSliderChanged;
            EyeXLabel.Content = EyeVectorXSlider.Value;

            EyeVectorYSlider.ValueChanged += EyeVectorYSliderChanged;
            EyeYLabel.Content = EyeVectorYSlider.Value;

            EyeVectorZSlider.ValueChanged += EyeVectorZSliderChanged;
            EyeZLabel.Content = EyeVectorZSlider.Value;

            var img = new BitmapImage(new Uri(@"..\..\img\x.png",UriKind.Relative));
            OriginalImg.Height = img.Height;
            ImgControl.Height = img.Height;

            _normalMap = new PixelMap(img.PixelWidth, img.PixelHeight);
            _lightedImgMap = new PixelMap(img.PixelWidth, img.PixelHeight);

            var stride = img.PixelWidth * 4;
            var data = new byte[stride * img.PixelHeight];
            img.CopyPixels(data, stride, 0);
            _imgMap = new PixelMap(data, img.PixelWidth, img.PixelHeight);

            OriginalImg.Source = img;

            var args = new RoutedPropertyChangedEventArgs<double>(0.5, 0.5) { RoutedEvent = RangeBase.ValueChangedEvent };
            StrengthSlider.RaiseEvent(args);
        }

        private void LightVectorXSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double val = Math.Round(e.NewValue, 2);
            LightXLabel.Content = val;
            _l.X = val;
            Apply();
        }

        private void LightVectorYSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double val = Math.Round(e.NewValue, 2);
            LightYLabel.Content = val;
            _l.Y = val;
            Apply();
        }

        private void LightVectorZSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double val = Math.Round(e.NewValue, 2);
            LightZLabel.Content = val;
            _l.Z = val;
            Apply();
        }

        private void EyeVectorXSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double val = Math.Round(e.NewValue, 2);
            EyeXLabel.Content = val;
            _v.X = val;
            Apply();
        }

        private void EyeVectorYSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double val = Math.Round(e.NewValue, 2);
            EyeYLabel.Content = val;
            _v.Y = val;
            Apply();
        }

        private void EyeVectorZSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double val = Math.Round(e.NewValue, 2);
            EyeZLabel.Content = val;
            _v.Z = val;
            Apply();
        }

        private void Apply()
        {
            var temp = _imgMap.Lighten(_normalMap, _l, _v);
            OriginalImg.Source = temp.ConvertToBitmapImage();
        }

        private void NormalStrengthSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double val = Math.Round(e.NewValue, 2);
            StrengthLabel.Content = val;

            _normalMap = _imgMap.Smooth().GetNormalMap(val);
            ImgControl.Source = _normalMap.ConvertToBitmapImage();
        }
    }
}