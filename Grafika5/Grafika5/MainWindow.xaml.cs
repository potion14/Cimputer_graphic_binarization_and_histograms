using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace Grafika5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] t, bufor;
        private byte[] original;
        private int[] histogram_bufor_phR;
        private int[] histogram_bufor_phG;
        private int[] histogram_bufor_phB;
        private int tmp, w, h;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Load_image(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName != "" && openFileDialog.FileName.Contains(".PNG") || openFileDialog.FileName.Contains(".png") || openFileDialog.FileName.Contains(".jpg"))
            {
                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                PixelFormat pf = PixelFormats.Bgra32;
                int stride = (bitmap.PixelWidth * pf.BitsPerPixel + 7) / 8;
                byte[] bufor = new byte[stride * bitmap.PixelHeight];
                bitmap.CopyPixels(bufor, stride, 0);
                t = bufor;
                original = bufor;
                tmp = stride;
                w = bitmap.PixelWidth;
                h = bitmap.PixelHeight;
                BitmapSource new_bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, t, tmp);
                Image_placeholder.Source = new_bitmap;
                //Calculate_avarage(t);

                int[] histogram_buforR = new int[256];
                int[] histogram_buforG = new int[256];
                int[] histogram_buforB = new int[256];
                int dividerR = 1;
                int dividerG = 1;
                int dividerB = 1;
                for (int i = 0; i < t.Length; i ++)
                {
                    if (i % 4 == 2)
                    {
                        histogram_buforR[t[i]] += 1;
                    }
                    if (i % 4 == 1)
                    {
                        histogram_buforG[t[i]] += 1;
                    }
                    if (i % 4 == 0)
                    {
                        histogram_buforB[t[i]] += 1;
                    }
                }
                if (histogram_buforR.Max() > 127) dividerR = histogram_buforR.Max() / 128;
                if (histogram_buforG.Max() > 127) dividerG = histogram_buforG.Max() / 128;
                if (histogram_buforB.Max() > 127) dividerB = histogram_buforB.Max() / 128;
                for (int i = 0; i < 256; i++)
                {
                    Rectangle recR = new Rectangle
                    {
                        Width = 1,
                        Height = histogram_buforR[i] / dividerR,
                        Fill = Brushes.Red
                    };
                    Rectangle recG = new Rectangle
                    {
                        Width = 1,
                        Height = histogram_buforG[i] / dividerG,
                        Fill = Brushes.Green
                    };
                    Rectangle recB = new Rectangle
                    {
                        Width = 1,
                        Height = histogram_buforB[i] / dividerB,
                        Fill = Brushes.Blue
                    };
                    Canvas.SetLeft(recR, i);
                    Canvas.SetBottom(recR, 0);
                    Histogram.Children.Add(recR);

                    Canvas.SetLeft(recG, i);
                    Canvas.SetBottom(recG, 0);
                    Histogram2.Children.Add(recG);

                    Canvas.SetLeft(recB, i);
                    Canvas.SetBottom(recB, 0);
                    Histogram3.Children.Add(recB);
                }
                histogram_bufor_phR = histogram_buforR;
                histogram_bufor_phG = histogram_buforG;
                histogram_bufor_phB = histogram_buforB;
            }
            else
            {
                MessageBox.Show("Nieprawidłowy format pliku!");
            }
        }

        private void Odcienie_szarosci2(object sender, RoutedEventArgs e)
        {
            if (t != null)
            {
                Histogram.Children.Clear();
                Histogram2.Children.Clear();
                Histogram3.Children.Clear();
                byte[] t2 = new byte[t.Length];
                int[] histogram_bufor = new int[256];
                int divider = 1;
                Image_placeholder.Source = null;
                for (int i = 0; i < t.Length; i += 4)
                {
                    int tmp = (t[i] + t[i + 1] + t[i + 2]) / 3;
                    t2[i] = (byte)tmp;
                    t2[i + 1] = (byte)tmp;
                    t2[i + 2] = (byte)tmp;
                    t2[i + 3] = 255;
                    histogram_bufor[tmp] += 1;
                }
                BitmapSource new_bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, t2, tmp);
                Image_placeholder.Source = new_bitmap;
                t = original;
                if (histogram_bufor.Max() > 127) divider = histogram_bufor.Max()/128;
                for (int i = 0; i < 256; i++)
                {
                    Rectangle rec = new Rectangle
                    {
                        Width = 1,
                        Height = histogram_bufor[i] / divider,
                        Fill = Brushes.Black
                    };
                    Canvas.SetLeft(rec, i);
                    Canvas.SetBottom(rec, 0);
                    Histogram.Children.Add(rec);
                }
            }
        }

        private void Rozciagnij_histogram(object sender, RoutedEventArgs e)
        {
            if (t != null)
            {
                Histogram.Children.Clear();
                Histogram2.Children.Clear();
                Histogram3.Children.Clear();

                histogram_bufor_phR = RH_kalkulacje(histogram_bufor_phR);
                histogram_bufor_phG = RH_kalkulacje(histogram_bufor_phG);
                histogram_bufor_phB = RH_kalkulacje(histogram_bufor_phB);

                byte[] t2 = new byte[t.Length];
                int[] histogram_buforR2 = new int[256];
                int[] histogram_buforG2 = new int[256];
                int[] histogram_buforB2 = new int[256];
                int dividerR = 1;
                int dividerG = 1;
                int dividerB = 1;
                Image_placeholder.Source = null;
                for (int i = 0; i < t.Length; i += 4)
                {
                    t2[i] = (byte)histogram_bufor_phB[t[i]];
                    t2[i + 1] = (byte)histogram_bufor_phG[t[i + 1]];
                    t2[i + 2] = (byte)histogram_bufor_phR[t[i + 2]];
                    t2[i + 3] = 255;
                    histogram_buforB2[histogram_bufor_phB[t[i]]] += 1;
                    histogram_buforG2[histogram_bufor_phG[t[i + 1]]] += 1;
                    histogram_buforR2[histogram_bufor_phR[t[i + 2]]] += 1;
                }
                BitmapSource new_bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, t2, tmp);
                Image_placeholder.Source = new_bitmap;
                if (histogram_buforR2.Max() > 127) dividerR = histogram_buforR2.Max() / 128;
                if (histogram_buforG2.Max() > 127) dividerG = histogram_buforG2.Max() / 128;
                if (histogram_buforB2.Max() > 127) dividerB = histogram_buforB2.Max() / 128;
                for (int i = 0; i < 256; i++)
                {
                    Rectangle recR = new Rectangle
                    {
                        Width = 1,
                        Height = histogram_buforR2[i] / dividerR,
                        Fill = Brushes.Red
                    };
                    Rectangle recG = new Rectangle
                    {
                        Width = 1,
                        Height = histogram_buforG2[i] / dividerG,
                        Fill = Brushes.Green
                    };
                    Rectangle recB = new Rectangle
                    {
                        Width = 1,
                        Height = histogram_buforB2[i] / dividerB,
                        Fill = Brushes.Blue
                    };
                    Canvas.SetLeft(recR, i);
                    Canvas.SetBottom(recR, 0);
                    Histogram.Children.Add(recR);

                    Canvas.SetLeft(recG, i);
                    Canvas.SetBottom(recG, 0);
                    Histogram2.Children.Add(recG);

                    Canvas.SetLeft(recB, i);
                    Canvas.SetBottom(recB, 0);
                    Histogram3.Children.Add(recB);
                }
            }
        }

        private void Wyrownaj_histogram(object sender, RoutedEventArgs e)
        {
            if (t != null)
            {
                Histogram.Children.Clear();
                Histogram2.Children.Clear();
                Histogram3.Children.Clear();

                histogram_bufor_phR = WH_kalkulacje(histogram_bufor_phR);
                histogram_bufor_phG = WH_kalkulacje(histogram_bufor_phG);
                histogram_bufor_phB = WH_kalkulacje(histogram_bufor_phB);

                byte[] t2 = new byte[t.Length];
                int[] histogram_buforR2 = new int[256];
                int[] histogram_buforG2 = new int[256];
                int[] histogram_buforB2 = new int[256];
                int dividerR = 1;
                int dividerG = 1;
                int dividerB = 1;
                Image_placeholder.Source = null;
                for (int i = 0; i < t.Length; i += 4)
                {
                    t2[i] = (byte)histogram_bufor_phB[t[i]];
                    t2[i + 1] = (byte)histogram_bufor_phG[t[i + 1]];
                    t2[i + 2] = (byte)histogram_bufor_phR[t[i + 2]];
                    t2[i + 3] = 255;
                    histogram_buforB2[histogram_bufor_phB[t[i]]] += 1;
                    histogram_buforG2[histogram_bufor_phG[t[i + 1]]] += 1;
                    histogram_buforR2[histogram_bufor_phR[t[i + 2]]] += 1;
                }
                BitmapSource new_bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, t2, tmp);
                Image_placeholder.Source = new_bitmap;
                bufor = t2;
                if (histogram_buforR2.Max() > 127) dividerR = histogram_buforR2.Max() / 128;
                if (histogram_buforG2.Max() > 127) dividerG = histogram_buforG2.Max() / 128;
                if (histogram_buforB2.Max() > 127) dividerB = histogram_buforB2.Max() / 128;
                for (int i = 0; i < 256; i++)
                {
                    Rectangle recR = new Rectangle
                    {
                        Width = 1,
                        Height = histogram_buforR2[i] / dividerR,
                        Fill = Brushes.Red
                    };
                    Rectangle recG = new Rectangle
                    {
                        Width = 1,
                        Height = histogram_buforG2[i] / dividerG,
                        Fill = Brushes.Green
                    };
                    Rectangle recB = new Rectangle
                    {
                        Width = 1,
                        Height = histogram_buforB2[i] / dividerB,
                        Fill = Brushes.Blue
                    };
                    Canvas.SetLeft(recR, i);
                    Canvas.SetBottom(recR, 0);
                    Histogram.Children.Add(recR);

                    Canvas.SetLeft(recG, i);
                    Canvas.SetBottom(recG, 0);
                    Histogram2.Children.Add(recG);

                    Canvas.SetLeft(recB, i);
                    Canvas.SetBottom(recB, 0);
                    Histogram3.Children.Add(recB);
                }
            }
        }

        private int[] RH_kalkulacje(int[] array)
        {
            int minValue = 0;
            for (int i = 0; i < 256; i++)
            {
                if (array[i] != 0)
                {
                    minValue = i;
                    break;
                }
            }

            int maxValue = 255;
            for (int i = 255; i >= 0; i--)
            {
                if (array[i] != 0)
                {
                    maxValue = i;
                    break;
                }
            }
            double a = 255.0 / (maxValue - minValue);
            for (int i = 0; i < 256; i++)
            {
                array[i] = (int)(a * (i - minValue));
            }
            return array;
        }

        private int[] WH_kalkulacje(int[] array)
        {
            double minValue = 0;
            for (int i = 0; i < 256; i++)
            {
                if (array[i] != 0)
                {
                    minValue = array[i];
                    break;
                }
            }

            double sum = 0;
            for (int i = 0; i < 256; i++)
            {
                sum += array[i];
                array[i] = (int)(((sum - minValue) / (w * h - minValue)) * 255.0);
            }
            return array;
        }

        private void Load_original(object sender, RoutedEventArgs e)
        {
            if (original != null)
            {
                Image_placeholder.Source = null;
                BitmapSource original_bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, original, tmp);
                Image_placeholder.Source = original_bitmap;
                t = original;
            }
        }

        private void Binaryzacja(object sender, RoutedEventArgs e)
        {
            if (t != null && prog_txtbox.Text != null && int.TryParse(prog_txtbox.Text, out _) && Convert.ToInt64(prog_txtbox.Text) >= 0 && Convert.ToInt64(prog_txtbox.Text) <= 256)
            {
                int[] _histogram = new int[256]; 
                byte[] t3 = new byte[t.Length];
                for (int i = 0; i < t.Length; i += 4)
                {
                    int tmp = (t[i] + t[i + 1] + t[i + 2]) / 3;
                    t3[i] = (byte)tmp;
                    t3[i + 1] = (byte)tmp;
                    t3[i + 2] = (byte)tmp;
                    t3[i + 3] = 255;
                    _histogram[tmp] += 1;
                }
                Histogram.Children.Clear();
                Histogram2.Children.Clear();
                Histogram3.Children.Clear();

                _histogram = RH_kalkulacje(_histogram);

                //histogram_bufor_phR = RH_kalkulacje(histogram_bufor_phR);
                //histogram_bufor_phG = RH_kalkulacje(histogram_bufor_phG);
                //histogram_bufor_phB = RH_kalkulacje(histogram_bufor_phB);

                byte[] t2 = new byte[t.Length];
                Image_placeholder.Source = null;
                for (int i = 0; i < t.Length; i += 4)
                {
                    if (_histogram[t3[i]] >= Convert.ToInt32(prog_txtbox.Text))
                    {
                        t2[i] = 255;
                        t2[i + 1] = 255;
                        t2[i + 2] = 255;
                    }
                    else
                    {
                        t2[i] = 0;
                        t2[i + 1] = 0;
                        t2[i + 2] = 0;
                    }
                    
                    t2[i + 3] = 255;
                }
                BitmapSource new_bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, t2, tmp);
                Image_placeholder.Source = new_bitmap;
            }
        }

        private void Binaryzacja_procent_czarnego(object sender, RoutedEventArgs e)
        {
            if (t != null && procent_czarnego_txtbox.Text != null && int.TryParse(procent_czarnego_txtbox.Text, out _) && Convert.ToInt64(procent_czarnego_txtbox.Text) >= 0 && Convert.ToInt64(procent_czarnego_txtbox.Text) <= 100)
            {
                int[] _histogram = new int[256];
                byte[] t3 = new byte[t.Length];
                for (int i = 0; i < t.Length; i += 4)
                {
                    int tmp = (t[i] + t[i + 1] + t[i + 2]) / 3;
                    t3[i] = (byte)tmp;
                    t3[i + 1] = (byte)tmp;
                    t3[i + 2] = (byte)tmp;
                    t3[i + 3] = 255;
                    _histogram[tmp] += 1;
                }
                Histogram.Children.Clear();
                Histogram2.Children.Clear();
                Histogram3.Children.Clear();

                double suma = 0;
                int j;
                for (j = 0; j < 256; j++)
                {
                    suma += _histogram[j];
                    if (suma / (t.Length / 4) >= Convert.ToDouble(procent_czarnego_txtbox.Text) / 100) break;
                }
                byte[] t2 = new byte[t.Length];
                Image_placeholder.Source = null;
                for (int i = 0; i < t.Length; i += 4)
                {
                    if (t3[i] > j)
                    {
                        t2[i] = 255;
                        t2[i + 1] = 255;
                        t2[i + 2] = 255;
                    }
                    else
                    {
                        t2[i] = 0;
                        t2[i + 1] = 0;
                        t2[i + 2] = 0;
                    }

                    t2[i + 3] = 255;
                }
                BitmapSource new_bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, t2, tmp);
                Image_placeholder.Source = new_bitmap;
            }
        }

        private void Binaryzacja_iteracja(object sender, RoutedEventArgs e)
        {
            if (t != null && procent_czarnego_txtbox.Text != null)
            {
                byte[] t3 = new byte[t.Length];
                for (int i = 0; i < t.Length; i += 4)
                {
                    int tmp = (t[i] + t[i + 1] + t[i + 2]) / 3;
                    t3[i] = (byte)tmp;
                    t3[i + 1] = (byte)tmp;
                    t3[i + 2] = (byte)tmp;
                    t3[i + 3] = 255;
                }
                Histogram.Children.Clear();
                Histogram2.Children.Clear();
                Histogram3.Children.Clear();

                int srednia_tla = (t3[0] + t3[w] + t3[w*h - w] + t[w*h])/4;
                int srednia_obiektu = 0;
                int suma_tla = 0;
                int suma_obiektu = 0;
                int licznik_tla = 0;
                int licznik_obiektu = 0;
                int T = 0;
                int T2 = 0;

                for(int i =0; i < t.Length; i+=4)
                {
                    srednia_obiektu += t3[i];
                }
                srednia_obiektu -= srednia_tla;
                srednia_obiektu = srednia_obiektu / (t.Length / 4) - 4;
                T = (srednia_obiektu + srednia_tla) / 2;

                byte[] t2 = new byte[t.Length];
                Image_placeholder.Source = null;
                while(T != T2)
                {
                    T2 = T;
                    for (int i = 0; i < t.Length; i += 4)
                    {
                        if (t3[i] >= T)
                        {
                            t2[i] = 255;
                            t2[i + 1] = 255;
                            t2[i + 2] = 255;
                            licznik_obiektu++;
                            suma_obiektu += t3[i];
                        }
                        else
                        {
                            t2[i] = 0;
                            t2[i + 1] = 0;
                            t2[i + 2] = 0;
                            licznik_tla++;
                            suma_tla += t3[i];
                        }
                        t2[i + 3] = 255;
                    }
                    srednia_tla = suma_tla / licznik_tla;
                    srednia_obiektu = suma_obiektu / licznik_obiektu;
                    T = (srednia_tla + srednia_obiektu) / 2;
                    licznik_obiektu = 0;
                    licznik_tla = 0;
                    suma_obiektu = 0;
                    suma_tla = 0;
                }
                MessageBox.Show("Treshold: " + Convert.ToString(T));
                BitmapSource new_bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, t2, tmp);
                Image_placeholder.Source = new_bitmap;
            }
        }

        private void Save_original(object sender, EventArgs e)
        {
            
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Format JPEG|*.jpg";
            saveFileDialog.Title = "Zapisz jako obraz JPEG";
            if (saveFileDialog.ShowDialog() == true)
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                BitmapFrame outputFrame = BitmapFrame.Create(BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, bufor, tmp));
                encoder.Frames.Add(outputFrame);

                using (var MyFile = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    encoder.Save(MyFile);
                    MyFile.Close();
                }
            }
        }
    }
}
