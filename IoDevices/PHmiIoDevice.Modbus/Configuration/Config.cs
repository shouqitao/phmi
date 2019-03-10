using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using PHmiIoDevice.Modbus.BytesConverters;

namespace PHmiIoDevice.Modbus.Configuration {
    public class Config : INotifyPropertyChanged {
        private BytesOrder _bytesOrder = BytesOrder.HL;
        private byte _defaultAddress = 1;
        private int _messageEndTimeout = 10;
        private int _timeout = 1500;
        private int _tryCount = 3;

        public Config(string name) {
            ConfigName = name;
        }

        public string ConfigName { get; }

        public int TryCount {
            get { return _tryCount; }
            set {
                if (value <= 0)
                    throw new Exception("Try count. Must be > 0");
                _tryCount = value;
                OnPropertyChanged("TryCount");
            }
        }

        public byte DefaultAddress {
            get { return _defaultAddress; }
            set {
                _defaultAddress = value;
                OnPropertyChanged("DefaultAddress");
            }
        }

        public BytesOrder BytesOrder {
            get { return _bytesOrder; }
            set {
                _bytesOrder = value;
                OnPropertyChanged("BytesOrder");
            }
        }

        public int Timeout {
            get { return _timeout; }
            set {
                if (value < 100)
                    throw new Exception("Timeout. Must be >= 100");
                _timeout = value;
                OnPropertyChanged("Timeout");
            }
        }

        public int MessageEndTimeout {
            get { return _messageEndTimeout; }
            set {
                if (value < 10)
                    throw new Exception("Message end timeout. Must be >= 10");
                _messageEndTimeout = value;
                OnPropertyChanged("MessageEndTimeout");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void AddElement(XmlDocument document, XmlNode rootElement, string name, string value) {
            XmlElement element = document.CreateElement(name);
            element.InnerText = value;
            rootElement.AppendChild(element);
        }

        protected virtual void GetXml(XmlDocument document, XmlNode rootElement) {
            AddElement(document, rootElement, "Timeout", Timeout.ToString(CultureInfo.InvariantCulture));
            AddElement(document, rootElement, "TryCount", TryCount.ToString(CultureInfo.InvariantCulture));
            AddElement(document, rootElement, "MessageEndTimeout",
                MessageEndTimeout.ToString(CultureInfo.InvariantCulture));
        }

        public string GetXml() {
            return ConfigHelper.GetXml(GetDocument());
        }

        public XmlDocument GetDocument() {
            var document = new XmlDocument();
            XmlElement rootElement = document.CreateElement(ConfigName);
            document.AppendChild(rootElement);
            GetXml(document, rootElement);
            return document;
        }

        public void SetXml(string xml) {
            XmlDocument document = ConfigHelper.GetDocument(xml);
            SetDocument(document);
        }

        public void SetDocument(XmlDocument document) {
            XmlNode rootElement = document.GetElementsByTagName(ConfigName).Item(0);
            SetXml(rootElement);
        }

        protected virtual void SetXml(XmlNode rootElement) {
            Timeout = GetInt(rootElement, "Timeout");
            TryCount = GetInt(rootElement, "TryCount");
            MessageEndTimeout = GetInt(rootElement, "MessageEndTimeout");
        }

        protected string GetString(XmlNode rootElement, string tagName) {
            foreach (XmlNode node in rootElement.ChildNodes.OfType<XmlNode>()
                .Where(node => node.Name == tagName)) return node.InnerText;
            throw new KeyNotFoundException(tagName);
        }

        protected int GetInt(XmlNode rootElement, string tagName) {
            return int.Parse(GetString(rootElement, tagName));
        }
    }
}