namespace Dull.WinformApp
{
    static class Helper
    {
        public static int BuildWParam(ushort low, ushort high)
        {
            return ((int)high << 16) | (int)low;
        }
    }
}
