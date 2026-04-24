using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class ConfigScreen
{
    public static void Build(
        View root,
        ConfigViewModel viewModel,
        TuiNavigationState navigation,
        Action onBack,
        Action onQuit)
    {
        var topBar = new Label(" TermBullet \u2500 Config \u2500 data:local \u2500 ai:off \u2500 sync:idle \u2500 mode:normal")
        {
            X = 0, Y = 0, Width = Dim.Fill()
        };

        var footer = new Label(" Enter edit  Tab focus  ? help  q quit")
        {
            X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill()
        };

        // Panel 1: Sections
        var sectionsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Sections", navigation, 0))
        {
            X = 0, Y = 1, Width = Dim.Percent(22), Height = Dim.Fill(1)
        };
        var sectionsList = new ListView(viewModel.Sections.ToArray())
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        sectionsPanel.Add(sectionsList);

        // Panel 2: Options
        var optionsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Options", navigation, 1))
        {
            X = Pos.Right(sectionsPanel), Y = 1, Width = Dim.Percent(40), Height = Dim.Fill(1)
        };
        var optionKeys = viewModel.OptionsForActiveSection.Keys.ToArray();
        var optionsList = new ListView(optionKeys.Length > 0 ? optionKeys : new[] { "(no options)" })
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        optionsPanel.Add(optionsList);

        // Panel 3: Value / Preview
        var valuePanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Value / Preview", navigation, 2))
        {
            X = Pos.Right(optionsPanel), Y = 1, Width = Dim.Fill(), Height = Dim.Fill(1)
        };
        var selectedOpt = viewModel.SelectedOption;
        var valueLines = selectedOpt.HasValue
            ? new[] { selectedOpt.Value.Key, selectedOpt.Value.Value }
            : new[] { "(nothing selected)" };
        var valueList = new ListView(valueLines)
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        valuePanel.Add(valueList);

        var panels = new[] { sectionsPanel, optionsPanel, valuePanel };
        var panelTitles = new[] { "Sections", "Options", "Value / Preview" };
        var focusTargets = new View[] { sectionsList, optionsList, valueList };
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        root.Add(topBar, sectionsPanel, optionsPanel, valuePanel, footer);

        root.KeyPress += args =>
        {
            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.Config);
                args.Handled = true;
                return;
            }

            switch (args.KeyEvent.Key)
            {
                case Key.Tab:
                    navigation.MoveNextPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    args.Handled = true;
                    break;
                case Key.BackTab:
                    navigation.MovePreviousPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    args.Handled = true;
                    break;
                case Key.q:
                    onQuit();
                    args.Handled = true;
                    break;
                case Key.Esc:
                    onBack();
                    args.Handled = true;
                    break;
            }
        };

        sectionsList.SelectedItemChanged += _ =>
        {
            var index = sectionsList.SelectedItem;
            if (index < 0 || index >= viewModel.Sections.Count)
            {
                return;
            }

            viewModel.ChangeSection(viewModel.Sections[index]);
            var optionKeys = viewModel.OptionsForActiveSection.Keys.ToArray();
            TuiScreenUtilities.RefreshListView(optionsList, optionKeys.Length > 0 ? optionKeys : ["(no options)"]);
            if (viewModel.SelectedOptionIndex >= 0)
            {
                optionsList.SelectedItem = viewModel.SelectedOptionIndex;
            }
            TuiScreenUtilities.RefreshListView(valueList, BuildValueLines(viewModel.SelectedOption));
        };

        optionsList.SelectedItemChanged += _ =>
        {
            var diff = optionsList.SelectedItem - viewModel.SelectedOptionIndex;
            if (diff > 0)
                for (var i = 0; i < diff; i++) viewModel.SelectNextOption();
            else if (diff < 0)
                for (var i = 0; i < -diff; i++) viewModel.SelectPreviousOption();

            TuiScreenUtilities.RefreshListView(valueList, BuildValueLines(viewModel.SelectedOption));
        };
    }

    private static string[] BuildValueLines((string Key, string Value)? selectedOpt) =>
        selectedOpt.HasValue
            ? [selectedOpt.Value.Key, selectedOpt.Value.Value]
            : ["(nothing selected)"];
}
