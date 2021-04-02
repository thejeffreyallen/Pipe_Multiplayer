//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.Tools
{
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Read bytes from ressources
    /// </summary>
    public class EmbeddedRessourceHelper
	{
        /// <summary>
        /// return bytes from embeded resources
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static byte[] ReadResourceFile(string filename)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            using (var stream = thisAssembly.GetManifestResourceStream(filename)) {
                using (var mem = new MemoryStream())
                {
                    CopyStream(stream, mem);
                    return mem.ToArray();
                }
            }
        }

        /// <summary>
        /// Copy one stream to another
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        private static void CopyStream(Stream input, Stream output)
        {
            byte[] b = new byte[32768];
            int r;
            while ((r = input.Read(b, 0, b.Length)) > 0) output.Write(b, 0, r);
        }
    }
}
