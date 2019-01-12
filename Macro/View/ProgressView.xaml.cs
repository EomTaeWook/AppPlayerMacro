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

namespace Macro.View
{
    /// <summary>
    /// ProgressView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProgressView : Window
    {
        public ProgressView()
        {
            InitializeComponent();
            Loaded += ProgressView_Loaded;
        }

        private void ProgressView_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Init()
        {
            if(Owner != null)
            {
                Left = Owner.Left;
                Top = Owner.Top;
                Width = Owner.Width;
                Height = Owner.Height;
            }
        }
    }
}
