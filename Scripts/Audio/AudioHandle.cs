using System;

namespace Rhinox.Magnus
{
    public sealed class AudioHandle
    {
        private readonly Guid _id;

        public AudioHandle()
        {
            _id = Guid.NewGuid();
        }

        private bool Equals(AudioHandle other)
        {
            return _id.Equals(other._id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AudioHandle) obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }

}