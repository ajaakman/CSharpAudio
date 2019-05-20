using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using SDL2;

namespace CSharp_SDL2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SDL.SDL_AudioSpec spec = new SDL.SDL_AudioSpec();
        uint device = 0;

        public MainWindow()
        {
            InitializeComponent();

            if (SDL.SDL_Init(SDL.SDL_INIT_AUDIO) < 0)
                Console.WriteLine("SDL Failed to Initilize!!!");

            spec.channels = 1;
            spec.freq = 44100;
            spec.format = SDL.AUDIO_S16SYS;
            spec.samples = 2048 * 2;
            spec.callback = MyAudioCallback;

            device = SDL.SDL_OpenAudioDevice(null, 0, ref spec, out spec, 0);

            if (device == 0)
                Console.WriteLine("SDL Failed to open Audio Device!!!");
            
            SDL.SDL_PauseAudioDevice(device, 0);
        }

        double dTime = 0.0;
        double dAmpSlider = 0.5;
        double dAmplitude = 0.5;
        double dSmoothingFactor = 0.0001;

        unsafe void MyAudioCallback(IntPtr userdata, IntPtr stream, int streamLength)
        {
            for (uint i = 0; i < streamLength / 2; ++i)
            {
                if (dAmplitude < dAmpSlider - dSmoothingFactor || dAmplitude > dAmpSlider + dSmoothingFactor)
                {
                    if (dAmplitude < dAmpSlider)
                        dAmplitude += dSmoothingFactor;
                    else
                        dAmplitude -= dSmoothingFactor;
                }
                checked
                {
                    ((short*)stream)[i] = (short)(Math.Sin(440.0 * 6.28318530 * dTime) * 32767.0 * dAmplitude);
                }
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
