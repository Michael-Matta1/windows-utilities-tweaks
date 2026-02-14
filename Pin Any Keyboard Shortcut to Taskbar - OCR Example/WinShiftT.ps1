# source of this file is https://github.com/Michael-Matta1/windows-utilities-tweaks (c) Michael-Matta1
# PowerShell script to activate Win+Shift+T - Silent version
# This script properly holds Windows and Shift keys while pressing T with no output

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public class KeyboardSender {
    [DllImport("user32.dll", SetLastError = true)]
    public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
    
    public const int VK_LWIN = 0x5B;
    public const int VK_SHIFT = 0x10;
    public const int VK_T = 0x54;
    public const uint KEYEVENTF_KEYUP = 0x0002;
    
    public static void SendWinShiftT() {
        // Press and hold Windows key
        keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
        
        // Small delay to ensure Windows key is registered
        System.Threading.Thread.Sleep(50);
        
        // Press and hold Shift key
        keybd_event(VK_SHIFT, 0, 0, UIntPtr.Zero);
        
        // Small delay
        System.Threading.Thread.Sleep(50);
        
        // Press T key
        keybd_event(VK_T, 0, 0, UIntPtr.Zero);
        
        // Small delay
        System.Threading.Thread.Sleep(50);
        
        // Release T key
        keybd_event(VK_T, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        
        // Release Shift key
        keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        
        // Release Windows key
        keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }
}
"@ -ErrorAction SilentlyContinue | Out-Null

# Send Win+Shift+T key combination
[KeyboardSender]::SendWinShiftT()