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
using static CommonModels.ProjectTask.EarningSite.SearchTasksModels.SearchTaskEnums;

namespace DesktopWPFManagementApp.MVVM.View.CustomControls.Elements
{
    /// <summary>
    /// Логика взаимодействия для SearchEarnSiteTasksTextBoxUserControlView.xaml
    /// </summary>
    public partial class SearchEarnSiteTasksTextBoxUserControlView : UserControl
    {
        private static readonly FontFamily ArialFontFamily = new FontFamily("Arial");

        public ObservableCollection<KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>> ComboboxItemSourceList
        {
            get { return (ObservableCollection<KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>>)GetValue(ComboboxItemSourceListProperty); }
            set { SetValue(ComboboxItemSourceListProperty, value); }
        }

        public static readonly DependencyProperty ComboboxItemSourceListProperty =
            DependencyProperty.Register("ComboboxItemSourceList", typeof(ObservableCollection<KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>>), typeof(SearchEarnSiteTasksTextBoxUserControlView), new PropertyMetadata(new ObservableCollection<KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>>()));

        public KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks> ComboboxSelectedItem
        {
            get { return (KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>)GetValue(ComboboxSelectedItemProperty); }
            set { SetValue(ComboboxSelectedItemProperty, value); }
        }

        public static readonly DependencyProperty ComboboxSelectedItemProperty =
            DependencyProperty.Register("ComboboxSelectedItem", typeof(KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>), typeof(SearchEarnSiteTasksTextBoxUserControlView), new PropertyMetadata(new KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>("Id", FindSearchKeywordParametersEarnSiteTasks.Id)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SearchEarnSiteTasksTextBoxUserControlView), new PropertyMetadata(""));

        public bool IncorrectInputData
        {
            get { return (bool)GetValue(IncorrectInputDataProperty); }
            set { SetValue(IncorrectInputDataProperty, value); }
        }

        public static readonly DependencyProperty IncorrectInputDataProperty =
            DependencyProperty.Register("IncorrectInputData", typeof(bool), typeof(SearchEarnSiteTasksTextBoxUserControlView), new PropertyMetadata(false));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(SearchEarnSiteTasksTextBoxUserControlView), new PropertyMetadata("Label"));

        public FontFamily FontfamilyCustom
        {
            get { return (FontFamily)GetValue(FontfamilyCustomProperty); }
            set { SetValue(FontfamilyCustomProperty, value); }
        }

        public static readonly DependencyProperty FontfamilyCustomProperty =
            DependencyProperty.Register("FontfamilyCustom", typeof(FontFamily), typeof(SearchEarnSiteTasksTextBoxUserControlView), new PropertyMetadata(ArialFontFamily));

        public SearchEarnSiteTasksTextBoxUserControlView()
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
