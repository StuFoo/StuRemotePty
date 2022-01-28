///
///	<ItemGroup>
///		<PackageReference Include="Vanara.PInvoke.Kernel32" Version="2.3.6" />
///	</ItemGroup>
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace StuRemotePty.Client
{
    public class ConsoInputContentWindows : IConsoInputContent
    {
        HFILE handle;
        IntPtr _handle;
        private const int BufferSize = 1;

        Kernel32.CONSOLE_INPUT_MODE oldMode;

        public int Init()
        {
            handle = Kernel32.GetStdHandle(Kernel32.StdHandleType.STD_INPUT_HANDLE);

            if (!Kernel32.GetConsoleMode(handle, out Kernel32.CONSOLE_INPUT_MODE mode))
                throw Marshal.GetExceptionForHR(MakeHRFromErrorCode(Marshal.GetLastWin32Error())) ?? new Exception("GetConsoleMode error");
            oldMode = mode;
            mode |= Kernel32.CONSOLE_INPUT_MODE.ENABLE_WINDOW_INPUT;
            mode |= Kernel32.CONSOLE_INPUT_MODE.ENABLE_VIRTUAL_TERMINAL_INPUT;
            mode &= ~Kernel32.CONSOLE_INPUT_MODE.ENABLE_ECHO_INPUT;
            mode &= ~Kernel32.CONSOLE_INPUT_MODE.ENABLE_LINE_INPUT;

            if (!Kernel32.SetConsoleMode(handle, mode))
                throw Marshal.GetExceptionForHR(MakeHRFromErrorCode(Marshal.GetLastWin32Error())) ?? new Exception("SetConsoleMode error");

            _handle = handle.DangerousGetHandle();

            SetVirtualTerminalProcessing();
            return 0;
        }

        //返回的会附带颜色等控制字符 将为 VT100 和类似控制字符序列分析字符，这些字符序列可控制光标移动、颜色/字体模式以及其他也可通过现有控制台 API 执行的操作。
        private void SetVirtualTerminalProcessing()
        {
            HFILE handle;
            handle = Kernel32.GetStdHandle(Kernel32.StdHandleType.STD_OUTPUT_HANDLE);

            if (!Kernel32.GetConsoleMode(handle, out Kernel32.CONSOLE_OUTPUT_MODE mode))
                return;
            mode |= Kernel32.CONSOLE_OUTPUT_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

            if (!Kernel32.SetConsoleMode(handle, mode))
                return;

        }


        private static int MakeHRFromErrorCode(int errorCode)
        {
            // Don't convert it if it is already an HRESULT
            if ((0xFFFF0000 & errorCode) != 0)
            {
                return errorCode;
            }

            return unchecked(((int)0x80070000) | errorCode);
        }

        public char Read()
        {
            var records = new Kernel32.INPUT_RECORD[BufferSize];

            // begin input loop
            while (true)
            {
                var readSuccess = Kernel32.ReadConsoleInput(_handle, records, BufferSize, out var recordsRead);

                // some of the arithmetic here is deliberately more explicit than it needs to be 
                // in order to show how 16-bit unicode WCHARs are packed into the buffer. The console
                // subsystem is one of the last bastions of UCS-2, so until UTF-16 is fully adopted
                // the two-byte character assumptions below will hold. 
                if (!readSuccess && recordsRead <= 0)
                {
                    continue;
                }

                for (var index = 0; index < recordsRead; index++)
                {
                    var record = records[index];

                    if (record.EventType == Kernel32.EVENT_TYPE.KEY_EVENT)
                    {
                        // skip key up events - if not, every key will be duped in the stream
                        if (!record.Event.KeyEvent.bKeyDown) continue;

                        // pack ucs-2/utf-16le/unicode chars into position in our byte[] buffer.
                        return record.Event.KeyEvent.uChar;
                    }
                }
            }
        }


        public int CloseRead()
        {
            if (!Kernel32.SetConsoleMode(handle, oldMode))
                throw Marshal.GetExceptionForHR(MakeHRFromErrorCode(Marshal.GetLastWin32Error())) ?? new Exception("SetConsoleMode error");

            return 0;
        }
    }
}