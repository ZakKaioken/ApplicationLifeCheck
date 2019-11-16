using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AppLifeCheck
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        

        DispatcherTimer UpdateTimer = new DispatcherTimer();
        public MainWindow()
        {

            InitializeComponent();
         
        }
        DateTime oldesttime;

        static StringBuilder sb = new StringBuilder();
        private void Tree_Loaded(object sender, RoutedEventArgs e)
        {
            var ps = Process.GetProcesses().ToList();

            foreach (Process p in ps.ToArray())
            {
                try
                {
                    oldesttime = p.StartTime;
                } catch (Exception sa)
                {
                    ps.Remove(p);
                }
            }

                ps = ps.OrderByDescending(x => (DateTime.Now - x.StartTime)).ToList();
          

            oldesttime = ps[0].StartTime;

            foreach (Process p in ps)
            {
                
                try
                {


                    sb.Clear();
                    sb.Append($" {p.ProcessName}: ");

                    GetTime(p.StartTime);

                    TreeViewItem newItem = CreateProcessInfo(p, sb);
                    Treeb.Items.Add(newItem);
                } catch (Exception pe)
                {
                    //Console.WriteLine(e.Message);
                }
            }
        }

        private static void GetTime(DateTime p)
        {
            var time = DateTime.Now - p;
            float seconds = time.Seconds;
            float minutes = time.Minutes;
            float hours = time.Hours;
            float days = time.Days;


            if (days > 0) {
                sb.AppendFormat("{0} days, ", Math.Floor(days));
                    }
            if (hours > 0)
            {
                sb.AppendFormat("{0} hours, ", Math.Floor(hours));
            }
            if (minutes > 0)
            {
                sb.AppendFormat("{0} minues, ", Math.Floor(minutes));
            }
            if (seconds > 0)
            {
                sb.AppendFormat("{0} seconds.", Math.Floor(seconds));
            }
        }

        private TreeViewItem CreateProcessInfo(Process p, StringBuilder sb)
        {
            var file = p.MainModule.FileName;
            var ico = System.Drawing.Icon.ExtractAssociatedIcon(p.MainModule.FileName);
            var bmi = FromIconToBitmap(ico);

            TreeViewItem newItem = new TreeViewItem();
            System.Windows.Controls.Image tempImage = new System.Windows.Controls.Image();
            BitmapImage bitmapImage = Bitmap2BitmapImage(bmi);

            tempImage.Source = ResizeImage(bitmapImage, 32, 32);

            TextBlock tempTextBlock = new TextBlock();
            tempTextBlock.Inlines.Add(tempImage);
            tempTextBlock.Inlines.Add(sb.ToString());
            newItem.Header = tempTextBlock;
            return newItem;
        }

        private static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
        public Bitmap FromIconToBitmap(Icon icon)
        {
            Bitmap bmp = new Bitmap(icon.Width, icon.Height);
            using (Graphics gp = Graphics.FromImage(bmp))
            {
                gp.Clear(System.Drawing.Color.Transparent);
                gp.DrawIcon(icon, new System.Drawing.Rectangle(0, 0, icon.Width, icon.Height));
            }
            return bmp;
        }
        private static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        public static BitmapImage ResizeImage(BitmapImage imagg, int width, int height)
        {
            var image = BitmapImage2Bitmap(imagg);
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return Bitmap2BitmapImage(destImage);
        }

        private void PCTime_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTimer.Interval = new TimeSpan(0, 0, 0, 1);
            UpdateTimer.Tick += (y, v) => DoUpdate();
            UpdateTimer.Start();
        }
        StringBuilder se = new StringBuilder();
        private void DoUpdate()
        {
            sb.Clear();
            sb.Append("One may think your pc has been on for: ");
            GetTime(oldesttime);
            PCTime.Content = sb.ToString();
        }
    }
    public class MyItem
    {
        public string Text { get; set; }
        public string ImageSource { get; set; }
    }
}
