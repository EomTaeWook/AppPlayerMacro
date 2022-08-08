using System.Windows;

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
            HideProgressView();
        }
        public void ShowProgressView()
        {
            this.Visibility = Visibility.Visible;
        }
        public void HideProgressView()
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}
