using System;
using System.Windows.Forms;

public class TextAnimation
{
    private Label _targetLabel;
    private Timer _timer;
    private string _fullText;
    private int _currentIndex;
    public bool IsRunning => _timer.Enabled;

    public TextAnimation(Label targetLabel, int intervalMs = 25)
    {
        _targetLabel = targetLabel;

        _timer = new Timer();
        _timer.Interval = intervalMs;

        _timer.Tick += Timer_Tick;
    }

    public void StartAnimation(string text)
    {
        _timer.Stop();
        _fullText = text ?? "";
        _currentIndex = 0;
        _targetLabel.Text = "";
        _timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        if (_currentIndex < _fullText.Length)
        {
            _targetLabel.Text += _fullText[_currentIndex];
            _currentIndex++;
        }
        else
        {
            _timer.Stop();
        }
    }
    public void Skip()
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
            _targetLabel.Text = _fullText;
        }
    }
}