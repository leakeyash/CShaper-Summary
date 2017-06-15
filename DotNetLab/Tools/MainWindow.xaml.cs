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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            CommanLibrary.Xml.FieldsMapping fm = new CommanLibrary.Xml.FieldsMapping(TextBoxFilePath.Text);
            Dictionary<string, string> selectedDictionary;
            List<string> missList;
            TextBoxReplaced.Text = fm.ReplaceSpecificNodes(TextBoxOrigin.Text, TextBoxXpath.Text, TextBoxSymbolStart.Text,
                TextBoxSymbolEnd.Text, out selectedDictionary, out missList);
            TextBoxMisMatchs.Text = missList.Aggregate(string.Empty, (current, s) => current + (s + "\r\n"));
        }
    }
}
