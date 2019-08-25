namespace Blish_HUD.Contexts {

    /// <summary>
    /// The result when querying a <see cref="Context"/>.
    /// </summary>
    /// <typeparam name="T">The type the call returns.</typeparam>
    public class ContextResult<T> {

        /// <summary>
        /// The value requested from the <see cref="Context"/>.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// A message indicating the result of the context request.
        /// This string is suitable to display in the UI.
        /// </summary>
        public string Status { get; }

        public ContextResult(T value, string status) {
            this.Value  = value;
            this.Status = status;
        }

        public ContextResult(T value, bool succeeded) {
            this.Value  = value;
            this.Status = succeeded ? "Succeeded!" : "Failed!";
        }

        public static implicit operator T(ContextResult<T> contextResult) {
            return contextResult.Value;
        }

    }

}
