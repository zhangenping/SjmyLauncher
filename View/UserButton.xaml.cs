using System.Windows.Controls;

namespace SjmyLauncher.View
{
    /// <summary>
    /// UserButton.xaml 的交互逻辑
    /// </summary>
    public partial class UserButton : UserControl
    {
        private UserButtonModel _model;
        public UserButton()
        {
            InitializeComponent();

            _model = new UserButtonModel();
            this.DataContext = _model;
        }
    }
}
