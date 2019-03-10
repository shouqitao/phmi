namespace PHmiRunner.Utils.IoDeviceRunner {
    public interface ITagValueHolder {
        string Address { get; }

        void Update(object value);

        object GetWriteValue();
    }

    public interface ITagValueHolder<T> : ITagValueHolder {
        T GetValue();

        void SetValue(T value);
    }

    public abstract class TagValueHolder<T> : ITagValueHolder<T> {
        private bool _valueIsSet;
        protected T Value;

        public void Update(object value) {
            if (!_valueIsSet) Value = RawToEng(value);
        }

        public object GetWriteValue() {
            object value = EngToRaw(Value);
            _valueIsSet = false;
            return value;
        }

        public abstract string Address { get; }

        public T GetValue() {
            return Value;
        }

        public void SetValue(T value) {
            _valueIsSet = true;
            Value = value;
        }

        protected abstract T RawToEng(object value);

        protected abstract object EngToRaw(T value);

        public void ClearValue() {
            _valueIsSet = false;
            Value = default(T);
        }
    }
}