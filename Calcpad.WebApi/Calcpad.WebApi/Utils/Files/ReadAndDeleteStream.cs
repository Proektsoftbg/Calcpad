namespace Calcpad.WebApi.Utils.Files
{
    /// <summary>
    /// Read a file and delete it after closing the stream
    /// </summary>
    /// <param name="path"></param>
    public class ReadAndDeleteStream(string path) : FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
    {
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                File.Delete(this.Name);
            }
        }
    }
}
