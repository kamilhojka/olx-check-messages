using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace OLXChecker
{
    public partial class MessageBoxCustom : Window
    {
        public MessageBoxCustom(string Message, MessageType Type, MessageButtons Buttons)
        {
            InitializeComponent();
            MessageTextBlock.Text = Message;
            switch (Type)
            {
                case MessageType.Info:
                    {
                        TitleTextBlock.Text = "Informacja";
                    }
                    break;
                case MessageType.Confirmation:
                    {
                        TitleTextBlock.Text = "Komunikat";
                    }
                    break;
                case MessageType.Success:
                    {
                        TitleTextBlock.Text = "Sukces";
                    }
                    break;
                case MessageType.Warning:
                    {
                        TitleTextBlock.Text = "Uwaga";
                    }
                    break;
                case MessageType.Error:
                    {
                        TitleTextBlock.Text = "Błąd";
                    }
                    break;
            }
            switch (Buttons)
            {
                case MessageButtons.OkCancel:
                    YesButton.Visibility = Visibility.Collapsed; NoButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageButtons.YesNo:
                    OkButton.Visibility = Visibility.Collapsed; CancelButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageButtons.Ok:
                    OkButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Collapsed;
                    YesButton.Visibility = Visibility.Collapsed; NoButton.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.SizeAll;
            try
            {
                base.OnMouseLeftButtonDown(e);
                DragMove();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        private void ClickOk(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        private void ClickYes(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        private void ClickNo(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void ClickCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;
                    DialogResult = true;
                    Close();
                    break;
                case Key.Escape:
                    e.Handled = true;
                    DialogResult = false;
                    Close();
                    break;
                default:
                    return;
            }
        }
    }
    public enum MessageType
    {
        Info,
        Confirmation,
        Success,
        Warning,
        Error,
    }
    public enum MessageButtons
    {
        OkCancel,
        YesNo,
        Ok,
    }
}
