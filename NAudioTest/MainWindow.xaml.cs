using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using NAudio.Wave;

namespace NAudioTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private BlockAlignReductionStream stream= null;
        private DirectSoundOut output = null;
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Audio File (*.mp3;*.wav)|*.mp3;*.wav;";
            if (open.ShowDialog() != true) return;

            DisposeWave();

            if (open.FileName.EndsWith(".mp3"))
            {
                WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(open.FileName));
                stream = new BlockAlignReductionStream(pcm);
            }
            else if (open.FileName.EndsWith(".wav"))
            {
                WaveStream pcm = new WaveChannel32(new WaveFileReader(open.FileName));
                stream = new BlockAlignReductionStream(pcm);
            }
            else throw new InvalidOperationException("Not correct audio file type.");
            output = new DirectSoundOut();
            output.Init(new WaveChannel32(stream));
            output.Play();

            PauseButton.IsEnabled = true;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (output != null)
            {
                if (output.PlaybackState == NAudio.Wave.PlaybackState.Playing) output.Pause();
                else if (output.PlaybackState == NAudio.Wave.PlaybackState.Paused) output.Play();
            }
        }

        private void DisposeWave()
        {
            if (output != null)
            {
                if (output.PlaybackState == NAudio.Wave.PlaybackState.Playing) output.Stop();
                output.Dispose();
                output = null;
            }
            if (stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DisposeWave();
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "MP3 File (*.mp3)|*.mp3;";
            if (open.ShowDialog() != true) return;

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "WAV File (*.wav)|*.wav;";
            if (save.ShowDialog() != true) return;

            using (Mp3FileReader mp3 = new Mp3FileReader(open.FileName))
            {
                using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
                {
                    WaveFileWriter.CreateWaveFile(save.FileName, pcm);
                }
            }
        }
    }
}
