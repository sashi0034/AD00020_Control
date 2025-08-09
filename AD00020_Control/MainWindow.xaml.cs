using System.IO;
using System.Text.Json;
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
        private const string settingPath = "settings/settings.json";

        private SafeFileHandle? _deviceHandle = null;

        private List<string> _logMessages = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            if (!loadSettings())
            {
                Application.Current.Shutdown();
                return;
            }

            ensureDeviceHandle();
        }

        private bool loadSettings()
        {
            if (!File.Exists(settingPath))
            {
                MessageBox.Show($"'{settingPath}' not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            try
            {
                string json = File.ReadAllText(settingPath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                var root = JsonSerializer.Deserialize<SettingsObject>(json, options);
                root.InitializeCommandMap();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load settings: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void appendLogMessage(string message)
        {
            if (_logMessages.Count >= 128)
            {
                _logMessages.RemoveAt(0);
            }

            _logMessages.Add($"[{DateTime.Now:MM/dd HH:mm:ss}] {message}");
            LoggerText.Text = string.Join(Environment.NewLine, _logMessages);
        }

        private void ensureDeviceHandle()
        {
            if (_deviceHandle == null || _deviceHandle.IsInvalid)
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                _deviceHandle = USBIR.openUSBIR(hwnd);
                if (_deviceHandle == null || _deviceHandle.IsInvalid)
                {
                    StatusText.Text = "❌ Disconnected";
                    StatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                    appendLogMessage("❌ Failed to open USB DEVICE.");
                }
                else
                {
                    StatusText.Text = "✅ Connected";
                    StatusText.Foreground = System.Windows.Media.Brushes.GreenYellow;
                    appendLogMessage("✅ USB DEVICE opened successfully.");
                }
            }
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