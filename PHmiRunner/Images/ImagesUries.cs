using System;

namespace PHmiRunner.Images {
    public static class ImagesUries {
        public static Uri Runit {
            get { return GetImageUri("agt_runit.ico"); }
        }

        public static Uri RunitGray {
            get { return GetImageUri("agt_runit_gray.ico"); }
        }

        public static Uri RunitRed {
            get { return GetImageUri("agt_runit_red.ico"); }
        }

        private static Uri GetImageUri(string image) {
            return new Uri(string.Format(@"pack://application:,,,/PHmiRunner;component/Images/{0}", image),
                UriKind.Absolute);
        }
    }
}