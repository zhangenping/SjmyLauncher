using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using System.Windows.Controls;

namespace SjmyLauncher.View
{
    /// <summary>
    /// StartMyView.xaml 的交互逻辑
    /// </summary>
    public partial class StartMyView : UserControl
    {
        public StartMyView()
        {
            Messenger.Default.Register<UserEvent.UserEventType>(this, (eventType) =>
            {
                if (eventType == UserEvent.UserEventType.BeginEncryption)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        BtnStarSJMY.IsEnabled = false;
                    });
                }
                else if (eventType == UserEvent.UserEventType.EndEncryption)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        BtnStarSJMY.IsEnabled = true;
                    });
                }
            });

            InitializeComponent();

            this.DataContext = new StarSjMyModel();
        }
    }
}
