namespace TermBullet.Tui;

public static class TuiAsciiControls
{
    public static string Checkbox(bool isChecked) =>
        isChecked ? "[ x ]" : "[   ]";

    public static string Radio(bool isSelected) =>
        isSelected ? "( x )" : "(   )";

    public static string CheckboxLine(bool isChecked, string label) =>
        $"{Checkbox(isChecked)} {label}";

    public static string RadioLine(bool isSelected, string label) =>
        $"{Radio(isSelected)} {label}";

    public static string ActionLine(bool isSelected, string label) =>
        $"{(isSelected ? ">" : " ")} {label}";
}
