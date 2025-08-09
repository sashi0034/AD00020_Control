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

        private readonly List<string> _logMessages = new List<string>();

        private SettingsObject _settingsObject = new SettingsObject();

        private CancellationTokenSource _sendCancellation = new CancellationTokenSource();

        private bool _jobRunning = false;
        private CancellationTokenSource _jobCancellation = new CancellationTokenSource();
        private int _previousJobTimestampHour = 0;

        public MainWindow()
        {
            InitializeComponent();

            if (!loadSettings())
            {
                Application.Current.Shutdown();
                return;
            }

            ensureDeviceHandle();

            requestJobToggle(false);
        }

        ~MainWindow()
        {
            _sendCancellation.Cancel();

            if (_deviceHandle != null && !_deviceHandle.IsInvalid)
            {
                USBIR.closeUSBIR(_deviceHandle);
                _deviceHandle.Dispose();
            }
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

                var deserialized = JsonSerializer.Deserialize<SettingsObject>(json, options);
                if (deserialized == null)
                {
                    MessageBox.Show("Failed to deserialize settings.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                _settingsObject = deserialized;
                _settingsObject.InitializeCommandMap();
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

        private void requestCommand(string commandName)
        {
            ensureDeviceHandle();

            if (_deviceHandle == null || _deviceHandle.IsInvalid)
            {
                MessageBox.Show("USB DEVICE is not connected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_settingsObject.CommandMap.TryGetValue(commandName, out var command) == false)
            {
                MessageBox.Show("Power On command not found in settings.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            _sendCancellation.Cancel();
            _sendCancellation = new CancellationTokenSource();
            sendCommandAsync(command, _sendCancellation.Token).RunErrorHandler();
        }

        private async Task sendCommandAsync(CommandData command, CancellationToken cancellationToken)
        {
            await using var cancelRegister = cancellationToken.Register(() =>
            {
                Dispatcher.Invoke(() => { appendLogMessage($"⚠️ Command cancelled: {command.Comment}"); });
            });

            foreach (var bytes in command.Bytes)
            {
                HelperFunctions.ParseHexString(bytes, out byte[] byteCode, out int byteLength);

                // USB DEVICE へ送信
                if (USBIR.writeUSBIR_Direct(_deviceHandle, byteCode, byteLength) != 0)
                {
                    Dispatcher.Invoke(() => { appendLogMessage($"❌ Failed to send: {bytes}"); });
                }
                else
                {
                    Dispatcher.Invoke(() => { appendLogMessage($"✅ Sent: {bytes}"); });
                }

                await Task.Delay(1000, cancellationToken);
            }

            Dispatcher.Invoke(() => { appendLogMessage($"ℹ️ Command succeeded: {command.Comment}"); });
        }

        private void PowerOnButton_OnClick(object sender, RoutedEventArgs e)
        {
            requestCommand("power_on");
        }

        private void PowerOffButton_OnClick(object sender, RoutedEventArgs e)
        {
            requestCommand("power_off");
        }

        private void OpenSettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            // エクスプローラーで設定ファイルを開く
            try
            {
                string settingsDir = Path.GetDirectoryName(settingPath) ?? string.Empty;
                if (Directory.Exists(settingsDir))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = settingsDir,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
                else
                {
                    MessageBox.Show("Settings directory does not exist.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open settings directory: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ReloadSettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            // 設定を再読み込み
            if (loadSettings())
            {
                appendLogMessage("Settings reloaded successfully.");
            }
            else
            {
                appendLogMessage("Failed to reload settings.");
            }
        }

        private void JobToggleButton_OnClick(object sender, RoutedEventArgs e)
        {
            requestJobToggle(!_jobRunning);
        }

        private void requestJobToggle(bool shouldRun)
        {
            _jobRunning = shouldRun;

            JobToggleButton.Content = shouldRun ? "⏹️ Stop Job" : "▶️ Start Job";

            _jobCancellation.Cancel();
            _jobCancellation = new CancellationTokenSource();
            if (shouldRun)
            {
                runJobAsync(_jobCancellation.Token).RunErrorHandler();
            }
        }

        private async Task runJobAsync(CancellationToken cancellationToken)
        {
            _previousJobTimestampHour = -1;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    DateTime now = DateTime.Now;
                    int currentHour = now.Hour;

                    // 前回の実行時刻と異なる場合のみ実行
                    if (currentHour != _previousJobTimestampHour)
                    {
                        _previousJobTimestampHour = currentHour;
                        foreach (var job in _settingsObject.Job)
                        {
                            if (job.Hour == currentHour)
                            {
                                appendLogMessage($"Executing job: {job.Command}");

                                // コマンドを実行
                                if (_settingsObject.CommandMap.TryGetValue(job.Command, out var command))
                                {
                                    await sendCommandAsync(command, cancellationToken);
                                }
                                else
                                {
                                    appendLogMessage($"❌ Command '{job.Command}' not found in settings.");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    appendLogMessage($"Error in job execution: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }
        }
    }
}