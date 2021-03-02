using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace DllInjection
{
    public static class Injector
    {
        [DllImport("ManagedMessageBox.dll")]
        public static extern void DllMain();

        public static void Inject(int processId, string dllPath)
        {
            IntPtr dllPathLength = (IntPtr)dllPath.Length;

            // Make sure we don't touch SYSTEM 0 and/or 4 processes
            if ((processId == 4) || (processId == 0))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] SYSTEM process id {0} not allowed.", processId);
                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(1);
            }
            else
            {
                // Get process by id
                try
                {
                    Process localById = Process.GetProcessById(processId);
                    string ProcName = localById.ProcessName;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[+] Process name '{0}' found.", ProcName);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                catch (ArgumentException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!] Error: {0}", e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                    Environment.Exit(1);
                }
            }

            // Open handle to the target process
            IntPtr processHandle = Kernel32.OpenProcess(
                ProcessAccessFlags.All,
                false,
                processId
            );

            if (processHandle == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Handle to target process could not be obtained!");
                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Handle (0x" + processHandle + ") to target process has been be obtained.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Allocate DLL space
            IntPtr dllSpace = Kernel32.VirtualAllocEx(
                processHandle,
                IntPtr.Zero,
                dllPathLength,
                AllocationType.Reserve | AllocationType.Commit,
                MemoryProtection.ExecuteReadWrite
            );

            if (dllSpace == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] DLL space allocation failed.");
                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] DLL space (0x" + dllSpace + ") allocation is successful.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Write DLL content to VAS of target process
            byte[] bytes = Encoding.ASCII.GetBytes(dllPath);
            bool dllWrite = Kernel32.WriteProcessMemory(
                processHandle,
                dllSpace,
                bytes,
                (int)bytes.Length,
                out var bytesread
            );

            if (dllWrite == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Writing DLL content to target process failed.");
                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Writing DLL content to target process is successful.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Get handle to Kernel32.dll and get address for LoadLibraryA
            IntPtr kernel32Handle = Kernel32.GetModuleHandle("Kernel32.dll");
            IntPtr loadLibraryAAddress = Kernel32.GetProcAddress(kernel32Handle, "LoadLibraryA");

            if (loadLibraryAAddress == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Obtaining an addess to LoadLibraryA function has failed.");
                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] LoadLibraryA function address (0x" + loadLibraryAAddress + ") has been obtained.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Create remote thread in the target process
            IntPtr threadHandle = Kernel32.CreateRemoteThread(
                processHandle,
                IntPtr.Zero,
                0,
                loadLibraryAAddress,
                dllSpace,
                0,
                IntPtr.Zero
            );

            if (threadHandle == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Obtaining a handle to remote thread in target process failed.");
                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Obtaining a handle to remote thread (0x" + threadHandle + ") in target process is successful.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Deallocate memory assigned to DLL
            bool freeDllSpace = Kernel32.VirtualFreeEx(
                processHandle,
                dllSpace,
                0,
                AllocationType.Release
            );

            if (freeDllSpace == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Failed to release DLL memory in target process.");
                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Successfully released DLL memory in target process.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Close remote thread handle
            Kernel32.CloseHandle(threadHandle);

            // Close target process handle
            Kernel32.CloseHandle(processHandle);
        }
    }
}