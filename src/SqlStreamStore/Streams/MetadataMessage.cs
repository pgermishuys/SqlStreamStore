namespace SqlStreamStore.Streams
{
    /// <summary>
    ///     Represents a streams metadata.
    /// </summary>
    public class MetadataMessage
    {
        /// <summary>
        /// The Stream Id.
        /// </summary>
        public string StreamId;

        /// <summary>
        /// The max age of messages retained in the stream.
        /// </summary>
        public int? MaxAge;

        /// <summary>
        /// The max count of message retained in the stream.
        /// </summary>
        public int? MaxCount;
        
        /// <summary>
        /// The point at which any messages prior to this position will get ignored.
        /// </summary>
        public int? TruncateBefore { get; set; }

        /// <summary>
        /// Custom Json 
        /// </summary>
        public string MetaJson;
    }
}