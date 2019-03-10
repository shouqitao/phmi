using System.ComponentModel;

namespace PHmiClient.Tags {
    public abstract class TagAbstract<T> : ITag<T> {
        internal abstract int Id { get; }

        internal abstract bool IsWritten { get; }

        internal abstract bool IsRead { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract T Value { get; set; }

        public abstract event PropertyChangedEventHandler PropertyChanged;

        internal abstract void UpdateValue(T value);

        internal abstract T GetWrittenValue();
    }
}