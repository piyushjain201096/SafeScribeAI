using System.Windows;
using System.Windows.Input;

namespace SafeScribe.UI
{
    public partial class FloatingWidget : Window
    {
        public FloatingWidget()
        {
            InitializeComponent();
            this.MouseDown += (s, e) => { if (e.ChangedButton == MouseButton.Left) this.DragMove(); };
        }

        public void SetRecordingStatus(bool isRecording)
        {
            Dispatcher.Invoke(() =>
            {
                if (isRecording)
                {
                    DotRecording.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(239, 68, 68));
                    TxtStatus.Text = "Recording Live...";
                }
                else
                {
                    DotRecording.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 185, 129));
                    TxtStatus.Text = "SafeScribe Ready";
                }
            });
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
