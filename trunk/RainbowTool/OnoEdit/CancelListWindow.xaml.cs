﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OnoEdit
{
    /// <summary>
    /// Interaction logic for CancelListWindow.xaml
    /// </summary>
    public partial class CancelListWindow : Window
    {
        public CancelListWindow()
        {
            InitializeComponent();
            if (!UserSettings.CurrentSettings.WindowCollection.ContainsKey(Name))
                UserSettings.CurrentSettings.WindowCollection.Add(Name, new TypeSettings());
            else
            {
                Left = UserSettings.CurrentSettings.WindowCollection[Name].ThisLocation.X;
                Top = UserSettings.CurrentSettings.WindowCollection[Name].ThisLocation.Y;

                Width = UserSettings.CurrentSettings.WindowCollection[Name].ThisSize.Width;
                Height = UserSettings.CurrentSettings.WindowCollection[Name].ThisSize.Height;
            }
            this.Title = Title + " - " + MainWindow.Opened;
        }
        private void AddCancelList(object sender, RoutedEventArgs e)
        {
            var tmp = new RainbowLib.BCM.CancelList();
            tmp.Name = "NEW";
            App.OpenedFiles.BCMFile.CancelLists.Add(tmp);
        }

        private void RemoveCancelList(object sender, RoutedEventArgs e)
        {
            App.OpenedFiles.BCMFile.CancelLists.Remove(ListBox.SelectedValue as RainbowLib.BCM.CancelList);

        }
        private void AddMove(object sender, RoutedEventArgs e)
        {
            (ListBox.SelectedValue as RainbowLib.BCM.CancelList).Moves.Add(App.OpenedFiles.BCMFile.Moves[0]);
        }

        private void RemoveMove(object sender, RoutedEventArgs e)
        {
            (ListBox.SelectedValue as RainbowLib.BCM.CancelList).Moves.Remove(
                (Grid.SelectedValue as RainbowLib.Reference<RainbowLib.BCM.Move>));
        }

        private void WindowLocationChanged(object sender, EventArgs e)
        {
            UserSettings.CurrentSettings.WindowCollection[Name].ThisLocation = new Point(Left, Top);
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UserSettings.CurrentSettings.WindowCollection[Name].ThisSize = e.NewSize;
        }

        private void WindowStateChanged(object sender, EventArgs e)
        {
            UserSettings.CurrentSettings.WindowCollection[Name].IsMaximized = WindowState.HasFlag(System.Windows.WindowState.Maximized);

            if (!UserSettings.CurrentSettings.WindowCollection[Name].IsMaximized) return;
            UserSettings.CurrentSettings.WindowCollection[Name].ThisLocation = RestoreBounds.Location;
            UserSettings.CurrentSettings.WindowCollection[Name].ThisSize = RestoreBounds.Size;
        }


    }
}