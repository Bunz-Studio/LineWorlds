using System;
using System.IO;
using System.Collections.Generic;
using SimpleFileBrowser;

public static class Storage
{
	static bool p_dataChanged;
	static string p_dataPath = GoUpFolder(UnityEngine.Application.dataPath);
	public static string dataPath
	{
		#if UNITY_ANDROID
		get
		{
            return UnityEngine.Application.isEditor ? p_dataPath : p_dataChanged ? p_dataPath : UnityEngine.Application.persistentDataPath;
		}
		#else
		get
		{
			return UnityEngine.Application.isEditor ? p_dataPath : p_dataChanged ? p_dataPath : GoUpFolder(UnityEngine.Application.dataPath);
		}
		#endif
		
		set
		{
			p_dataChanged = true;
			p_dataPath = value;
		}
	}
	
	public static string GetPathLocal(string path)
	{
		return string.IsNullOrWhiteSpace(path) ? path : Path.Combine(dataPath, path);
	}
	
	public static void CheckDirectoryLocal(string path)
	{
		CheckDirectory(GetPathLocal(path));
	}
	
	public static void CheckDirectory(string path)
	{
        // if(!Directory.Exists(path) && !string.IsNullOrWhiteSpace(path)) Directory.CreateDirectory(path);
        if (!FileBrowserHelpers.DirectoryExists(path) && !string.IsNullOrWhiteSpace(path)) FileBrowserHelpers.CreateFolderInDirectory(GoUpFolder(path), Path.GetFileName(path));
	}

    public static string[] GetFilesLocal(string directory)
    {
        CheckDirectory(GetPathLocal(directory));
        //return Directory.GetFiles(GetPathLocal(directory));
        var files = FileBrowserHelpers.GetEntriesInDirectory(GetPathLocal(directory), true);
        var trueFiles = new List<string>();
        foreach (var file in files)
        {
            if (!file.IsDirectory) trueFiles.Add(file.Path);
        }
        return trueFiles.ToArray();
    }
    public static string[] GetFiles(string directory)
    {
        CheckDirectory(directory);
        //return Directory.GetFiles(GetPathLocal(directory));
        var files = FileBrowserHelpers.GetEntriesInDirectory(directory, true);
        var trueFiles = new List<string>();
        foreach (var file in files)
        {
            if (!file.IsDirectory) trueFiles.Add(file.Path);
        }
        return trueFiles.ToArray();
    }

    public static string[] GetDirectoriesLocal(string directory)
	{
		CheckDirectory(GetPathLocal(directory));
        //return Directory.GetFiles(GetPathLocal(directory));
        var files = FileBrowserHelpers.GetEntriesInDirectory(GetPathLocal(directory), true);
        var trueFiles = new List<string>();
        foreach (var file in files)
        {
            if (file.IsDirectory) trueFiles.Add(file.Path);
        }
        return trueFiles.ToArray();
    }
    public static string[] GetDirectories(string directory)
    {
        CheckDirectory(directory);
        //return Directory.GetFiles(GetPathLocal(directory));
        var files = FileBrowserHelpers.GetEntriesInDirectory(directory, true);
        var trueFiles = new List<string>();
        foreach (var file in files)
        {
            if (file.IsDirectory) trueFiles.Add(file.Path);
        }
        return trueFiles.ToArray();
    }

    public static bool FileExistsLocal(string path)
	{
        //return File.Exists(GetPathLocal(path));
        return FileBrowserHelpers.FileExists(GetPathLocal(path));
    }
    public static bool FileExists(string path)
    {
        //return File.Exists(GetPathLocal(path));
        return FileBrowserHelpers.FileExists(path);
    }

    public static void WriteAllTextLocal(string path, string content)
	{
        //File.WriteAllText(GetPathLocal(path), content);
        FileBrowserHelpers.WriteTextToFile(GetPathLocal(path), content);
    }
    public static void WriteAllText(string path, string content)
    {
        //File.WriteAllText(GetPathLocal(path), content);
        FileBrowserHelpers.WriteTextToFile(path, content);
    }

    public static void WriteAllBytesLocal(string path, byte[] bytes)
    {
        // File.WriteAllBytes(GetPathLocal(path), bytes);
        FileBrowserHelpers.WriteBytesToFile(GetPathLocal(path), bytes);
    }

    public static void WriteAllBytes(string path, byte[] bytes)
    {
        // File.WriteAllBytes(GetPathLocal(path), bytes);
        FileBrowserHelpers.WriteBytesToFile(path, bytes);
    }

    public static string ReadAllTextLocal(string path)
	{
		// return File.ReadAllText(GetPathLocal(path));
        return FileBrowserHelpers.ReadTextFromFile(GetPathLocal(path));
    }
    public static string ReadAllText(string path)
    {
        // return File.ReadAllText(GetPathLocal(path));
        return FileBrowserHelpers.ReadTextFromFile(path);
    }

    public static byte[] ReadAllBytesLocal(string path)
    {
        // return File.ReadAllBytes(GetPathLocal(path));
        return FileBrowserHelpers.ReadBytesFromFile(GetPathLocal(path));
    }
    public static byte[] ReadAllBytes(string path)
    {
        // return File.ReadAllBytes(GetPathLocal(path));
        return FileBrowserHelpers.ReadBytesFromFile(path);
    }

    public static string GoUpFolder(string dir)
    {
		var e = dir.Split(new [] {'/', '\\'});
		var y = dir.Remove(dir.Length - e[e.Length - 1].Length, e[e.Length - 1].Length);
        if(y.EndsWith("\\", StringComparison.CurrentCulture) || y.EndsWith("/", StringComparison.CurrentCulture))
        {
            y = y.Remove(y.Length - 1, 1);
        }
		return y;
    }
}
