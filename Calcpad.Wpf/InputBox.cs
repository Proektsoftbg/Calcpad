using System.Windows;
using System.Windows.Controls;


namespace Calcpad.Wpf
{
    internal static class InputBox
    {
        internal static bool Show(string title, string promptText, ref string retValue)
        {
            var label = new Label()
            {
                Content = promptText,
                Height = 30,
                Margin = new Thickness(20, 10, 25, 100),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Bottom
            };
            var textBox = new TextBox()
            {
                Text = retValue,
                SelectionLength = retValue.Length,
                Height = 25,
                Margin = new Thickness(25, 0, 25, 65),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                VerticalContentAlignment = VerticalAlignment.Center,
                TabIndex = 0
            };
            var buttonOk = new Button()
            {
                Content = "OK",
                IsDefault = true,
                Width = 60,
                Height = 25,
                Margin = new Thickness(0, 0, 110, 20),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            buttonOk.Click += Button_Click;
            var buttonCancel = new Button()
            {
                Content = "Cancel",
                IsCancel = true,
                Width = 60,
                Height = 25,
                Margin = new Thickness(320, 100, 25, 20),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            buttonCancel.Click += Button_Click;
            var grid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            grid.Children.Add(label);
            grid.Children.Add(textBox);
            grid.Children.Add(buttonOk);
            grid.Children.Add(buttonCancel);
            var form = new Window()
            {
                Title = title,
                Width = 500,
                Height = 180,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = grid
            };
            textBox.Focus();
            var dialogResult = form.ShowDialog();
            retValue = textBox.Text;
            if (dialogResult.HasValue)
                return dialogResult.Value;

            return false;
        }
        private static void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var form = (Window)((Grid)button.Parent).Parent;
            form.DialogResult = button.IsDefault;
            form.Close();
        }
    }
}