using System.Windows;
using BananaLove.Utility;

namespace BananaLove.View;

public class WindowView : Window
{
    public Login LoginData;

    public WindowView(Login loginData)
    {
        LoginData = loginData;
    }

    protected WindowView()
    {
        
    }
}