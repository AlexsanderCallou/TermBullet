using Terminal.Gui;

namespace TermBullet.Tui.Screens;

public static class PlanningScreen
{
    public static void Build(
        View root,
        Action onBack,
        Action onQuit)
    {
        var topBar = new Label(" TermBullet - Planning")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };

        var panel = new FrameView("Future AI Planning")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(1)
        };

        var message = new Label("Planning will become the AI-assisted workspace for turning goals into tasks.")
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2)
        };
        var detail = new Label("For now, this screen is intentionally empty. V1 keeps planning manual and local-first.")
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill(2)
        };
        var future = new Label("Future scope: goals, context selection, task suggestions, and preview before saving.")
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill(2)
        };
        panel.Add(message, detail, future);

        var footer = new Label(" ? help  Esc back  q quit")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        root.Add(topBar, panel, footer);

        root.KeyPress += args =>
        {
            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(Tui.Navigation.TuiScreen.Planning);
                args.Handled = true;
                return;
            }

            switch (args.KeyEvent.Key)
            {
                case Key.Esc:
                    onBack();
                    args.Handled = true;
                    break;
                case Key.q:
                    onQuit();
                    args.Handled = true;
                    break;
            }
        };
    }
}
