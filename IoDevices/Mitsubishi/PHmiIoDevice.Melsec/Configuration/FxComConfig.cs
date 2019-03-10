﻿using System;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Xml;

namespace PHmiIoDevice.Melsec.Configuration {
    public class FxComConfig : Config {
        public const string Name = "FxCom";
        private int _baudRate = 9600;
        private int _dataBits = 7;
        private Parity _parity = Parity.Even;
        private string _portName;
        private StopBits _stopBits = StopBits.One;
        private int _tryCount = 3;

        public FxComConfig() : base(Name) {
            Timeout = 1500;
            try {
                _portName = SerialPort.GetPortNames().OrderBy(p => p).First();
            } catch (Exception) {
                _portName = "COM1";
            }
        }

        public string PortName {
            get { return _portName; }
            set {
                _portName = value;
                OnPropertyChanged("PortName");
            }
        }

        public int BaudRate {
            get { return _baudRate; }
            set {
                _baudRate = value;
                OnPropertyChanged("BaudRate");
            }
        }

        public int DataBits {
            get { return _dataBits; }
            set {
                _dataBits = value;
                OnPropertyChanged("DataBits");
            }
        }

        public Parity Parity {
            get { return _parity; }
            set {
                _parity = value;
                OnPropertyChanged("Parity");
            }
        }

        public StopBits StopBits {
            get { return _stopBits; }
            set {
                _stopBits = value;
                OnPropertyChanged("StopBits");
            }
        }

        public int TryCount {
            get { return _tryCount; }
            set {
                if (value <= 0)
                    throw new Exception("Try count. Must be > 0");
                _tryCount = value;
                OnPropertyChanged("TryCount");
            }
        }

        protected override void GetXml(XmlDocument document, XmlNode rootElement) {
            base.GetXml(document, rootElement);
            AddElement(document, rootElement, "PortName", PortName);
            AddElement(document, rootElement, "BaudRate", BaudRate.ToString(CultureInfo.InvariantCulture));
            AddElement(document, rootElement, "DataBits", DataBits.ToString(CultureInfo.InvariantCulture));
            AddElement(document, rootElement, "Parity", Parity.ToString());
            AddElement(document, rootElement, "StopBits", StopBits.ToString());
            AddElement(document, rootElement, "TryCount", TryCount.ToString(CultureInfo.InvariantCulture));
        }

        protected override void SetXml(XmlNode rootElement) {
            base.SetXml(rootElement);
            PortName = GetString(rootElement, "PortName");
            BaudRate = GetInt(rootElement, "BaudRate");
            DataBits = GetInt(rootElement, "DataBits");
            Parity = (Parity) Enum.Parse(typeof(Parity), GetString(rootElement, "Parity"));
            StopBits = (StopBits) Enum.Parse(typeof(StopBits), GetString(rootElement, "StopBits"));
            TryCount = GetInt(rootElement, "TryCount");
        }
    }
}