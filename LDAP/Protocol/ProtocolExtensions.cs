namespace Telefrek.Security.LDAP.Protocol
{
    /// <summary>
    /// Extension method definitions
    /// </summary>
    public static class ProtocolExtensions
    {
        #region MSB/LSB (Most/Least Significant Bit/Log Base 2)

        static int[] MSBDeBruijnLookup = new int[]
        {
            0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
            8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
        };

        /// <summary>
        /// Uses a DeBruijn Lookup to calculate the MSB.
        /// </summary>
        /// <param name="x">The value to calculate the MSB for.</param>
        /// <returns>The position of the highest set bit.</returns>
        public static int MSB(this int x)
        {
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;

            return MSBDeBruijnLookup[(uint)(x * 0x07C4ACDDU) >> 27];
        }
        #endregion
    }
}