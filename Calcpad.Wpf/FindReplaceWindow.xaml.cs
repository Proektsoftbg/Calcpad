using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            SearchCombo.IsTextSearchCaseSensitive = true;
            ReplaceCombo.IsTextSearchCaseSensitive = true;
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
            if (ReferenceEquals(tabItem, FindTabItem))
                SearchCombo.Focus();
            else if (ReferenceEquals(tabItem, ReplaceTabItem))
                ReplaceCombo.Focus();

            SetButtons((!SelectionCheckbox.IsChecked ?? false) || FindReplaceTabControl.SelectedIndex == 0);
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
            FindReplace.Mode = (FindReplace.Modes)FindReplaceTabControl.SelectedIndex;
            if (FindReplaceTabControl.SelectedIndex == 1)
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
            FindReplaceTabControl.SelectedIndex = (int)FindReplace.Mode;
            SelectionCheckbox.IsChecked =
                FindReplace.Mode == FindReplace.Modes.Replace &&
                FindReplace.Selection &&
                SelectionCheckbox.IsEnabled;
            SearchList = FindReplace.SearchList;
            ReplaceList = FindReplace.ReplaceList;
            SearchCombo.Text = FindReplace.SearchString;
            ReplaceCombo.Text = FindReplace.ReplaceString;
            FindReplace.InitPosition();
            FindReplace.HighlightSelection();   
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
                FindButton_Click(null, null);
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.F)
                    FindReplaceTabControl.SelectedIndex = 0;
                else if (e.Key == Key.H)
                    FindReplaceTabControl.SelectedIndex = 1;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            SetFindReplaceOptions();
            FindReplace.ClearSelection();
        }

        private void SelectionCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            SetButtons(FindReplaceTabControl.SelectedIndex == 0);
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

        private void SearchCombo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && FindReplaceTabControl.SelectedIndex == 1 && !IsShiftDown())
            {
                e.Handled = true;
                ReplaceCombo.Focus();
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            FindButton.Focus();
            Dispatcher.InvokeAsync(() => SearchCombo.Focus(), DispatcherPriority.ApplicationIdle);
        }

        private void ReplaceCombo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
                if (IsShiftDown())
                    SearchCombo.Focus();
                else
                    DirectionCombo.Focus();
            }
        }

        private void WholeWordsCheckbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && FindReplaceTabControl.SelectedIndex == 1 && !IsShiftDown())
            {
                e.Handled = true;
                ReplaceButton.Focus();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                FindReplaceTabControl.SelectedIndex = 1 - FindReplaceTabControl.SelectedIndex;
            }
        }

        private static bool IsShiftDown() => 
            (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
    }
}
