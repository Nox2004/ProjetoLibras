// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("YuIv7GGrhPJPL9EwNCPgcW+lvDzzQcLh887FyulFi0U0zsLCwsbDwKqxVnKaeksjfhjcs7vEV7AgmX7C1DyAJAcWSF1bfJmCyUu+Y8zhj9G0jXXRa53Ks757MjwyiPDNcLa6+0jzetIcUI5q319Nm+QolrWQ7NATQcLMw/NBwsnBQcLCw3nl1b0RnAGdAvj+3bRKajCa43fzJ/tbxUpwH+yEzGKVXvpWaxc5/JWtXwa6kapzkMtmO4sLiBol75n+O/EFQ2pM7xN0K0R8rWF/w3aHLYlfI1HDpGZHn8Fr9DcSqnbKosBZTDUEdfnX5xYXHJq7q1dDobXBHQtuv1l5dW17775Mw+A9gHFfUrC3+EuRGtvI3VyDMvhIwkS0j3qKxsHAwsPC");
        private static int[] order = new int[] { 4,4,12,12,13,9,13,10,8,9,12,12,13,13,14 };
        private static int key = 195;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
