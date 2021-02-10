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
using System.Windows.Threading;
using TagLib;

namespace MP3Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string title = "";
        string year;
        string[] artist;
        string album = "";
        IPicture coverArt;
        TagLib.File fileInfo;
        bool scrubbing = false;
        bool infoShown = false;
        bool editShown = false;
        public MainWindow()
        {
            InitializeComponent();
            editBox.saveInfo += new EventHandler(SaveBtn_Clicked);
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            playingInfo.Visibility = Visibility.Hidden;
            editBox.Visibility = Visibility.Hidden;
        }

        private void SaveBtn_Clicked(object sender, EventArgs e)
        {
            try{
                if(fileInfo != null)
                {
                    string[] artistToinput = { editBox.artistTxt.Text };
                    fileInfo.Tag.Title = editBox.titleTxt.Text;
                    fileInfo.Tag.Album = editBox.albumTxt.Text;
                    fileInfo.Tag.AlbumArtists = artistToinput;
                    fileInfo.Tag.Year = (uint)Int32.Parse(editBox.yearTxt.Text);
                    fileInfo.Save();
                    editBox.Visibility = Visibility.Hidden;
                }
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void timer_Tick(object sender, EventArgs e)
        {
            if ((mediaPlayer.Source != null) && (mediaPlayer.NaturalDuration.HasTimeSpan) && (!scrubbing))
            {
                ProgressSlider.Minimum = 0;
                ProgressSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                ProgressSlider.Value = mediaPlayer.Position.TotalSeconds;
            }
        }

        public void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// Get the information about the file that is being opening and saving the information for use later
        /// </summary>
        public void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //file path that will be used for TagLib
            string fileName = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //Only files with a mathcing file type will be able to be selected
            openFileDialog.Filter = "Media files (*.mp3;*.mpg;*.mpeg;*.mp4)|*.mp3;*.mpg;*.mpeg;*.mp4|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                mediaPlayer.Source = new Uri(openFileDialog.FileName);
            fileName = openFileDialog.FileName;

            //Creating the TabLib file
            try
            {
                fileInfo = TagLib.File.Create(path: fileName);
                // Getting the title,album,artist,year and coverArt
                title = fileInfo.Tag.Title;
                album = fileInfo.Tag.Album;
                artist = fileInfo.Tag.AlbumArtists;
                year = (fileInfo.Tag.Year).ToString();
                coverArt = fileInfo.Tag.Pictures[0];

                // how to get an image: https://stackoverflow.com/questions/17904184/using-taglib-to-display-the-cover-art-in-a-image-box-in-wpf
                MemoryStream ms = new MemoryStream(coverArt.Data.Data);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                //Displaying the songs image
                albumArt.Source = bitmap;
            }
            catch (TagLib.UnsupportedFormatException ex)
            {
                Console.WriteLine(ex);
            }
        }

        // The play button can be used
        public void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // when the play button is the music will start
        public void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Play();
        }

        // The pause button can be used
        public void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // when the pause button is the music will pause
        public void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        // The pause button can be used
        public void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // when the stop button is the music will stop and go back to start
        public void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        /// <summary>
        /// Display menu that allows the user to edit the info of the selected file
        /// </summary>
        public void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            // on first click the menu will display
            if (editShown == false)
            {
                editShown = true;
                editBox.Visibility = Visibility.Visible;
            }
            // if the menu is already displayed it will hide the menu
            else
            {
                editShown = false;
                editBox.Visibility = Visibility.Hidden;
            }
            //fill in the menu with the files information
            try
            {
                if (title == null || artist == null || album == null || year == null)
                {
                    editBox.yearTxt.Text = "";
                    editBox.titleTxt.Text = "";
                    editBox.albumTxt.Text = "";
                    editBox.artistTxt.Text = "";
                }
                else
                {
                    editBox.yearTxt.Text = year;
                    editBox.titleTxt.Text = title;
                    editBox.albumTxt.Text = album;
                    editBox.artistTxt.Text = artist[0];
                }
            }
            catch(NullReferenceException error)
            {
                Console.WriteLine(error);
            }
        }
        
        //update the time text based on the value from the slider
        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            time.Text = TimeSpan.FromSeconds(ProgressSlider.Value).ToString(@"hh\:mm\:ss");
        }

        // that users is dragging the slider to move around in the song
        private void ProgressSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            scrubbing = true;
        }

        // that users is done dragging the slider
        // moves the song to the time that the user scrubbed too
        private void ProgressSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            scrubbing = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
        }


        private void InfoBtn_Click(object sender, RoutedEventArgs e)
        {
            // on first click the menu will display
            if (infoShown == false)
            {
                infoShown = true;
                playingInfo.Visibility = Visibility.Visible;
            }
            // if the menu is already displayed it will hide the menu
            else
            {
                infoShown = false;
                playingInfo.Visibility = Visibility.Hidden;
            }
            //fill in the menu with the files information
            try
            {
                if (title == null || artist == null || album == null || year == null)
                {
                    playingInfo.titleTxt.Text = "";
                    playingInfo.albumTxt.Text = "";
                    playingInfo.artistYearTxt.Text = "";
                }
                else
                {
                    playingInfo.titleTxt.Text = title;
                    playingInfo.albumTxt.Text = album;
                    playingInfo.artistYearTxt.Text = artist[0] + " (" + year + ")";
                }
            }
            catch(NullReferenceException ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Close the program when the exit is clicked
        /// </summary>
        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}

// Resources used
// taglib documentation https://github.com/mono/taglib-sharp
// Getting song image https://stackoverflow.com/questions/17904184/using-taglib-to-display-the-cover-art-in-a-image-box-in-wpf
// How to create a media player https://www.wpf-tutorial.com/audio-video/how-to-creating-a-complete-audio-video-player/
