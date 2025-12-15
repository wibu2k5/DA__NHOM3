using System;
using System.Windows.Forms;

namespace DOAN_Nhom3; 
internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }
}