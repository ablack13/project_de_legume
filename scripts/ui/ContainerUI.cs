using Godot;
using ProiectDeLegume.Scripts.Inventory;
using ProiectDeLegume.Scripts.Localization;
using ProiectDeLegume.Scripts.World;

namespace ProiectDeLegume.Scripts.UI;

public partial class ContainerUI : PanelContainer
{
    public bool IsOpen { get; private set; }

    private ContainerData _currentContainer;
    private Inventory.Inventory _inventory;
    private Player.PlayerStats _stats;
    private VBoxContainer _itemList;
    private Label _titleLabel;
    private System.Action _onChanged;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(320, 300);
        Size = new Vector2(320, 300);
        Position = new Vector2(20, 20);
        Visible = false;

        // Dark background
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.08f, 0.08f, 0.12f, 0.86f);
        style.BorderWidthBottom = style.BorderWidthTop = style.BorderWidthLeft = style.BorderWidthRight = 1;
        style.BorderColor = new Color(0.3f, 0.3f, 0.35f);
        AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        AddChild(vbox);

        _titleLabel = new Label();
        _titleLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(_titleLabel);

        var sep = new HSeparator();
        vbox.AddChild(sep);

        _itemList = new VBoxContainer();
        vbox.AddChild(_itemList);

        var spacer = new Control { SizeFlagsVertical = SizeFlags.ExpandFill };
        vbox.AddChild(spacer);

        var closeHint = new Label();
        closeHint.Text = Lang.Get("container.close");
        closeHint.AddThemeFontSizeOverride("font_size", 12);
        closeHint.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.4f));
        vbox.AddChild(closeHint);
    }

    public void Open(ContainerData container, Inventory.Inventory inventory, Player.PlayerStats stats, System.Action onChanged)
    {
        _currentContainer = container;
        _inventory = inventory;
        _stats = stats;
        _onChanged = onChanged;
        IsOpen = true;
        Visible = true;

        _titleLabel.Text = container.DisplayName;
        RefreshItems();
    }

    public void Close()
    {
        IsOpen = false;
        Visible = false;
        _currentContainer = null;
    }

    private void RefreshItems()
    {
        // Clear old
        foreach (var child in _itemList.GetChildren())
            child.QueueFree();

        var items = _currentContainer.Items;
        if (items.Count == 0)
        {
            var empty = new Label();
            empty.Text = Lang.Get("container.empty");
            empty.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.4f));
            _itemList.AddChild(empty);
            return;
        }

        for (int i = 0; i < items.Count; i++)
        {
            var stack = items[i];
            var row = new HBoxContainer();

            var nameLabel = new Label();
            string countStr = stack.Count > 1 ? $" x{stack.Count}" : "";
            nameLabel.Text = $"{stack.Def.LocalizedName}{countStr}";
            nameLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            row.AddChild(nameLabel);

            var takeBtn = new Button();
            takeBtn.Text = Lang.Get("container.take");
            takeBtn.CustomMinimumSize = new Vector2(60, 0);
            int capturedIdx = i;
            takeBtn.Pressed += () =>
            {
                if (_inventory.CanAdd(stack.Def))
                {
                    _inventory.Add(stack.Def);
                    if (stack.Count > 1) stack.Count--;
                    else _currentContainer.Items.Remove(stack);
                    RefreshItems();
                    _onChanged?.Invoke();
                }
            };
            row.AddChild(takeBtn);

            var weightLabel = new Label();
            weightLabel.Text = $"{stack.TotalWeight:F1}{Lang.Get("unit.kg")}";
            weightLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            row.AddChild(weightLabel);

            _itemList.AddChild(row);
        }
    }
}
