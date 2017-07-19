using ceqalib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using Button = System.Windows.Controls.Button;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace WpfSamples
{
    /// <summary>
    ///     Interaction logic for WinMail.xaml
    /// </summary>
    public partial class WinMail : Window
    {
        private readonly List<string> _attachmentsList = new List<string>();
        private readonly string _defaultPath;
        private readonly Window _owner;

        public WinMail()
        {
            InitializeComponent();
            KeysCollection = new ObservableCollection<string>();
            DataContext = this;
            WrapPanelAttachments.Children.Clear();
        }

        public WinMail(string defaultPath, Window owner) : this()
        {
            _defaultPath = defaultPath;
            _owner = owner;
            UpdateMailConfigColletion();
            if (!string.IsNullOrEmpty(_defaultPath) && Directory.Exists(_defaultPath))
            {
                var sArray = Directory.GetFiles(_defaultPath);
                _attachmentsList.AddRange(sArray);
            }
            foreach (var at in _attachmentsList)
                AddButtonToWrapPannel(at);
            //TbAttachments.Text = sArray.Aggregate("", (current, s) => current + (s.Substring(s.LastIndexOf('\\')+1) + ";"));
        }

        public ObservableCollection<string> KeysCollection { get; set; }

        private void AddButtonToWrapPannel(string content)
        {
            var bt = new Button();
            var ct = TryFindResource("ButtonWithText") as ControlTemplate;
            bt.Template = ct ?? bt.Template;
            bt.Content = content.Substring(content.LastIndexOf('\\') + 1);
            bt.ToolTip = content;
            bt.Click += (sender, e) =>
            {
                var btn = sender as Button;
                if (btn != null && WrapPanelAttachments.Children.Contains(btn))
                {
                    WrapPanelAttachments.Children.Remove(btn);
                    if ((string) btn.ToolTip != null && _attachmentsList.Contains(content))
                        _attachmentsList.Remove(content);
                }
            };
            WrapPanelAttachments.Children.Add(bt);
        }

        private void TbAttachmentsSelect_Click(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();
            fd.InitialDirectory = Directory.Exists(_defaultPath) ? _defaultPath : Directory.GetCurrentDirectory();
            fd.Multiselect = true;
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var fileNames = fd.FileNames.Aggregate("", (current, file) => current + file + ";");
                TbAttachments.Text = fileNames.Length != 0 ? fileNames.Remove(fileNames.Length - 1) : fileNames;
            }
        }

        private void TbAttachmentsAdd_Click(object sender, RoutedEventArgs e)
        {
            var filespath = TbAttachments.Text;
            foreach (var file in filespath.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries))
                if (File.Exists(file) && !_attachmentsList.Contains(file))
                {
                    _attachmentsList.Add(file);
                    AddButtonToWrapPannel(file);
                }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            var result = false;
            var message = "";
            try
            {
                var mm = new MailManager();
                mm.AddressFrom = ReplacePattern(TbEmailFrom.Text);
                mm.Address = ReplacePattern(TbEmailTo.Text);
                mm.Subject = ReplacePattern(TbSubject.Text);
                var tr = new TextRange(TbMessageBody.Document.ContentStart, TbMessageBody.Document.ContentEnd);
                mm.MessageBody = tr.Text;
                var ccList = ReplacePattern(TbCc.Text).Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var c in ccList)
                    mm.AddCc(c);
                var bccList = ReplacePattern(TbBcc.Text).Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var bc in bccList)
                    mm.AddCc(bc);
                foreach (var ac in _attachmentsList)
                    mm.AddAttachmentFile(ac);
                result = mm.SendMail();
            }
            catch (Exception ex)
            {
                result = false;
                message = ex.Message;
            }
            if (result)
            {
                MessageBox.Show("Mail Sent Success");
                Close();
            }
            else
            {
                MessageBox.Show("Mail Sent Failed" + Environment.NewLine + message, "Message", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        internal string ReplacePattern(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 3 || !(value.StartsWith("<") && value.EndsWith(">")))
                return value;
            var mw = _owner as MainWindow;
            if (mw == null) return value;
            var refValue = value.Substring(1, value.Length - 2);
            //Config cf=new Config();
            //if (cf.LoadDefault(mw.GetSolution()))
            //{
            //    string actualValue = cf.Value(refValue);
            //    return actualValue == string.Empty ? value : actualValue;
            //}

            return value;
        }

        private void UpdateMailConfigColletion()
        {
            var mw = _owner as MainWindow;
            if (mw == null) return;
            //Config cf = new Config();
            //if (cf.LoadDefault(mw.GetSolution()))
            //{
            //    foreach (var data in cf.Data)
            //    {
            //        KeysCollection.Add(data.Key);
            //    }              
            //}
        }

        private void MailConfigSelection_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfigPopup.IsOpen = false;
                var lb = sender as ListBox;
                if (lb == null) return;

                var mailConfig = lb.SelectedItem.ToString();

                //Popup pp = (lb.Parent as Grid).Parent as Popup;
                var tb = ConfigPopup.PlacementTarget as TextBox;
                var i = tb.CaretIndex;
                tb.Text = tb.Text.Insert(i, mailConfig) + ">";
                tb.CaretIndex = i + mailConfig.Length + 1;
                tb.Focus();
            }
            else if (e.Key == Key.Escape)
            {
                ConfigPopup.IsOpen = false;
            }
        }

        private void TextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.OemComma) return;

            var tbm = e.OriginalSource as TextBox;
            if (tbm.Text.EndsWith("<") && KeysCollection.Count != 0)
                ShowPopUp(tbm.GetRectFromCharacterIndex(tbm.CaretIndex), tbm);
        }

        private void ShowPopUp(Rect placementRect, TextBox tb)
        {
            ConfigPopup.PlacementTarget = tb;
            ConfigPopup.PlacementRectangle = placementRect;
            ConfigPopup.IsOpen = true;
            MailConfigSelection.Focus();
            MailConfigSelection.SelectedIndex = 0;
            var listBoxItem =
                (ListBoxItem) MailConfigSelection.ItemContainerGenerator.ContainerFromItem(MailConfigSelection
                    .SelectedItem);
            listBoxItem.Focus();
        }
    }

    public sealed class TextBoxWithMark : TextBox
    {
        public static readonly DependencyProperty WaterMarksProperty = DependencyProperty.Register(
            "WaterMarks", typeof(string), typeof(TextBoxWithMark), new PropertyMetadata(default(string)));

        public string WaterMarks
        {
            get => (string) GetValue(WaterMarksProperty);
            set => SetValue(WaterMarksProperty, value);
        }
    }

    public sealed class ReverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = false;
            if (value is bool)
            {
                flag = (bool) value;
            }
            else if (value is bool?)
            {
                var nullable = (bool?) value;
                flag = nullable.HasValue && nullable.Value;
            }
            return (Visibility) (!flag ? 0 : 2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
                return !((Visibility) value == Visibility.Visible ? true : false);
            return false;
        }
    }

    public sealed class Helper : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = false;
            if (value is int)
                flag = (int) value != 0 ? true : false;
            return (Visibility) (flag ? 0 : 2);
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}