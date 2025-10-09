namespace Calcpad.Document.Include
{
    public interface IIncludeResolver
    {
        string Include(string fileName, Queue<string>? fields);
        string GetIncludeFileName(ReadOnlySpan<char> line);
    }
}
