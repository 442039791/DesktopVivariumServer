using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SettingSaveData
{
    public string language;
    public Vector2Int WinowPos;
    public float WinowScale;
    public BioTankSaveData[] BioTankSaveDatas;

    public static SettingSaveData GetNewSaveData()
    {
        SettingSaveData saveData = new SettingSaveData();
        saveData.language = LocalizationManager.GetCurrentLanguage();
        saveData.WinowPos = DirectWindowDrag.Instance.GetWindowPosition();
        saveData.WinowScale = WallpaperManager.Instance.GetWindowScale();
        saveData.BioTankSaveDatas = WindowManager.Instance.GetSaveData();
        if (saveData.BioTankSaveDatas == null)
        {
            List<BioTankSaveData> saveDatas = new List<BioTankSaveData>();
            BioTankSaveData saveData1 = new BioTankSaveData();
            saveData1.Id = 0;
            saveData1.Position = WallpaperManager.Instance.GetStartPosition();
            saveData1.Size = WallpaperManager.Instance.GetStartSize();
            saveDatas.Add(saveData1);
            saveData.BioTankSaveDatas = saveDatas.ToArray();
        }
        return saveData;
    }
}
[System.Serializable]
public class BioTankSaveData
{
    public int Id;
    public Vector2Int Position;
    public Vector2Int Size;
}
