using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32.SafeHandles;
using ModernWpf;

namespace AD00020_Control
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            SafeFileHandle deviceHandle = null; // USB DEVICEハンドル

            string code = "0140980220E004000000060220E00400213480AF000006604000800006B60000000000";
            HelperFunctions.ParseHexString(code, out byte[] byteCode, out int byteLength);

            int i_ret = 0;

            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            try
            {
                // USB DEVICEオープン
                deviceHandle = USBIR.openUSBIR(hwnd);
                if (deviceHandle != null)
                {
                    // USB DEVICEへ送信 パラメータ[USB DEVICEハンドル、フォーマットタイプ、送信赤外線コード、赤外線コードのビット長]
                    i_ret = USBIR.writeUSBIR_Direct(deviceHandle, byteCode, byteLength);
                }
            }
            catch
            {
            }
            finally
            {
                if (deviceHandle != null)
                {
                    // USB DEVICEクローズ
                    i_ret = USBIR.closeUSBIR(deviceHandle);
                }
            }
        }
    }
}