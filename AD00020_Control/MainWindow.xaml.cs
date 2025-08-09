using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32.SafeHandles;

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
            SafeFileHandle handle_usb_device = null; // USB DEVICEハンドル
            byte[] code = new byte[] { 0x12, 0x08, 0x00, 0x00, 0x00, 0xFF }; // 赤外線コード
            int i_ret = 0;

            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            try
            {
                // USB DEVICEオープン
                handle_usb_device = USBIR.openUSBIR(hwnd);
                if (handle_usb_device != null)
                {
                    // USB DEVICEへ送信 パラメータ[USB DEVICEハンドル、フォーマットタイプ、送信赤外線コード、赤外線コードのビット長]
                    i_ret = USBIR.writeUSBIR(handle_usb_device, USBIR.IR_FORMAT.SONY, code, 12);
                    i_ret = USBIR.writeUSBIR(handle_usb_device, USBIR.IR_FORMAT.SONY, code, 48);
                }
            }
            catch
            {
            }
            finally
            {
                if (handle_usb_device != null)
                {
                    // USB DEVICEクローズ
                    i_ret = USBIR.closeUSBIR(handle_usb_device);
                }
            }
        }
    }
}