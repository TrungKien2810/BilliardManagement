using System.Windows;
using System.Windows.Input;

namespace Billiards.UI.Windows;

public partial class InputDialog : Window
{
    public string Answer { get; private set; } = string.Empty;

    public InputDialog(string question, string title, string defaultAnswer = "")
    {
        InitializeComponent();
        txtQuestion.Text = question;
        Title = title;
        txtAnswer.Text = defaultAnswer;
        txtAnswer.SelectAll();
        txtAnswer.Focus();
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
        Answer = txtAnswer.Text;
        DialogResult = true;
    }

    private void txtAnswer_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            btnOk_Click(sender, e);
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

