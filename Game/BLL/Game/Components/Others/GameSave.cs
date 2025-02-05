using System.Text.Json;
using BLL.Utilities;
using DAL;

namespace BLL.Game.Components.Others
{
    [Serializable]
    public class GameSave : GameComponent
    {
        public DateTime SaveTime { get; set; } = DateTime.Now;
        public RunData RunData { get; set; } = new();

        public GameSave() {}

        public GameSave(RunData gameData, DateTime? saveTime = null) : base("UnnamedSave")
        {
            RunData = gameData;
            SaveTime = saveTime ?? DateTime.Now;
        }
        
        public void SaveAs(string? saveName)
        {
            Name = saveName ?? "UnnamedSave";
            RunData.SaveTime();
            FileManager.WriteJson(FileManager.FolderNames.Saves, Name, this);
        }

        public static List<GameSave> LoadGameSaves(out string? error)
        {
            List<GameSave> loadedSaves = [];
            error = null;

            try
            {
                foreach (string file in FileManager.ReadAllJson(FileManager.FolderNames.Saves))
                {
                    var loadedSave = GenericUtilities.FromJson<GameSave>(file);

                    if (loadedSave != null)
                        loadedSaves.Add(loadedSave);
                    else
                        throw new JsonException();
                }

                if (loadedSaves.Count == 0)
                    throw new FileNotFoundException();
            }
            catch (JsonException)
            {
                error = "Corrupted saves";
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
            {
                error = "No save found";
            }

            return loadedSaves;
        }
    }
}