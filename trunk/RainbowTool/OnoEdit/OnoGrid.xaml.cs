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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;

namespace OnoEdit
{
    /// <summary>
    /// Interaction logic for OnoGrid.xaml
    /// </summary>
    public partial class OnoGrid : UserControl
    {
        private object copy;
        public OnoGrid()
        {
            InitializeComponent();
        }


        public int FrozenColumns
        {
            get { return (int)GetValue(FrozenColumnsProperty); }
            set { SetValue(FrozenColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FrozenColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FrozenColumnsProperty =
            DependencyProperty.Register("FrozenColumns", typeof(int), typeof(OnoGrid), new UIPropertyMetadata(0));



        private void ColumnGeneration(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            //Change to a fixed width font
            myDataGrid.FontFamily = new FontFamily("Consolas");

            /*
             * Comboboxes 
             */
            //CancelLists
            if (e.PropertyType == typeof(RainbowLib.BCM.CancelList))
            {
                var col = new DataGridComboBoxColumn();
                col.ItemsSource = App.OpenedFiles.BCMFile.CancelLists;
                col.SelectedItemBinding = new Binding(e.PropertyName);
                col.DisplayMemberPath = "Name";
                col.Header = "Cancel";
                e.Column = col;
            }
            if (e.PropertyType == typeof(RainbowLib.BCM.InputMotion))
            {
                var col = new DataGridComboBoxColumn();
                col.ItemsSource = App.OpenedFiles.BCMFile.InputMotions;
                col.SelectedItemBinding = new Binding(e.PropertyName);
                col.DisplayMemberPath = "Name";
                col.Header = "InputMotion";
                e.Column = col;
            }
            //Scripts
            if (e.PropertyType == typeof(RainbowLib.BAC.Script))
            {
                var col = new DataGridComboBoxColumn();
                col.ItemsSource = App.OpenedFiles.BACFile.Scripts;
                col.SelectedItemBinding = new Binding(e.PropertyName);
                col.DisplayMemberPath = "Name";
                col.Header = "Script";
                e.Column = col;
            }
            //Hex display and entry of Unknown params
            if (e.PropertyName.Contains("Unknown") && e.PropertyType != typeof(float))
            {
                var col = new DataGridTextColumn();
                //col.ItemsSource = Enum.GetValues(e.PropertyType);
                var b = new Binding(e.PropertyName);
                col.Binding = b;
                b.Converter = new HexConverter();
                //col.DisplayMemberPath = "Name";
                col.Header = e.PropertyName.Replace("Unknown", "Unk");
                e.Column = col;
            }
            //Raw Enum Display
            if (e.PropertyType.BaseType == typeof(Enum) && RawCheckbox.IsChecked.Value)
            {
                var col = new DataGridTextColumn();
                //col.ItemsSource = Enum.GetValues(e.PropertyType);
                var b = new Binding(e.PropertyName);
                col.Binding = b;
                b.StringFormat = "X";
                //b.Converter = new HexConverter();
                //col.DisplayMemberPath = "Name";
                col.Header = e.PropertyName;
                e.Column = col;
            }
            //Enum Dispaly (ComboBox)
            else
                if (e.PropertyType.BaseType == typeof(Enum) && !Attribute.IsDefined(e.PropertyType, typeof(FlagsAttribute)))
                {
                    var col = new DataGridComboBoxColumn();
                    col.ItemsSource = Enum.GetValues(e.PropertyType);
                    col.SelectedItemBinding = new Binding(e.PropertyName);
                    col.Header = e.PropertyName;
                    e.Column = col;
                }// Enum Display (Checkboxes) for flags
                else if (e.PropertyType.BaseType == typeof(Enum))
                {
                    var col = new DataGridTemplateColumn();
                    col.CellTemplateSelector = new EnumTemplateGenerator(e.PropertyType, e.PropertyName, false);
                    col.CellEditingTemplateSelector = new EnumTemplateGenerator(e.PropertyType, e.PropertyName, true);
                    col.Header = e.PropertyName;
                    e.Column = col;
                }
            if (e.PropertyName == "Raw" || e.PropertyName == "ScriptIndex")
                e.Cancel = true;
            if (e.PropertyName == "StartFrame")
            {
                e.Column.Header = "S";
                e.Column.DisplayIndex = 0;
            }
            if (e.PropertyName == "EndFrame")
            {
                e.Column.Header = "E";
                e.Column.DisplayIndex = 1;
            }
            if (e.PropertyName == "Animation" && !RawCheckbox.IsChecked.Value)
            {
                var column = new DataGridTemplateColumn();
                column.CellTemplate = App.Current.Resources["AnimationViewTemplate"] as DataTemplate;
                column.CellEditingTemplate = App.Current.Resources["AnimationEditTemplate"] as DataTemplate;
                column.Header = e.PropertyName;
                e.Column = column;

            }
            if (e.PropertyName == "HitGFX" && !RawCheckbox.IsChecked.Value)
            {
                var c = new DataGridComboBoxColumn();
                var b = new Binding(e.PropertyName);

                b.Converter = new IndexReferenceTypeConverter();
                var data = RainbowLib.ResourceManager.Load("VFX2");
                b.ConverterParameter = data;
                c.ItemsSource = data.Values;
                c.Header = e.PropertyName;
                //c.SelectedValuePath = "Value";
                c.DisplayMemberPath = "Name";
                c.SelectedValueBinding = b;
                e.Column = c;
            }
            if (e.PropertyName == "ShortParam" && !RawCheckbox.IsChecked.Value)
            {
                var column = new DataGridTemplateColumn();
                column.CellTemplateSelector = new EtcTemplateSelector(false);
                column.CellEditingTemplateSelector = new EtcTemplateSelector(true);
                column.Header = e.PropertyName;
                e.Column = column;
            }
            // start anotak
            if (e.PropertyName == "RawString")
            {
                // this hurt my braaaaaaiinnnsss
                var col = new DataGridTextColumn();
                var b = new Binding(e.PropertyName);
                col.Binding = b;
                col.EditingElementStyle = (Style)Application.Current.Resources["RawStringBox"];
                if (col.EditingElementStyle == null)
                    throw new Exception("rawstring style problem");
                col.Header = "RawString";
                e.Column = col;
            }
            // end anotak

        }

        private void ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        private void DuplicateCommand(object sender, RoutedEventArgs e)
        {
            ListCollectionView lc = myDataGrid.ItemsSource as ListCollectionView;
            var current = lc.CurrentItem;
            var clone = RainbowLib.Cloner.Clone(current);
            var list = lc.SourceCollection as System.Collections.IList;
            var index = list.IndexOf(current);
            list.Insert(index+1, clone);

        }

        private void RemoveCommand(object sender, RoutedEventArgs e)
        {
            ListCollectionView lc = myDataGrid.ItemsSource as ListCollectionView;
            if (lc.IsAddingNew)
                lc.CancelNew();
            else
            {
                lc.CommitEdit();
                lc.Remove(lc.CurrentItem);
            }
        }

        private void MoveToTopCommand(object sender, RoutedEventArgs e)
        {
            ListCollectionView lc = myDataGrid.ItemsSource as ListCollectionView;
            var current = lc.CurrentItem;
            var list = lc.SourceCollection as System.Collections.IList;
            list.Remove(current);
            list.Insert(0, current);
            lc.MoveCurrentTo(current);
            myDataGrid.ScrollIntoView(current);
        }

        private void MoveDownCommand(object sender, RoutedEventArgs e)
        {
            ListCollectionView lc = myDataGrid.ItemsSource as ListCollectionView;
            var current = lc.CurrentItem;
            var list = lc.SourceCollection as System.Collections.IList;
            var index = list.IndexOf(current);
            list.Remove(current);
            list.Insert(index + 1, current);
            lc.MoveCurrentTo(current);
            myDataGrid.ScrollIntoView(current);
        }

        private void MoveUpCommand(object sender, RoutedEventArgs e)
        {
            ListCollectionView lc = myDataGrid.ItemsSource as ListCollectionView;
            var current = lc.CurrentItem;
            var list = lc.SourceCollection as System.Collections.IList;
            var index = list.IndexOf(current);
            list.Remove(current);
            list.Insert(Math.Max(0, index - 1), current);
            lc.MoveCurrentTo(current);
            myDataGrid.ScrollIntoView(current);
        }

        private void MoveToBottomCommand(object sender, RoutedEventArgs e)
        {
            ListCollectionView lc = myDataGrid.ItemsSource as ListCollectionView;
            var current = lc.CurrentItem;
            var list = lc.SourceCollection as System.Collections.IList;
            var index = list.IndexOf(current);
            list.Remove(current);
            list.Add(current);
            lc.MoveCurrentTo(current);
            myDataGrid.ScrollIntoView(current);
        }

        private void AddCommand(object sender, RoutedEventArgs e)
        {
            ListCollectionView lc = myDataGrid.ItemsSource as ListCollectionView;
            myDataGrid.ScrollIntoView(lc.AddNew());
        }

        private void myDataGrid_StylusEnter(object sender, StylusEventArgs e)
        {

        }

        private void myDataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            myDataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            //myDataGrid.CancelEdit();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //myDataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            //myDataGrid.SelectedItem = null;
        }

        private void myDataGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            //myDataGrid.CommitEdit();
        }

        private void RawCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            myDataGrid.AutoGenerateColumns = false;
            myDataGrid.AutoGenerateColumns = true;
        }

        private void PasteCommand(object sender, RoutedEventArgs e)
        {
            if (copy == null)
                return;
            ListCollectionView lc = myDataGrid.ItemsSource as ListCollectionView;
            var current = lc.CurrentItem;
            var clone = RainbowLib.Cloner.Clone(copy);
            var list = lc.SourceCollection as System.Collections.IList;
            var index = list.IndexOf(current);
            try
            {
                list.Insert(index+1, clone);
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot paste that here!");
            }

        }

        private void CopyCommand(object sender, RoutedEventArgs e)
        {
            ListCollectionView lc = myDataGrid.ItemsSource as ListCollectionView;
            var current = lc.CurrentItem;
            copy = current;
        }


    }
}
