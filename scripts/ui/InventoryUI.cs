using Godot;
using ProiectDeLegume.Scripts.Inventory;
using ProiectDeLegume.Scripts.Localization;
using ProiectDeLegume.Scripts.Player;

namespace ProiectDeLegume.Scripts.UI;

public partial class InventoryUI : PanelContainer
{
    public bool IsOpen { get; private set; }

    private Inventory.Inventory _inventory;
    private PlayerStats _stats;
    private VBoxContainer _itemList;
    private Label _titleLabel;
    private Label _weightLabel;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(340, 400);
        Size = new Vector2(340, 400);
        Position = new Vector2(920, 20);
        Visible = false;

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.08f, 0.08f, 0.12f, 0.86f);
        style.BorderWidthBottom = style.BorderWidthTop = style.BorderWidthLeft = style.BorderWidthRight = 1;
        style.BorderColor = new Color(0.3f, 0.3f, 0.35f);
        AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        AddChild(vbox);

        // Header row
        var header = new HBoxContainer();
        _titleLabel = new Label();
        _titleLabel.Text = Lang.Get("inventory.title");
        _titleLabel.AddThemeFontSizeOverride("font_size", 18);
        _titleLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        header.AddChild(_titleLabel);

        _weightLabel = new Label();
        _weightLabel.AddThemeFontSizeOverride("font_size", 13);
        _weightLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        header.AddChild(_weightLabel);
        vbox.AddChild(header);

        var sep = new HSeparator();
        vbox.AddChild(sep);

        _itemList = new VBoxContainer();
        vbox.AddChild(_itemList);

        var spacer = new Control { SizeFlagsVertical = SizeFlags.ExpandFill };
        vbox.AddChild(spacer);

        var closeHint = new Label();
        closeHint.Text = Lang.Get("inventory.close");
        closeHint.AddThemeFontSizeOverride("font_size", 12);
        closeHint.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.4f));
        vbox.AddChild(closeHint);
    }

    public void Open(Inventory.Inventory inventory, PlayerStats stats)
    {
        _inventory = inventory;
        _stats = stats;
        IsOpen = true;
        Visible = true;
        RefreshItems();
    }

    public void Close()
    {
        IsOpen = false;
        Visible = false;
    }

    public void Toggle(Inventory.Inventory inventory, PlayerStats stats)
    {
        if (IsOpen) Close();
        else Open(inventory, stats);
    }

    public void RefreshItems()
    {
        if (_inventory == null) return;

        _weightLabel.Text = $"{_inventory.CurrentWeight:F1} / {_inventory.MaxWeight:F0} {Lang.Get("unit.kg")}";

        foreach (var child in _itemList.GetChildren())
            child.QueueFree();

        if (_inventory.Items.Count == 0)
        {
            var empty = new Label();
            empty.Text = Lang.Get("inventory.empty");
            empty.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.4f));
            _itemList.AddChild(empty);
            return;
        }

        for (int i = 0; i < _inventory.Items.Count; i++)
        {
            var stack = _inventory.Items[i];
            var row = new HBoxContainer();

            // Category color indicator
            var catColor = stack.Def.Category switch
            {
                "food" => Colors.Green,
                "medical" => Colors.Red,
                "tool" => Colors.Yellow,
                "weapon" => Colors.Orange,
                "clothing" => Colors.Cyan,
                _ => Colors.Gray
            };
            var colorRect = new ColorRect();
            colorRect.CustomMinimumSize = new Vector2(4, 20);
            colorRect.Color = catColor;
            row.AddChild(colorRect);

            // Name
            var nameLabel = new Label();
            string countStr = stack.Count > 1 ? $" x{stack.Count}" : "";
            nameLabel.Text = $"{stack.Def.LocalizedName}{countStr}";
            nameLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            row.AddChild(nameLabel);

            int capturedIdx = i;

            // Use button (if usable)
            if (stack.Def.IsUsable)
            {
                var useBtn = new Button();
                useBtn.Text = stack.Def.UseLabel;
                useBtn.CustomMinimumSize = new Vector2(60, 0);
                useBtn.Pressed += () =>
                {
                    switch (stack.Def.UseAction)
                    {
                        case "eat": _stats.Eat(stack.Def.HungerRestore); break;
                        case "drink": _stats.Drink(stack.Def.ThirstRestore, stack.Def.HungerRestore); break;
                        case "heal": _stats.Heal(stack.Def.HpRestore); break;
                    }
                    _inventory.Remove(capturedIdx);
                    RefreshItems();
                };
                row.AddChild(useBtn);
            }

            // Drop button
            var dropBtn = new Button();
            dropBtn.Text = Lang.Get("inventory.drop");
            dropBtn.CustomMinimumSize = new Vector2(60, 0);
            dropBtn.Pressed += () =>
            {
                _inventory.Remove(capturedIdx);
                RefreshItems();
            };
            row.AddChild(dropBtn);

            // Weight
            var weightLabel = new Label();
            weightLabel.Text = $"{stack.TotalWeight:F1}{Lang.Get("unit.kg")}";
            weightLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            row.AddChild(weightLabel);

            _itemList.AddChild(row);
        }
    }
}
