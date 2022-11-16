using NAudio.Wave;
using QuickMusic3.Core;
using QuickMusic3.MVVM.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace QuickMusic3.MVVM.ViewModel;

public class SharedState : ObservableObject
{
    public Player Player { get; } = new();
    public PlayHistory History { get; }
    private Theme active_theme;
    public Theme ActiveTheme
    {
        get { return active_theme; }
        private set { active_theme = value; OnPropertyChanged(); }
    }
    public bool LyricsEnabled
    {
        get { return Properties.Settings.Default.LyricsVisible; }
        set { Properties.Settings.Default.LyricsVisible = value; OnPropertyChanged(); }
    }

    public ICommand PlayPauseCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PrevCommand { get; }
    public ICommand ChangeRepeatCommand { get; }
    public ICommand ChangeMuteCommand { get; }
    public ICommand ChangeShuffleCommand { get; }
    public ICommand ChangeVolumeCommand { get; }
    public ICommand SeekCommand { get; }
    public ICommand ChangeThemeCommand { get; }
    public ICommand ChangeLyricsEnabledCommand { get; }
    public ICommand ForwardCommand { get; }
    public ICommand BackwardCommand { get; }

    public SharedState()
    {
        History = new(Player);
        PlayPauseCommand = new RelayCommand(() =>
        {
            if (Player.PlayState == PlaybackState.Playing)
                Player.Pause();
            else
                Player.Play();
        });
        NextCommand = new RelayCommand(async () =>
        {
            await Player.NextAsync();
            History.Add();
        });
        PrevCommand = new RelayCommand(async () =>
        {
            if (Player.CurrentTime > TimeSpan.FromSeconds(2))
                Player.CurrentTime = TimeSpan.Zero;
            else
            {
                await Player.PreviousAsync();
                History.Add();
            }
        });
        ForwardCommand = new RelayCommand(async () => await History.ForwardAsync());
        BackwardCommand = new RelayCommand(async () => await History.BackwardAsync());
        ChangeRepeatCommand = new RelayCommand(() =>
        {
            if (Player.RepeatMode == RepeatMode.RepeatAll)
                Player.RepeatMode = RepeatMode.RepeatOne;
            else if (Player.RepeatMode == RepeatMode.RepeatOne)
                Player.RepeatMode = RepeatMode.PlayAll;
            else
                Player.RepeatMode = RepeatMode.RepeatAll;
        });
        ChangeMuteCommand = new RelayCommand(() => { Player.Muted = !Player.Muted; });
        ChangeShuffleCommand = new RelayCommand(() => { Player.SetShuffle(!Player.IsShuffled); });
        ChangeVolumeCommand = new RelayCommand<float>(n => { Player.Volume = Math.Clamp(Player.Volume + n, 0, 1); });
        ChangeLyricsEnabledCommand = new RelayCommand(() => { LyricsEnabled = !LyricsEnabled; });
        SeekCommand = new RelayCommand<double>(n => Player.CurrentTime += TimeSpan.FromSeconds(n));
        ChangeThemeCommand = new RelayCommand(() =>
        {
            var all_themes = Theme.DefaultThemes.Keys.Concat(Properties.Settings.Default.ImportedThemes).ToList();
            int index = all_themes.IndexOf(Properties.Settings.Default.SelectedTheme);
            index++;
            if (index >= all_themes.Count)
                index = 0;
            OpenTheme(all_themes[index]);
        });
        if (Properties.Settings.Default.ImportedThemes == null)
            Properties.Settings.Default.ImportedThemes = new();
        if (String.IsNullOrEmpty(Properties.Settings.Default.SelectedTheme))
            Properties.Settings.Default.SelectedTheme = Theme.DefaultThemes.Keys.First();
        OpenTheme(Properties.Settings.Default.SelectedTheme);
    }

    public void OpenTheme(string theme)
    {
        Properties.Settings.Default.SelectedTheme = theme;
        if (Theme.DefaultThemes.TryGetValue(theme, out var def))
        {
            ActiveTheme = def;
            return;
        }
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        try
        {
            using var file = File.OpenText(theme);
            ActiveTheme = deserializer.Deserialize<Theme>(file);
        }
        catch (FileNotFoundException)
        {
            Properties.Settings.Default.ImportedThemes.Remove(theme);
            Debug.WriteLine($"Removed theme {theme} because file not found");
        }
        catch
        { }
    }
}
