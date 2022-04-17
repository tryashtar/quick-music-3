﻿using Microsoft.Win32;
using NAudio.Wave;
using QuickMusic3.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace QuickMusic3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool playdragging;
        public bool PlayDragging
        {
            get { return playdragging; }
            set { playdragging = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayDragging))); }
        }

        private MainViewModel Model => (MainViewModel)DataContext;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
                Model.Player.OpenFiles(dialog.FileNames);
        }

        private void TimeBar_MouseDown(object sender, MouseEventArgs e)
        {
            PlayDragging = Model.Player.PlayState == PlaybackState.Playing;
            Model.Player.Pause();
        }

        private void TimeBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (PlayDragging)
                Model.Player.Play();
            PlayDragging = false;
        }

        private void Volume_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta != 0)
            {
                float volume = Model.Player.Volume + (1 / ((float)e.Delta / 3));
                Model.Player.Volume = Math.Clamp(volume, 0, 1);
            }
        }
    }
}
