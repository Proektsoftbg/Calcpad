﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Calcpad.Wpf
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class FindReplaceWindow : Window
    {
        internal FindReplace FindReplace { get; set; }
        public FindReplaceWindow()
        {
            InitializeComponent();
        }

        private List<string> SearchList
        {
            set
            {
                SearchCombo.Items.Clear();
                for (int i = value.Count - 1; i >= 0; --i)
                    SearchCombo.Items.Add(value[i]);
            }
        }

        private List<string> ReplaceList
        {
            set
            {
                ReplaceCombo.Items.Clear();
                for (int i = value.Count - 1; i >= 0; --i)
                    ReplaceCombo.Items.Add(value[i]);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is not TabControl tabControl)
                return;

            TabItem tabItem = (TabItem)tabControl.SelectedItem;
            if (ReferenceEquals(tabItem, tabControl.Items[0]))
                SearchCombo.Focus();
            else if (ReferenceEquals(tabItem, tabControl.Items[1]))
                ReplaceCombo.Focus();

            SetButtons((!SelectionCheckbox.IsChecked ?? false) || FindReplaceTab.SelectedIndex == 0);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    FindGrid.Focus();
                    SearchCombo.Focus();
                }),
                DispatcherPriority.ApplicationIdle
            );
        }

        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchCombo.Focus();
        }

        private void SetFindReplaceOptions()
        {
            FindReplace.SearchString = SearchCombo.Text;
            FindReplace.ReplaceString = ReplaceCombo.Text;
            FindReplace.MatchCase = MatchCaseCheckbox.IsChecked ?? false;
            FindReplace.WholeWords = WholeWordsCheckbox.IsChecked ?? false;
            FindReplace.Direction = (FindReplace.Directions)(DirectionCombo.SelectedIndex - 1);
            FindReplace.Mode = (FindReplace.Modes)FindReplaceTab.SelectedIndex;
            if (FindReplaceTab.SelectedIndex == 1)
                FindReplace.Selection = SelectionCheckbox.IsChecked ?? false;
            else
                FindReplace.Selection = false;

        }

        private void ReplaceAllButton_Click(object sender, RoutedEventArgs e)
        {
            SetFindReplaceOptions();
            FindReplace.ReplaceAll();
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            SetFindReplaceOptions();
            FindReplace.Replace();
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            SetFindReplaceOptions();
            FindReplace.Find();
            SelectionCheckbox.IsEnabled = FindReplace.SelectedText.Length > 5;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            MatchCaseCheckbox.IsChecked = FindReplace.MatchCase;
            WholeWordsCheckbox.IsChecked = FindReplace.WholeWords;
            DirectionCombo.SelectedIndex = (int)FindReplace.Direction + 1;
            FindReplaceTab.SelectedIndex = (int)FindReplace.Mode;
            SelectionCheckbox.IsChecked =
                FindReplace.Mode == FindReplace.Modes.Replace &&
                FindReplace.Selection &&
                SelectionCheckbox.IsEnabled;
            SearchList = FindReplace.SearchList;
            ReplaceList = FindReplace.ReplaceList;
            SearchCombo.Text = FindReplace.SearchString;
            ReplaceCombo.Text = FindReplace.ReplaceString;
            FindReplace.InitPosition();
            FindReplace.HighlghtSelection();
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F3)
                FindButton_Click(null, null);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            SetFindReplaceOptions();
            FindReplace.ClearSelection();
        }

        private void SelectionCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            SetButtons(false || FindReplaceTab.SelectedIndex == 0);
        }

        private void SelectionCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            SetButtons(true);
        }

        private void SetButtons(bool isEnabled)
        {
            FindButton.IsEnabled = isEnabled;
            ReplaceButton.IsEnabled = isEnabled;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Owner.Activate();
            Owner.Focus();
        }
    }
}
