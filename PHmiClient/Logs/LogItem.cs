using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using PHmiClient.Utils;

namespace PHmiClient.Logs {
    [DataContract]
    public sealed class LogItem : INotifyPropertyChanged {
        private byte[] _bytes;
        private string _text;
        private DateTime _time;

        [DataMember]
        public DateTime Time {
            get { return _time; }
            set {
                _time = value;
                OnPropertyChanged("Time");
            }
        }

        [DataMember]
        public string Text {
            get { return _text; }
            set {
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        [DataMember]
        public byte[] Bytes {
            get { return _bytes; }
            set {
                _bytes = value;
                OnPropertyChanged("Bytes");
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void SetToBytes(object serializable) {
            BinaryFormatter formatter = CreateFormatter();
            using (var stream = new MemoryStream()) {
                formatter.Serialize(stream, serializable);
                stream.Seek(0, SeekOrigin.Begin);
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                Bytes = bytes;
            }
        }

        public object GetFromBytes() {
            var bytes = Bytes;
            if (bytes == null)
                return null;
            BinaryFormatter formatter = CreateFormatter();
            using (var stream = new MemoryStream()) {
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(stream);
            }
        }

        private BinaryFormatter CreateFormatter() {
            var formatter = new BinaryFormatter {AssemblyFormat = FormatterAssemblyStyle.Simple};
            return formatter;
        }

        private void OnPropertyChanged(string propertyName) {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
        }
    }
}