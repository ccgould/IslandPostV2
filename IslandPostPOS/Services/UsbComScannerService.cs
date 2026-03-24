using Microsoft.UI.Dispatching;
using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace IslandPostPOS.Services
{
    public class UsbComScannerService : IDisposable
    {
        private readonly DispatcherQueue _dispatcher;
        private readonly string _portName;
        private SerialPort? _serialPort;
        private bool _isRunning;
        private bool _isOpening;
        private DateTime _lastAttempt = DateTime.MinValue;

        public event EventHandler<string>? ScanReceived;
        public event EventHandler? ScannerDisconnected;
        public event EventHandler? ScannerReconnected;

        public UsbComScannerService(DispatcherQueue dispatcher, string portName = "COM5")
        {
            _dispatcher = dispatcher;
            _portName = portName;
        }

        public void Start()
        {
            _isRunning = true;
            Task.Run(MonitorLoop);
        }

        public void Stop()
        {
            _isRunning = false;
            ClosePort();
        }

        private async Task MonitorLoop()
        {
            while (_isRunning)
            {
                var ports = SerialPort.GetPortNames();
                bool portAvailable = ports.Contains(_portName);

                if (portAvailable && (_serialPort == null || !_serialPort.IsOpen))
                {
                    // Only retry if not already opening and after cooldown
                    if (!_isOpening && DateTime.Now - _lastAttempt > TimeSpan.FromSeconds(5))
                    {
                        _isOpening = true;
                        _lastAttempt = DateTime.Now;
                        TryOpenPort();
                        _isOpening = false;
                    }
                }
                else if (!portAvailable && _serialPort != null)
                {
                    ClosePort();
                    ScannerDisconnected?.Invoke(this, EventArgs.Empty);
                }

                await Task.Delay(2000); // check every 2 seconds
            }
        }

        private void TryOpenPort()
        {
            try
            {
                _serialPort = new SerialPort(_portName, 9600, Parity.None, 8, StopBits.One);
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                ScannerReconnected?.Invoke(this, EventArgs.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                // Port busy — let backoff handle retry
            }
            catch (IOException)
            {
                // Device vanished — wait for next loop
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Scanner open failed: {ex.Message}");
                ClosePort();
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var sp = (SerialPort)sender;
                string data = sp.ReadExisting();

                _dispatcher.TryEnqueue(() =>
                {
                    ScanReceived?.Invoke(this, data);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Data receive failed: {ex.Message}");
                ClosePort();
                ScannerDisconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ClosePort()
        {
            if (_serialPort != null)
            {
                try
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    if (_serialPort.IsOpen) _serialPort.Close();
                    _serialPort.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Scanner close failed: {ex.Message}");
                }
                _serialPort = null;
            }
        }

        public void Dispose()
        {
            _isRunning = false;
            ClosePort();
        }
    }
}