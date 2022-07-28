namespace past.Core.Wrappers
{
    /// <summary>
    /// Represents a wrapper holding an instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the wrapped instance.</typeparam>
    public interface IInstanceWrapper<T>
    {
        /// <summary>
        /// The wrapped instance.
        /// </summary>
        T WrappedInstance { get; }
    }
}
