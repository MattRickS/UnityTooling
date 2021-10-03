/*
Separate class so that functionality can be easily changed, eg, encrypting
saves / saving to cloud / etc...
*/
public static class SaveManager
{
    public static bool SaveExists(string saveName)
    {
        return FileManager.FileExists(saveName);
    }
    public static bool SaveJSON(string saveName, string json)
    {
        return FileManager.WriteFile(saveName, json);
    }
    public static bool LoadJSON(string saveName, out string json)
    {
        return FileManager.ReadFile(saveName, out json);
    }
}