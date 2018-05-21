//------------------------------------------------------------------------------
// <copyright file="HeaderToImageConverter.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Aras.VS.MethodPlugin.Dialogs.Directory.Data;

namespace Aras.VS.MethodPlugin.Dialogs.Converters
{
	/// <summary>
	/// Converts a full path to a specific image type of a drive, folder or file
	/// </summary>
	[ValueConversion(typeof(DirectoryItemType), typeof(BitmapImage))]
	public class HeaderToImageConverter : IValueConverter
	{
		public static HeaderToImageConverter Instance = new HeaderToImageConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var image = Properties.Resources.file;

			switch ((DirectoryItemType)value)
			{
				case DirectoryItemType.Drive:
					image = Properties.Resources.drive;
					break;
				case DirectoryItemType.Folder:
					image = Properties.Resources.folder_closed;
					break;
			}

			return ToBitmapSource(image);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		private BitmapSource ToBitmapSource(Bitmap source)
		{
			BitmapSource bitSrc = null;

			var hBitmap = source.GetHbitmap();

			try
			{
				bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
					hBitmap,
					IntPtr.Zero,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions());
			}
			catch (Win32Exception)
			{
				bitSrc = null;
			}
			finally
			{
				DeleteObject(hBitmap);
			}

			return bitSrc;
		}

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);
	}
}
