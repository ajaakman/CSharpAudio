using SDL2;
using System;
using System.Windows;

namespace CSharp_SDL2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SDL.SDL_AudioSpec spec = new SDL.SDL_AudioSpec();
        private uint device = 0;

        public MainWindow()
        {
            InitializeComponent();

            if (SDL.SDL_Init(SDL.SDL_INIT_AUDIO) < 0)
                Console.WriteLine("SDL Failed to Initilize!!!");

            spec.channels = 1;
            spec.freq = 44100;
            spec.format = SDL.AUDIO_S16SYS;
            spec.samples = 2048;
            spec.callback = MyAudioCallback;

            device = SDL.SDL_OpenAudioDevice(null, 0, ref spec, out spec, 0);

            if (device == 0)
                Console.WriteLine("SDL Failed to open Audio Device!!!");

            SDL.SDL_PauseAudioDevice(device, 0);
        }

        private double dTime = 0.0;
        private double dAmpSlider = 0.5;
        private double dAmplitude = 0.5;
        private const double dSmoothingFactor = 0.0001;

        private unsafe void MyAudioCallback(IntPtr userdata, IntPtr stream, int streamLength)
        {
            for (uint i = 0; i < streamLength / 2; ++i)
            {
                if (dAmplitude < dAmpSlider - dSmoothingFactor || dAmplitude > dAmpSlider + dSmoothingFactor)
                    dAmplitude += dAmplitude < dAmpSlider ? dSmoothingFactor : -dSmoothingFactor;

                ((short*)stream)[i] = checked((short)(Math.Sin(440.0 * 6.28318530 * dTime) * 32767.0 * dAmplitude));
                dTime += 1.0 / 41000.0;
            }
        }

        private void AmpSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SDL.SDL_LockAudioDevice(device);
            dAmpSlider = e.NewValue;
            SDL.SDL_UnlockAudioDevice(device);
        }
    }
}