namespace Blish_HUD.Controls {
    public interface ITabOwner {

        Tab SelectedTab { get; set; }

        TabCollection Tabs { get; }

    }
}
