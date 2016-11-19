using Net;
using Net;

namespace System.Net
{
    /// <devdoc>
    /// <para>
    /// <see cref='System.Net.HttpWebRequest'/> is an HTTP-specific implementation of the <see cref='System.Net.WebRequest'/> class.
    ///
    ///  Performs the major body of HTTP request processing. Handles
    ///    everything between issuing the HTTP header request to parsing the
    ///    the HTTP response.  At that point, we hand off the request to the response
    ///    object, where the programmer can query for headers or continue reading, usw.
    ///  </para>
    /// </devdoc>


    [Flags]
    public enum DecompressionMethods{
        None = 0,
        GZip = 1,
        Deflate = 2
    }
}