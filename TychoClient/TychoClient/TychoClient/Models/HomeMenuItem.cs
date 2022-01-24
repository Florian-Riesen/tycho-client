using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
