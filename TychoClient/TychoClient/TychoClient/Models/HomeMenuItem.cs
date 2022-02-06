using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TychoClient.Services;

namespace TychoClient.Models
{
    public enum MenuItemType
    {
        //Browse,
        News,
        ReadCard,
        Login,
        BarMode,
        Transaction
    }
    public class HomeMenuItem : INotifyPropertyChanged
    {
        private bool _isVisible = true;
        private bool _isSelected;

        public MenuItemType Id { get; set; }

        public string Title { get; set; }

        public SelectionWatcher Watcher { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                Watcher.IsSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public bool IsVisible
        {
            get => _isVisible; set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }
        public bool AdminOnly { get; set; }

        //public void NotifySelectedChanged() => OnPropertyChanged(nameof(IsSelected));

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SelectionWatcher
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }

            set
            {
                if (value)
                {
                    this.Log($"{Enum.GetName(typeof(MenuItemType), WatchedMenuItem)}Watcher reports Selection!");
                    GotSelected?.Invoke(this, new EventArgs());
                }
                else
                {
                    this.Log($"{Enum.GetName(typeof(MenuItemType), WatchedMenuItem)}Watcher reports Deselection!");
                    GotDeselected?.Invoke(this, new EventArgs());
                }

                _isSelected = value;
            }
        }

        public MenuItemType WatchedMenuItem { get; private set; }

        public event EventHandler GotSelected;
        public event EventHandler GotDeselected;

        public SelectionWatcher(MenuItemType watchedItem)
        {
            this.Log("Creating new SelectionWatcher for " + Enum.GetName(typeof(MenuItemType), watchedItem));
            WatchedMenuItem = watchedItem;
            AllSelectionWatchers.List.Add(this);
        }
    }

    public static class AllSelectionWatchers
    {
        public static List<SelectionWatcher> List = new List<SelectionWatcher>();
    }
    
}
