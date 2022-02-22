namespace Blish_HUD.Settings {

    public readonly struct SettingValidationResult {

        /// <summary>
        /// Indicates if the value provided for validation was valid.
        /// </summary>
        public bool Valid { get; }

        /// <summary>
        /// [NOT IMPLEMENTED] If <see cref="Valid" /> is <c>false</c>, then this is the error message should be displayed to the user indicating the issue with the value.
        /// </summary>
        public string InvalidMessage { get; }

        public SettingValidationResult(bool valid, string invalidMessage = null) {
            this.Valid          = valid;
            this.InvalidMessage = invalidMessage;
        }

    }

}