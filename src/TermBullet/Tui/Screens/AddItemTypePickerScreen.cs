using Terminal.Gui;
using TermBullet.Core.Items;

namespace TermBullet.Tui.Screens;

public static class AddItemTypePickerScreen
{
    public static void Build(
        View root,
        Action<ItemType> onChoose,
        Action onCancel)
    {
        var selectedIndex = 0;
        var title = new Label(" Add Item")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };
        var panel = new FrameView("What do you want to add?")
        {
            X = Pos.Center() - 32,
            Y = Pos.Center() - 5,
            Width = 64,
            Height = 10
        };
        var list = new ListView(BuildRows(selectedIndex))
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = 4
        };
        var footer = new Label(" Enter choose  t task  n note  e event  Esc cancel")
        {
            X = 1,
            Y = 6,
            Width = Dim.Fill(2)
        };
        panel.Add(list, footer);
        root.Add(title, panel);

        void Select(int index)
        {
            selectedIndex = index < 0 ? 2 : index > 2 ? 0 : index;
            TuiScreenUtilities.RefreshListView(list, BuildRows(selectedIndex));
            list.SelectedItem = selectedIndex;
        }

        void Confirm()
        {
            onChoose(selectedIndex switch
            {
                1 => ItemType.Note,
                2 => ItemType.Event,
                _ => ItemType.Task
            });
        }

        root.KeyPress += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case Key.CursorUp:
                    Select(selectedIndex - 1);
                    args.Handled = true;
                    break;
                case Key.CursorDown:
                    Select(selectedIndex + 1);
                    args.Handled = true;
                    break;
                case Key.Enter:
                    Confirm();
                    args.Handled = true;
                    break;
                case Key t when t == (Key)'t':
                    onChoose(ItemType.Task);
                    args.Handled = true;
                    break;
                case Key n when n == (Key)'n':
                    onChoose(ItemType.Note);
                    args.Handled = true;
                    break;
                case Key e when e == (Key)'e':
                    onChoose(ItemType.Event);
                    args.Handled = true;
                    break;
                case Key.Esc:
                    onCancel();
                    args.Handled = true;
                    break;
            }
        };

        list.SetFocus();
    }

    private static string[] BuildRows(int selectedIndex)
    {
        var rows = new[]
        {
            "Task   executable work with planned_for",
            "Note   reference or context, no planned date",
            "Event  scheduled appointment with scheduled_at"
        };

        return rows
            .Select((row, index) => $"{(index == selectedIndex ? ">" : " ")} {row}")
            .ToArray();
    }
}
