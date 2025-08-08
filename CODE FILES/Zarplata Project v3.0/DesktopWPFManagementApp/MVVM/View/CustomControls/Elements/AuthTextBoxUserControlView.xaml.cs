using FontAwesome.Sharp;
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

namespace DesktopWPFManagementApp.MVVM.View.CustomControls.Elements
{
    /// <summary>
    /// Логика взаимодействия для AuthTextBoxUserControlView.xaml
    /// </summary>
    public partial class AuthTextBoxUserControlView : UserControl
    {
        private static readonly FontFamily ArialFontFamily = new FontFamily("Arial");
        private static readonly FontFamily PasswordFontFamily = new FontFamily(new Uri("pack://application:,,,/Resources/Fonts/"), "./#password");

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AuthTextBoxUserControlView), new PropertyMetadata(""));

        public IconChar Icon
        {
            get { return (IconChar)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(IconChar), typeof(AuthTextBoxUserControlView), new PropertyMetadata(IconChar.Robot));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(AuthTextBoxUserControlView), new PropertyMetadata("Label"));

        public FontFamily FontfamilyCustom
        {
            get { return (FontFamily)GetValue(FontfamilyCustomProperty); }
            set { SetValue(FontfamilyCustomProperty, value); }
        }

        public static readonly DependencyProperty FontfamilyCustomProperty =
            DependencyProperty.Register("FontfamilyCustom", typeof(FontFamily), typeof(AuthTextBoxUserControlView), new PropertyMetadata(ArialFontFamily));

        public bool IsPassword
        {
            get { return (bool)GetValue(IsPasswordProperty); }
            set { SetValue(IsPasswordProperty, value); }
        }

        public static readonly DependencyProperty IsPasswordProperty =
            DependencyProperty.Register("IsPassword", typeof(bool), typeof(AuthTextBoxUserControlView), new PropertyMetadata(false));

        public AuthTextBoxUserControlView()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            if (IsPassword)
            {
                SetValue(FontfamilyCustomProperty, PasswordFontFamily);
            }

            base.OnApplyTemplate();
        }

        private void TextBoxTextBox_TextChanged(object sender, TextChangedEventArgs e)
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
    }
}
