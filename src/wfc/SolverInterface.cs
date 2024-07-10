namespace wfc
{
    /// <summary>
    /// GBWFC wrapper for specific task.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISolver<T>
    {
        /// <summary>
        /// Solves given example using GBWFC and returns result in the same format.
        /// </summary>
        /// <param name="example"></param>
        /// <returns></returns>
        public T? Solve(T example);
    }
}
