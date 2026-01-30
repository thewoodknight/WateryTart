using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using WateryTart.MassClient.Models.Auth;

namespace WateryTart.Settings;

public partial class Settings : INotifyPropertyChanged, ISettings
{
    public IMassCredentials Credentials
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    public string LastSelectedPlayerId
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    public double WindowWidth
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    public double WindowHeight
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    public double WindowPosX
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    public double WindowPosY
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    [JsonIgnore]
    public string Path
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    private bool suppressSave = true;

    public Settings(string path)
    {
        Credentials = new MassCredentials();
        Path = path;
        Load();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Load()
    {
        if (File.Exists(Path))
        {
            var fileData = File.ReadAllText(Path);
            JsonConvert.PopulateObject(fileData, this);
            suppressSave = false;
        }
    }

    private void Save()
    {
        if (!suppressSave)
            File.WriteAllText(Path, JsonConvert.SerializeObject(this));
    }

    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}