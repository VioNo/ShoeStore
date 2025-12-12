using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows;

namespace ShoeStore
{
    //public class ImagePathConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        string imageName = value as string;

    //        if (string.IsNullOrWhiteSpace(imageName))
    //        {
    //            imageName = "picture.png";
    //        }

    //        try
    //        {
    //            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    //            string projectDir = GetProjectDirectory(baseDir);

    //            if (!string.IsNullOrEmpty(projectDir))
    //            {
    //                string imagePath = Path.Combine(projectDir, "Resources", imageName);

    //                if (File.Exists(imagePath))
    //                {
    //                    return LoadImageFromPath(imagePath);
    //                }
    //            }
    //            return null;
    //        }
    //        catch
    //        {
    //            return null;
    //        }
    //    }

    //    private BitmapImage LoadImageFromPath(string imagePath)
    //    {
    //        try
    //        {
    //            BitmapImage bitmap = new BitmapImage();
    //            bitmap.BeginInit();
    //            bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
    //            bitmap.CacheOption = BitmapCacheOption.OnLoad;
    //            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
    //            bitmap.EndInit();
    //            bitmap.Freeze();
    //            return bitmap;
    //        }
    //        catch
    //        {
    //            return null;
    //        }
    //    }

    //    private string GetProjectDirectory(string baseDirectory)
    //    {
    //        try
    //        {
    //            if (baseDirectory.Contains(@"\bin\"))
    //            {
    //                string projectPath = Path.Combine(baseDirectory, "..", "..");
    //                return Path.GetFullPath(projectPath);
    //            }
    //            return baseDirectory;
    //        }
    //        catch
    //        {
    //            return null;
    //        }
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}