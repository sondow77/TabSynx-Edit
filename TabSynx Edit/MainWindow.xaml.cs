using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using NAudio.Wave;
using WaveFormRendererLib;
using Path = System.IO.Path;

namespace TabSynx_Edit
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WaveFormRenderer renderer;
        WaveFormRendererSettings standardSettings;
        AveragePeakProvider averagePeakProvider;
        SamplingPeakProvider samplingPeakProvider;
        MaxPeakProvider maxPeakProvider;
        RmsPeakProvider rmsPeakProvider;
        Mp3FileReader mp3_file;
        WaveOut wo_file;
        

        public MainWindow()
        {
            InitializeComponent();

            maxPeakProvider = new MaxPeakProvider();
            rmsPeakProvider = new RmsPeakProvider(200); // e.g. 200
            samplingPeakProvider = new SamplingPeakProvider(200); // e.g. 200
            averagePeakProvider = new AveragePeakProvider(4); // e.g. 4


            standardSettings = new StandardWaveFormRendererSettings();
            standardSettings.Width = 1080;
            standardSettings.TopHeight = 128;
            standardSettings.BottomHeight = 128;
            standardSettings.TopPeakPen = new Pen(Color.DarkGreen);
            standardSettings.BottomPeakPen = new Pen(Color.Green);
            standardSettings.BackgroundColor = Color.Transparent;

            renderer = new WaveFormRenderer();
        }

        Timer timer = new Timer();

        private void Btn_browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd_browse = new OpenFileDialog
            {
                Filter = "Audio|*.mp3",
                Title = "Cargar audio"
            };
            DialogResult dr_browse = ofd_browse.ShowDialog();
            if(dr_browse == System.Windows.Forms.DialogResult.OK)
            {
                txt_file.Text = ofd_browse.FileName;
            }
        }
        public static System.Windows.Media.ImageSource ConvertImage(System.Drawing.Image image)
        {
            try
            {
                if (image != null)
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
                    image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch { }
            return null;
        }
        private void Btn_open_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(txt_file.Text) && Path.GetExtension(txt_file.Text) == ".mp3")
            {
                mp3_file = new Mp3FileReader(txt_file.Text);
                wo_file = new WaveOut();
                wo_file.Init(mp3_file);
                UpdateImage();
                wo_file.GetPosition();
                btn_play.IsEnabled = true;
            }
        }
        private void UpdateSeekBar()
        {
            ln_seek.Margin = new Thickness(((double)standardSettings.Width / mp3_file.TotalTime.TotalMilliseconds) * mp3_file.CurrentTime.TotalMilliseconds, 0, 0, 0);
            if(ln_seek.Margin.Left > (sv_audio.HorizontalOffset + sv_audio.ActualWidth) && wo_file.PlaybackState == PlaybackState.Playing)
            {
                sv_audio.ScrollToHorizontalOffset(sv_audio.HorizontalOffset + sv_audio.ActualWidth);
            }
        }
        private void Btn_play_Click(object sender, RoutedEventArgs e)
        {
            timer.Interval = 1;
            timer.Tick += Timer_Tick;
            timer.Start();

            if (btn_play.Content.ToString() == "Reproducir")
            {
                wo_file.Play();
                btn_play.Content = "Pausar";
            }
            else
            {
                wo_file.Pause();
                btn_play.Content = "Reproducir";
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateSeekBar();
        }

        private void UpdateImage()
        {
            standardSettings.Width = Convert.ToInt32(mp3_file.TotalTime.TotalSeconds) * 100;
            Image image = renderer.Render(txt_file.Text, averagePeakProvider, standardSettings);
            img_wave.Source = ConvertImage(image);
        }

        private void Btn_seek_Click(object sender, RoutedEventArgs e)
        {
            double real_pos = (sv_audio.HorizontalOffset + (Mouse.GetPosition(this).X - sv_audio.Margin.Left)) * (mp3_file.Length / standardSettings.Width);

            mp3_file.Seek(Convert.ToInt64((sv_audio.HorizontalOffset + (Mouse.GetPosition(this).X - sv_audio.Margin.Left)) * (mp3_file.Length / standardSettings.Width)) , SeekOrigin.Begin);
        }
    }
}
