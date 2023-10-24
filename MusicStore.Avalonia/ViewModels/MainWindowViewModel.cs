﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using MusicStore.Avalonia.Models;
using ReactiveUI;

namespace MusicStore.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ICommand BuyMusicCommand { get; }
    public Interaction<MusicStoreViewModel, AlbumViewModel?> ShowMusicStoreDialog { get; } = new();

    public ObservableCollection<AlbumViewModel> Albums { get; } = new();

    public MainWindowViewModel()
    {
        BuyMusicCommand = ReactiveCommand.CreateFromObservable(() => Observable.StartAsync(async ct =>
        {
            var store = new MusicStoreViewModel();
            var result = await ShowMusicStoreDialog.Handle(store);
            if(result != null)
            {
                Albums.Add(result);
                await result.SaveToDiskAsync();
            }
        }));

        RxApp.MainThreadScheduler.Schedule(LoadAlbums);
    }

    private async void LoadAlbums()
    {
        var albums = (await Album.LoadCachedAsync()).Select(x => new AlbumViewModel(x));

        foreach(var album in albums)
        {
            Albums.Add(album);
        }

        foreach(var album in Albums.ToList())
        {
            await album.LoadCoverAsync();
        }
    }
}