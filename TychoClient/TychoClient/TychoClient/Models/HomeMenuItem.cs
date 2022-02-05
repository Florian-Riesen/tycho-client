using System;
using System.Collections.Generic;
using System.Text;
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
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }

        public SelectionWatcher Watcher { get; set; }

        public bool IsVisible { get; set; } = true;

        public bool AdminOnly { get; set; }
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
