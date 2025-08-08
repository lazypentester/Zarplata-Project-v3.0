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
    /// Логика взаимодействия для RoundedLabelWithIconUserControlView.xaml
    /// </summary>
    public partial class RoundedLabelWithIconUserControlView : UserControl
    {
        public BitmapImage Icon
        {
            get { return (BitmapImage)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(BitmapImage), typeof(RoundedLabelWithIconUserControlView), new PropertyMetadata(new BitmapImage(new Uri("Images/image_default.png", UriKind.Relative))));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(RoundedLabelWithIconUserControlView), new PropertyMetadata("Текст"));



        public RoundedLabelWithIconUserControlView()
        {
            InitializeComponent();
        }
    }
}
