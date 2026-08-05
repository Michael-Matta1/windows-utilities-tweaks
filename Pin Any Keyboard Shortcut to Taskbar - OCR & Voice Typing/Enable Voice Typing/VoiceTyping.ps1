# PowerShell script to activate Win+H (Voice Typing) - Silent version
# This script properly holds Windows key while pressing H with no output

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public class KeyboardSender {
    [DllImport("user32.dll", SetLastError = true)]
    public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
    
    public const int VK_LWIN = 0x5B;
    public const int VK_H = 0x48;
    public const uint KEYEVENTF_KEYUP = 0x0002;
    
    public static void SendWinH() {
        // Press and hold Windows key
        keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
        
        // Small delay to ensure Windows key is registered
        System.Threading.Thread.Sleep(50);
        
        // Press H key
        keybd_event(VK_H, 0, 0, UIntPtr.Zero);
        
        // Small delay
        System.Threading.Thread.Sleep(50);
        
        // Release H key
        keybd_event(VK_H, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        
        // Release Windows key
        keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }
}
"@ -ErrorAction SilentlyContinue | Out-Null

# Send Win+H key combination
[KeyboardSender]::SendWinH()