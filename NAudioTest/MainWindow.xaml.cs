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
                EffectStream effect = new EffectStream(pcm);
                stream = new BlockAlignReductionStream(effect);
            }
            else if (open.FileName.EndsWith(".wav"))
            {
                WaveStream pcm = new WaveChannel32(new WaveFileReader(open.FileName));
                EffectStream effect = new EffectStream(pcm);
                stream = new BlockAlignReductionStream(effect);
            }
            else throw new InvalidOperationException("Not correct audio file type.");
            output = new DirectSoundOut(200);
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
                if (output.PlaybackState == PlaybackState.Playing) output.Stop();
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

        public class WaveTone : WaveStream
        {
            private double frequency;
            private double amplitude;
            private double time;

            public WaveTone(double f, double a)
            {
                this.time = 0.0;
                this.frequency = f;
                this.amplitude = a;
            }

            public override long Position { get; set; }

            public override long Length => long.MaxValue;

            public override WaveFormat WaveFormat => new WaveFormat(44100, 16, 1);

            public override int Read(byte[] buffer, int offset, int count)
            {
                int samples = count / 2;
                for (int i = 0; i < samples; i++)
                {
                    double sine = amplitude * Math.Sin(Math.PI * 2 * frequency * time);
                    time += 1.0 / 44100.0;
                    short truncated = (short)Math.Round(sine * (Math.Pow(2, 15) - 1));
                    buffer[i * 2] = (byte)(truncated & 0x00ff);
                    buffer[i * 2 + 1] = (byte)((truncated & 0xff00) >> 8);
                }

                return count;
            }
        }

        private void StartToneButton_Click(object sender, RoutedEventArgs e)
        {
            WaveTone tone = new WaveTone(1000, 0.1);
            stream = new BlockAlignReductionStream(tone);

            output = new DirectSoundOut();
            output.Init(stream);
            output.Play();
        }

        private void StopToneButton_Click(object sender, RoutedEventArgs e)
        {
            if (output != null) output.Stop();
        }

        private void RefreshSourcesButton_Click(object sender, RoutedEventArgs e)
        {
            List<WaveInCapabilities> sources = new List<WaveInCapabilities>();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                sources.Add(WaveIn.GetCapabilities(i));
            }

            SourcesList.Items.Clear();

            foreach (var source in sources)
            {
                ListViewItem item = new ListViewItem();
                item.Content = source.ProductName;
                
                SourcesList.Items.Add(item);
            }
        }

        private WaveIn sourceStream = null;
        private DirectSoundOut waveOut = null;

        private void StartSourcesButton_Click(object sender, RoutedEventArgs e)
        {
            if (SourcesList.Items.Count == 0) return;

            Console.WriteLine("Recording...");

            sourceStream = new WaveIn();
            sourceStream.DeviceNumber = 0;
            sourceStream.WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(0).Channels);

            WaveInProvider waveIn = new WaveInProvider(sourceStream);

            waveOut = new DirectSoundOut();
            waveOut.Init(waveIn);

            sourceStream.StartRecording();
            waveOut.Play();
        }

        private void StopSourcesButton_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
            if (sourceStream != null)
            {
                sourceStream.StopRecording();
                sourceStream.Dispose();
                sourceStream = null;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            StopSourcesButton_Click(sender, e);
            this.Close();
        }
    }
}
