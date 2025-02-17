﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SixLabors.ImageSharp;
using IS = SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Collections.ObjectModel;
using System.IO;

namespace WorkItWithWario
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<BitmapSource> _frames = new ObservableCollection<BitmapSource>();

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void RecalculateFrames()
        {
            _frames = new ObservableCollection<BitmapSource>();

            if (xTextBox is null || yTextBox is null || widthTextBox is null || heightTextBox is null || WholeImage is null || framesTextBox is null)
                return;

            int originX = ParseInt(xTextBox.Text);
            int originY = ParseInt(yTextBox.Text);
            int width = ParseInt(widthTextBox.Text, 1);
            int height = ParseInt(heightTextBox.Text, 1);
            int frames = ParseInt(framesTextBox.Text, 1);

            using var img = IS.Image.Load<Rgba32>("WorkItWithWario.png");

            var frameCoordinates = new List<RectangleF>();

            for (int i = 0; i < frames; i++)
            {
                int x = originX + width * i;
                int y = originY;
                
                var window = img.Clone(a => a.Crop(new IS.Rectangle(x, y, width, height)));
                frameCoordinates.Add(new RectangleF(x, y, width, height));

                BitmapSource bitmapSource = Convert(window);
                _frames.Add(bitmapSource);
            }

            foreach (var coords in frameCoordinates)
            {
                img.Mutate(a => a.Draw(IS.Color.Red, 2, coords));
            }

            // TODO remove this
            // throw away the bottom part of the image for display purposes
            img.Mutate(async => async.Crop(new IS.Rectangle(0,0,img.Width, img.Height - 380)));

            WholeImage.Source = Convert(img);
        }

        private static int ParseInt(string s, int min = 0)
        {
            if (string.IsNullOrEmpty(s))
                return min;
            
            if(int.TryParse(s, out var result))
            {
                return Math.Max(result, min);
            }
            return min;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RecalculateFrames();

            int i = 0; 

            while (true)
            {
                try
                {
                    int millis = ParseInt(msTextBox.Text, 100);
                    await Task.Delay(millis);

                    FrameImage.Source = _frames[i];

                    i++;

                    if (i >= _frames.Count)
                    {
                        i = 0;
                    }
                }
                catch (Exception)
                {
                    // HACK: add proper thread safety later
                }

            }
        }

        private BitmapSource Convert(IS.Image<Rgba32> image)
        {
            var memoryStream = new MemoryStream();
            image.SaveAsPng(memoryStream);
            memoryStream.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        private void RecalculateFrames(object sender, TextChangedEventArgs e)
        {
            RecalculateFrames();
        }
    }
}
