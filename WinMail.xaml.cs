using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ceqalib;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;
using TextBox = System.Windows.Controls.TextBox;

namespace TestDataManager
{
    /// <summary>
    /// Interaction logic for WinMail.xaml
    /// </summary>
    public partial class WinMail : Window
    {
        private readonly string _defaultPath;
        private readonly List<string> _attachmentsList=new List<string>();
        private readonly Window _owner;
        public ObservableCollection<string> KeysCollection { get; set; }
        public WinMail()
        {
            InitializeComponent();
            KeysCollection=new ObservableCollection<string>();           
            this.DataContext = this;
            WrapPanelAttachments.Children.Clear();
        }

        public WinMail(string defaultPath,Window owner):this()
        {            
            _defaultPath = defaultPath;
            _owner = owner;
            UpdateMailConfigColletion();
            if (!string.IsNullOrEmpty(_defaultPath) && Directory.Exists(_defaultPath))
            {
                string[] sArray = Directory.GetFiles(_defaultPath);
                _attachmentsList.AddRange(sArray);
            }
            foreach (var at in _attachmentsList)
            {
                AddButtonToWrapPannel(at);
            }
            //TbAttachments.Text = sArray.Aggregate("", (current, s) => current + (s.Substring(s.LastIndexOf('\\')+1) + ";"));
        }

        private void AddButtonToWrapPannel(string content)
        {
            Button bt=new Button();
            ControlTemplate ct = TryFindResource("ButtonWithText") as ControlTemplate;
            bt.Template = ct ?? bt.Template;
            bt.Content = content.Substring(content.LastIndexOf('\\') + 1);
            bt.ToolTip = content;
            bt.Click+= (sender, e) =>
            {
                Button btn = sender as Button;
                if (btn != null&&WrapPanelAttachments.Children.Contains(btn))
                {
                    WrapPanelAttachments.Children.Remove(btn);
                    if ((string) btn.ToolTip != null && _attachmentsList.Contains(content))
                    {
                        _attachmentsList.Remove(content);
                    }
                }
            };
            WrapPanelAttachments.Children.Add(bt);
        }

        private void TbAttachmentsSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd=new OpenFileDialog();
            fd.InitialDirectory = Directory.Exists(_defaultPath) ? _defaultPath : Directory.GetCurrentDirectory();
            fd.Multiselect = true;
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileNames = fd.FileNames.Aggregate("", (current, file) => current + (file + ";"));
                TbAttachments.Text = fileNames.Length != 0 ? fileNames.Remove(fileNames.Length - 1) : fileNames;
            }
        }

        private void TbAttachmentsAdd_Click(object sender, RoutedEventArgs e)
        {
            string filespath = TbAttachments.Text;
            foreach (var file in filespath.Split(new string[]{";"},StringSplitOptions.RemoveEmptyEntries))
            {
                if (File.Exists(file)&&!_attachmentsList.Contains(file))
                {
                    _attachmentsList.Add(file);
                    AddButtonToWrapPannel(file);
                }
            }
            
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            bool result = false;
            string message = "";
            try
            {
                MailManager mm = new MailManager();
                mm.AddressFrom = ReplacePattern(TbEmailFrom.Text);
                mm.Address = ReplacePattern(TbEmailTo.Text);
                mm.Subject = ReplacePattern(TbSubject.Text);
                TextRange tr = new TextRange(TbMessageBody.Document.ContentStart, TbMessageBody.Document.ContentEnd);
                mm.MessageBody = tr.Text;
                string[] ccList = ReplacePattern(TbCc.Text).Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var c in ccList)
                {
                    mm.AddCc(c);
                }
                string[] bccList = ReplacePattern(TbBcc.Text).Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var bc in bccList)
                {
                    mm.AddCc(bc);
                }
                foreach (var ac in _attachmentsList)
                {
                    mm.AddAttachmentFile(ac);
                }
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
                this.Close();
            }
            else
            {
                MessageBox.Show("Mail Sent Failed" + Environment.NewLine + message, "Message", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        internal string ReplacePattern(string value)
        {
            if (string.IsNullOrEmpty(value) ||value.Length<3 || !(value.StartsWith("<") && value.EndsWith(">")))
            {
                return value;
            }
            MainWindow mw = _owner as MainWindow;
            if (mw == null) return value;
            string refValue = value.Substring(1, value.Length - 2);
            Config cf=new Config();
            if (cf.LoadDefault(mw.GetSolution()))
            {
                string actualValue = cf.Value(refValue);
                return actualValue == string.Empty ? value : actualValue;
            }

            return value;           
        }

        private void UpdateMailConfigColletion()
        {
            MainWindow mw = _owner as MainWindow;
            if (mw == null) return;
            Config cf = new Config();
            if (cf.LoadDefault(mw.GetSolution()))
            {
                foreach (var data in cf.Data)
                {
                    KeysCollection.Add(data.Key);
                }              
            }
        }

        private void MailConfigSelection_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfigPopup.IsOpen = false;
                System.Windows.Controls.ListBox lb = sender as System.Windows.Controls.ListBox;
                if (lb == null) return;

                string mailConfig = lb.SelectedItem.ToString();

                //Popup pp = (lb.Parent as Grid).Parent as Popup;
                TextBox tb = ConfigPopup.PlacementTarget as TextBox;
                int i = tb.CaretIndex;
                tb.Text = tb.Text.Insert(i, mailConfig) + ">";
                tb.CaretIndex = i + mailConfig.Length + 1;
                tb.Focus();
            }
            else if (e.Key == Key.Escape)
            {
                ConfigPopup.IsOpen = false;
            }
        }

        private void TextBox_OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.OemComma) return;

            TextBox tbm = e.OriginalSource as TextBox;
            if (tbm.Text.EndsWith("<") && KeysCollection.Count != 0)
            {
                //TextBox tb=tbm.
                ShowPopUp(tbm.GetRectFromCharacterIndex(tbm.CaretIndex), tbm);
            }
        }

        private void ShowPopUp(Rect placementRect, TextBox tb)
        {
            ConfigPopup.PlacementTarget = tb;
            ConfigPopup.PlacementRectangle = placementRect;
            ConfigPopup.IsOpen = true;
            MailConfigSelection.Focus();
            MailConfigSelection.SelectedIndex = 0;
            var listBoxItem = (ListBoxItem)MailConfigSelection.ItemContainerGenerator.ContainerFromItem(MailConfigSelection.SelectedItem);
            listBoxItem.Focus();
        }
    }

    public sealed class TextBoxWithMark : TextBox
    {
        public static readonly DependencyProperty WaterMarksProperty = DependencyProperty.Register(
            "WaterMarks", typeof(string), typeof(TextBoxWithMark), new PropertyMetadata(default(string)));

        public string WaterMarks
        {
            get
            {
                return (string)GetValue(WaterMarksProperty);
            }
            set
            {
                SetValue(WaterMarksProperty, value);
            }
        }

    }

    public sealed class ReverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool)
                flag = (bool)value;
            else if (value is bool?)
            {
                bool? nullable = (bool?)value;
                flag = nullable.HasValue && nullable.Value;
            }
            return (object)(Visibility)(!flag ? 0 : 2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
                return (object)!(bool)((Visibility)value == Visibility.Visible ? true : false);
            return (object)false;
        }
    }

    public sealed class Helper : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool flag = false;
            if (value is int)
            {
                flag = (int)value != 0 ? true : false;
            }
            return (Visibility)(flag ? 0 : 2);
        }
        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
