namespace WebDAVSharp.Data.HelperClasses
{
    // The casts to object in the below code are an unfortunate necessity due to
    // C#'s restriction against a where T : Enum constraint. (There are ways around
    // this, but they're outside the scope of this simple illustration.)
    public static class FlagsHelper
    {
        public static bool IsSet<T>(T flags, T flag) where T : struct
        {
            int flagsValue = (int) (object) flags;
            int flagValue = (int) (object) flag;

            return (flagsValue & flagValue) != 0;
        }

        public static T Set<T>(T flags, T flag) where T : struct
        {
            int flagsValue = (int) (object) flags;
            int flagValue = (int) (object) flag;

            flags = (T) (object) (flagsValue | flagValue);
            return flags;
        }

        public static T Unset<T>(T flags, T flag) where T : struct
        {
            int flagsValue = (int) (object) flags;
            int flagValue = (int) (object) flag;

            flags = (T) (object) (flagsValue & (~flagValue));
            return flags;
        }
    }
}