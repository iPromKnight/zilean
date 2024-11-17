namespace Zilean.Scraper.Features.PythonSupport;

public static class PyObjectExtensions
{
    public static bool HasKey(this PyObject dict, string key) =>
        dict.InvokeMethod("__contains__", new PyString(key)).As<bool>();
}
