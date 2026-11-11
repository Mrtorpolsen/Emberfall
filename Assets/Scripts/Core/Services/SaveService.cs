using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SaveService : GlobalSystem<SaveService>
{
    public SaveGame Current { get; private set; }

    private string savePath;

    // Saves are written one at a time; each request is numbered so an older snapshot never overwrites a newer one
    private readonly SemaphoreSlim saveLock = new(1, 1);
    private int requestedSaveSequence;
    private int writtenSaveSequence;

    public event Func<Task> OnSaveLoaded;

    protected override void Awake()
    {
        base.Awake();
    }

    public void InitializeForPlayer(string playerId)
    {
        savePath = Path.Combine(
            Application.persistentDataPath,
            $"{playerId}_save.json"
        );
    }

    // Loads the save file, then replaces its progress with the server's (docs/cloud-save.md). Offline, or
    // when the server can't be reached, the local progress is used.
    public async Task Load()
    {
        if (!ValidateSavePath())
            return;

        bool needsWrite = await LoadLocalAsync();

        ServerProgress serverProgress = await ServerProgressSync.LoadOrImportAsync(Current.Progress);

        if (serverProgress != null)
        {
            Current.Progress = serverProgress;
            needsWrite = true;
        }

        if (needsWrite)
        {
            await SaveAsync();
        }

        await InvokeOnSaveLoaded();
    }

    // Sets Current from the save file, or to a new save when there is none or it can't be used.
    // True when Current differs from the file (new or migrated) and should be written.
    private async Task<bool> LoadLocalAsync()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found, creating new save file");
            Current = NewSave();
            return true;
        }

        string json = await Task.Run(() => File.ReadAllText(savePath));
        SaveLoadResult result = SaveSerializer.Deserialize(json);

        switch (result.Status)
        {
            case SaveLoadStatus.Corrupt:
                Debug.LogError($"Failed to parse save file, creating new save: {result.Error}");
                await BackupSave(json, "corrupt");
                Current = NewSave();
                return true;

            case SaveLoadStatus.Unsupported:
                Debug.LogError($"Save version {result.Version} is too old to migrate, creating new save");
                await BackupSave(json, $"unsupported-v{result.Version}");
                Current = NewSave();
                return true;

            case SaveLoadStatus.Failed:
                // Keep a copy so it can be recovered
                Debug.LogError($"Failed to load save version {result.Version}, creating new save: {result.Error}");
                await BackupSave(json, $"failed-v{result.Version}");
                Current = NewSave();
                return true;
        }

        Current = result.Save;
        return result.Status == SaveLoadStatus.Migrated;
    }

    private static SaveGame NewSave()
    {
        return SaveSerializer.EnsureDefaults(new SaveGame
        {
            Version = CurrentSaveVersion.CURRENT_SAVE_VERSION
        });
    }

    public Task SaveAsync()
    {
        // Serializing on the main thread at call time is the snapshot
        string json = SaveSerializer.Serialize(Current);
        return SaveInternal(json, ++requestedSaveSequence);
    }

    public void Save()
    {
        _ = SaveAsync().ContinueWith(t =>
        {
            if (t.Exception != null)
                Debug.LogError(t.Exception);
        });
    }

    private async Task SaveInternal(string json, int sequence)
    {
        await saveLock.WaitAsync();

        try
        {
            // A newer snapshot was already written; writing this one would roll the save back
            if (sequence <= writtenSaveSequence)
                return;

            await Task.Run(() => WriteAtomic(savePath, json));
            writtenSaveSequence = sequence;
        }
        finally
        {
            saveLock.Release();
        }
    }

    // Deletes the save with its temp file and backups (account deletion). Waits for a write in progress, and no
    // later save is written, so nothing recreates the file before the game restarts.
    public async Task DeleteSaveFilesAsync()
    {
        if (!ValidateSavePath())
            return;

        await saveLock.WaitAsync();

        try
        {
            writtenSaveSequence = int.MaxValue;

            string directory = Path.GetDirectoryName(savePath);
            string fileName = Path.GetFileName(savePath);

            await Task.Run(() =>
            {
                // <id>_save.json, .tmp and .<reason>-<date> backups
                foreach (string file in Directory.GetFiles(directory, fileName + "*"))
                {
                    File.Delete(file);
                }
            });
        }
        finally
        {
            saveLock.Release();
        }
    }

    // Write to a temp file, then swap it in, so a crash or kill mid-write can't leave a truncated save.
    // Public for tests.
    public static void WriteAtomic(string path, string contents)
    {
        string tempPath = path + ".tmp";

        File.WriteAllText(tempPath, contents);

        if (File.Exists(path))
        {
            File.Replace(tempPath, path, null);
        }
        else
        {
            File.Move(tempPath, path);
        }
    }

    // Copies a save we can't use next to the real one before it gets replaced, e.g. <id>_save.json.corrupt-20261022-143000
    private async Task BackupSave(string json, string reason)
    {
        string backupPath = $"{savePath}.{reason}-{DateTime.Now:yyyyMMdd-HHmmss}";

        await Task.Run(() =>
        {
            File.WriteAllText(backupPath, json);
        });
    }

    private bool ValidateSavePath()
    {
        if (string.IsNullOrEmpty(savePath))
        {
            Debug.LogError("SaveService not initialized with player ID");
            return false;
        }

        return true;
    }

    private async Task InvokeOnSaveLoaded()
    {
        if (OnSaveLoaded == null)
            return;

        var handlers = OnSaveLoaded.GetInvocationList();
        var tasks = new List<Task>();

        foreach (Func<Task> handler in handlers)
        {
            tasks.Add((Task)handler());
        }

        await Task.WhenAll(tasks);
    }
}
