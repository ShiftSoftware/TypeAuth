namespace ShiftSoftware.TypeAuth.Core
{
    /// <summary>
    /// An Action could be of one of the 5 available types.
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// The action could have Read Acess Type or Not
        /// Boolean and Read are the same under the hood. The two are here for convenience
        /// </summary>
        Read = 0,
        /// <summary>
        /// The action could have Read, Read/Write Access Type or None.
        /// </summary>
        ReadWrite = 1,
        /// <summary>
        /// The action could have Read, Read/Write, Read/Write/Delete Access Type or None.
        /// </summary>
        ReadWriteDelete = 2,
        /// <summary>
        /// The action could have Read Access Type or Not.
        /// Boolean and Read are the same under the hood. The two are here for convenience.
        /// </summary>
        Boolean = 3,
        /// <summary>
        /// The action could have a custom Access Type represented as a String.
        /// </summary>
        Text = 4,
    }
}
