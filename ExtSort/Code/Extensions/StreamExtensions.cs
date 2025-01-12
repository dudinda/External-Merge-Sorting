namespace ExtSort.Code.Extensions 
{
    internal static class StreamExtensions 
    {
        public static void SkipPreamble(this StreamWriter writer) 
        {
            var tmp = writer.AutoFlush;
            writer.AutoFlush = false;
            writer.BaseStream.Position = 0;
            writer.AutoFlush = tmp;
        }
    }
}
