/*
 * This code is copyright of the CCHS Byte Club
 */

using NAudio.Wave;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace JukeBox
{
    /// <summary>
    /// Controls a super fancy jukebox
    /// Copyright (c) 2018 CCHS Byte Club
    /// </summary>
    public partial class MainWindow : Window
    {
        // Audio stuff
        private WaveOut outputDevice = null;
        private AudioFileReader audioFileReader = null;

        // Tells wether when the song is switched, it should be handled automatically or not
        private bool handleSongSwitch = true;

        private const string SONG_TOOLTIP = "Use arrow F1, F2, and F3 to move around que";

        private const string PAUSE_TEXT = "| |";
        private const string PLAY_TEXT = "▶";

        private float volume;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Init()
        {
            // Make sure there is an audio directory, and create one if there is none
            Directory.CreateDirectory("audio/");
            string[] files = Directory.GetFiles("audio/", "*.mp3", SearchOption.TopDirectoryOnly);

            // Parse each file into a usable audio files

            int l1 = "audio/".Length;
            int l2 = ".mp3".Length;

            foreach (string file in files)
            {
                LstSong.Items.Add(new ListBoxItem()
                {
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    Content = file.Substring(l1, file.Length - (l1 + l2)),
                    ToolTip = SONG_TOOLTIP,
                });
            }

            if(LstSong.Items.Count != 0)
                TxtSongTitle.Text = (string) ((ListBoxItem)LstSong.Items.GetItemAt(0)).Content;

            // The timer will update the progress bar
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(166666); // 1 tick = 100 nanoseconds. 166,666 is approx 60 times per second
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (audioFileReader != null)
                SldPosition.Value = audioFileReader.Position;
            else
                SldPosition.Value = 0;
        }

        /// <summary>
        /// Puts the song that is at the top of the list at the bottom, and shifts the rest of the list up one, but doesn't play it
        /// </summary>
        private void NextSong()
        {
            if (LstSong.Items.Count != 0)
            {
                string content = (string)((ListBoxItem)LstSong.Items.GetItemAt(0)).Content;
                for (int i = 0; i < LstSong.Items.Count - 1; i++)
                {
                    ((ListBoxItem)LstSong.Items.GetItemAt(i)).Content = ((ListBoxItem)LstSong.Items.GetItemAt(i + 1)).Content;
                }

                ((ListBoxItem)LstSong.Items.GetItemAt(LstSong.Items.Count - 1)).Content = content;
            }
        }

        /// <summary>
        /// Plays the song at the top of the song list if there is no current song playing, and cycles the list to the next song
        /// </summary>
        private void PlaySong()
        {
            if (outputDevice == null && LstSong.Items.Count != 0)
            {
                ListBoxItem item = (ListBoxItem)LstSong.Items.GetItemAt(0);
                string content = (string)item.Content;

                string fileName = "audio/" + content + ".mp3";
                audioFileReader = new AudioFileReader(fileName);
                outputDevice = new WaveOut();
                outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;

                outputDevice.Init(audioFileReader);
                outputDevice.Volume = volume;
                outputDevice.Play();

                SldPosition.Value = 0;
                SldPosition.Maximum = (double)audioFileReader.Length;
                BtnPlay.Content = PAUSE_TEXT;
                TxtSongTitle.Text = content;
            }

            NextSong();
        }

        /// <summary>
        /// Whenever the song stops playing
        /// If handleSongSwitch is false, nothing will happen except for handleSongSwitch being set back to true
        /// </summary>
        private void OutputDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (handleSongSwitch) // If this is false, the song switching is being handled elsewhere.
            {
                EndSong();

                PlaySong();
            }

            handleSongSwitch = true;
        }

        /// <summary>
        /// Stops the current song if there is one
        /// </summary>
        private void EndSong()
        {
            if (outputDevice != null)
            {
                outputDevice.Stop();
                outputDevice.Dispose();
                outputDevice = null;
                audioFileReader.Dispose();
                audioFileReader = null;
            }
        }

        /// <summary>
        /// Sets the volume of the player
        /// </summary>
        /// <param name="vol">A number between 0.0f and 1.0f inclusive</param>
        private void SetVolume(float vol)
        {
            if(TxtVolume != null)
                TxtVolume.Text = (int)Math.Round((vol * 100)) + "%";

            volume = vol;

            if (outputDevice != null)
            {
                outputDevice.Volume = vol;
            }
        }

        /// <summary>
        /// Treats it like the title bar of any normal window
        /// </summary>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                AdjustWindow();
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            AdjustWindow();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Maximises or normalizes window based on its current state
        /// </summary>
        private void AdjustWindow()
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// Plays or pauses the current song
        /// </summary>
        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            string text = (string)BtnPlay.Content;
            if (text.Equals(PAUSE_TEXT)) // This is the pause symbol
            {
                if (outputDevice != null)
                    outputDevice.Pause();

                BtnPlay.Content = PLAY_TEXT;
            }
            else
            {
                if (outputDevice != null)
                    outputDevice.Play();
                else
                    PlaySong();

                BtnPlay.Content = PAUSE_TEXT;
            }
        }
        
        /// <summary>
        /// Ends the current song and starts the next one
        /// </summary>
        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            handleSongSwitch = false;
            EndSong();
            PlaySong();
        }

        /// <summary>
        /// Resets the audio progress to 0, and pauses the song
        /// </summary>
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (outputDevice != null)
            {
                handleSongSwitch = false;
                outputDevice.Stop();
                audioFileReader.Position = 0;

                BtnPlay.Content = PLAY_TEXT;
            }
        }
        
        /// <summary>
        /// Changes where in the song the player is at
        /// </summary>
        private void SldPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (audioFileReader != null)
            {
                audioFileReader.Position = (long)e.NewValue;
            }
        }

        /// <summary>
        /// Sets the volume of the player
        /// </summary>
        private void SldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetVolume((float)e.NewValue / 100.0f);
        }

        /// <summary>
        /// Handles sliding things in the list up or down and all the jazz
        /// </summary>
        private void LstSong_KeyDown(object sender, KeyEventArgs e)
        {
            int index = LstSong.SelectedIndex;

            if (LstSong.SelectedItem != null && index != -1 && LstSong.Items.Count > 1)
            {
                ListBoxItem selectedItem = (ListBoxItem)LstSong.SelectedItem;

                var title = selectedItem.Content;

                if (e.Key == Key.F1)
                {
                    // Shift whatever is selected up 1

                    if (index != 0)
                    {
                        ListBoxItem above = (ListBoxItem)LstSong.Items.GetItemAt(index - 1);
                        selectedItem.Content = above.Content;
                        above.Content = title;

                        LstSong.SelectedIndex = index - 1;
                    }
                }
                else if (e.Key == Key.F2)
                {
                    // Shift whatever is selected down 1
                    
                    if (index + 1 != LstSong.Items.Count)
                    {
                        ListBoxItem below = (ListBoxItem)LstSong.Items.GetItemAt(index + 1);
                        selectedItem.Content = below.Content;
                        below.Content = title;

                        LstSong.SelectedIndex = index + 1;
                    }
                }
                else if (e.Key == Key.F3)
                {
                    // Shift whatever is selected to the top of the list

                    for (int i = index; i >= 1; i--)
                    {
                        ListBoxItem next = (ListBoxItem)LstSong.Items.GetItemAt(i - 1);
                        ListBoxItem at = (ListBoxItem)LstSong.Items.GetItemAt(i);
                        var content = at.Content;
                        at.Content = next.Content;
                        next.Content = content;
                    }

                    ListBoxItem top = (ListBoxItem)LstSong.Items.GetItemAt(0);
                    top.Content = title;

                    LstSong.SelectedIndex = 0;
                }
            }
        }

        private void BtnUrl_Click(object sender, RoutedEventArgs e)
        {
            string url = Dialogue.Show();
            if(url != null)
            {

            }
        }
    }
}
