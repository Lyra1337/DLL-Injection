using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Text;

namespace DllInjection
{
    class Injector
    {
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
            IntPtr processHandle = Native.OpenProcess(
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
            IntPtr dllSpace = Native.VirtualAllocEx(
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
            bool dllWrite = Native.WriteProcessMemory(
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
            IntPtr kernel32Handle = Native.GetModuleHandle("Kernel32.dll");
            IntPtr loadLibraryAAddress = Native.GetProcAddress(kernel32Handle, "LoadLibraryA");

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
            IntPtr remoteThreadHandle = Native.CreateRemoteThread(
                processHandle,
                IntPtr.Zero,
                0,
                loadLibraryAAddress,
                dllSpace,
                0,
                IntPtr.Zero
            );

            if (remoteThreadHandle == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Obtaining a handle to remote thread in target process failed.");
                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Obtaining a handle to remote thread (0x" + remoteThreadHandle + ") in target process is successful.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Deallocate memory assigned to DLL
            bool freeDllSpace = Native.VirtualFreeEx(
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
            Native.CloseHandle(remoteThreadHandle);

            // Close target process handle
            Native.CloseHandle(processHandle);
        }
    }
}