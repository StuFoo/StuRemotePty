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

        public int Init()
        {
            handle = Kernel32.GetStdHandle(Kernel32.StdHandleType.STD_INPUT_HANDLE);

            if (!Kernel32.GetConsoleMode(handle, out Kernel32.CONSOLE_INPUT_MODE mode))
                throw Marshal.GetExceptionForHR(MakeHRFromErrorCode(Marshal.GetLastWin32Error())) ?? new Exception("GetConsoleMode error");

            mode |= Kernel32.CONSOLE_INPUT_MODE.ENABLE_WINDOW_INPUT;
            mode |= Kernel32.CONSOLE_INPUT_MODE.ENABLE_VIRTUAL_TERMINAL_INPUT;
            mode &= ~Kernel32.CONSOLE_INPUT_MODE.ENABLE_ECHO_INPUT;
            mode &= ~Kernel32.CONSOLE_INPUT_MODE.ENABLE_LINE_INPUT;

            if (!Kernel32.SetConsoleMode(handle, mode))
                throw Marshal.GetExceptionForHR(MakeHRFromErrorCode(Marshal.GetLastWin32Error())) ?? new Exception("SetConsoleMode error");

            _handle = handle.DangerousGetHandle();

            return 0;
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
    }
}