using Godot;

public partial class Main : Control
{
    private int _counter;
    private Label _label;

    public override void _Ready()
    {
        _label = GetNode<Label>("Label");
    }

    public override void _Process(double delta)
    {
        _counter++;
        _label.Text = _counter.ToString();
    }
}
