using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static CommonModels.Client.Models.SearchBotsModels.SearchBotEnums;

namespace DesktopWPFManagementApp.MVVM.View.CustomControls.Elements
{
    /// <summary>
    /// Логика взаимодействия для SearchBotsTextBoxUserControlView.xaml
    /// </summary>
    public partial class SearchBotsTextBoxUserControlView : UserControl
    {
        private static readonly FontFamily ArialFontFamily = new FontFamily("Arial");

        public ObservableCollection<KeyValuePair<string, FindSearchKeywordParametersBots>> ComboboxItemSourceList
        {
            get { return (ObservableCollection<KeyValuePair<string, FindSearchKeywordParametersBots>>)GetValue(ComboboxItemSourceListProperty); }
            set { SetValue(ComboboxItemSourceListProperty, value); }
        }

        public static readonly DependencyProperty ComboboxItemSourceListProperty =
            DependencyProperty.Register("ComboboxItemSourceList", typeof(ObservableCollection<KeyValuePair<string, FindSearchKeywordParametersBots>>), typeof(SearchBotsTextBoxUserControlView), new PropertyMetadata(new ObservableCollection<KeyValuePair<string, FindSearchKeywordParametersBots>>()));

        public KeyValuePair<string, FindSearchKeywordParametersBots> ComboboxSelectedItem
        {
            get { return (KeyValuePair<string, FindSearchKeywordParametersBots>)GetValue(ComboboxSelectedItemProperty); }
            set { SetValue(ComboboxSelectedItemProperty, value); }
        }

        public static readonly DependencyProperty ComboboxSelectedItemProperty =
            DependencyProperty.Register("ComboboxSelectedItem", typeof(KeyValuePair<string, FindSearchKeywordParametersBots>), typeof(SearchBotsTextBoxUserControlView), new PropertyMetadata(new KeyValuePair<string, FindSearchKeywordParametersBots>("Id", FindSearchKeywordParametersBots.Id)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SearchBotsTextBoxUserControlView), new PropertyMetadata(""));

        public bool IncorrectInputData
        {
            get { return (bool)GetValue(IncorrectInputDataProperty); }
            set { SetValue(IncorrectInputDataProperty, value); }
        }

        public static readonly DependencyProperty IncorrectInputDataProperty =
            DependencyProperty.Register("IncorrectInputData", typeof(bool), typeof(SearchBotsTextBoxUserControlView), new PropertyMetadata(false));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(SearchBotsTextBoxUserControlView), new PropertyMetadata("Label"));

        public FontFamily FontfamilyCustom
        {
            get { return (FontFamily)GetValue(FontfamilyCustomProperty); }
            set { SetValue(FontfamilyCustomProperty, value); }
        }

        public static readonly DependencyProperty FontfamilyCustomProperty =
            DependencyProperty.Register("FontfamilyCustom", typeof(FontFamily), typeof(SearchBotsTextBoxUserControlView), new PropertyMetadata(ArialFontFamily));

        public SearchBotsTextBoxUserControlView()
        {
            InitializeComponent();
        }

        private void TextBoxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxTextBox.Text))
            {
                TextBoxPlaceholder.Visibility = Visibility.Visible;
                RemoveTextButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                TextBoxPlaceholder.Visibility = Visibility.Hidden;
                RemoveTextButton.Visibility = Visibility.Visible;
            }
        }

        private void TextBoxTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBoxPlaceholder.Visibility = Visibility.Hidden;
        }

        private void TextBoxTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxTextBox.Text))
            {
                TextBoxPlaceholder.Visibility = Visibility.Visible;
            }
            else
            {
                TextBoxPlaceholder.Visibility = Visibility.Hidden;
            }
        }

        private void RemoveTextButton_Click(object sender, RoutedEventArgs e)
        {
            TextBoxTextBox.Clear();
        }
    }
}
