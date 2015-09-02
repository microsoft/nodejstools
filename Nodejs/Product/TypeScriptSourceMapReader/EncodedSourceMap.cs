namespace TypeScriptSourceMapReader
{
    /// <summary>
    /// SourceMap as we read from the mapping file
    /// </summary>
    public class EncodedSourceMap
    {
        /// <summary>
        /// Version string in the map
        /// </summary>
        public int version;

        /// <summary>
        /// File this map is for
        /// </summary>
        public string file;

        /// <summary>
        /// Souces from which the file was generated
        /// </summary>
        public string[] sources;

        /// <summary>
        /// Names
        /// </summary>
        public string[] names;

        /// <summary>
        /// Mapped Base64VLQ encoded string
        /// </summary>
        public string mappings;

        /// <summary>
        /// SourceRoot to be prepended to each sources entry
        /// </summary>
        public string sourceRoot;
    }
}
