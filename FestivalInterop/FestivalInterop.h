// FestivalInterop.h

#pragma once

#include "festival.h"

namespace Festival 
{
	public ref class FestivalInterop
	{
        public:
        FestivalInterop()
        {
        };
		
        static void Initialize(System::String^ libDir)
        {
            System::IntPtr ptrToNativeString = System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(libDir);
            festival_libdir = static_cast<char*>(ptrToNativeString.ToPointer());
            festival_initialize(TRUE, FESTIVAL_HEAP_SIZE);
        };

        static int SayText(System::String^ text)
        {
            System::IntPtr ptrToNativeString = System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(text);
            try
            {
                const EST_String str(static_cast<char*>(ptrToNativeString.ToPointer()));
                int result = festival_say_text(str);
                System::Runtime::InteropServices::Marshal::FreeHGlobal(ptrToNativeString);
                return result;
            }
            catch (...)
            {
                System::Runtime::InteropServices::Marshal::FreeHGlobal(ptrToNativeString);
                throw;
            }
        };

        static int ExecuteCommand(System::String^ festivalCommand)
        {
            System::IntPtr ptrToNativeString = System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(festivalCommand);
            try
            {
                const EST_String str(static_cast<char*>(ptrToNativeString.ToPointer()));
                int result = festival_eval_command(str);
                System::Runtime::InteropServices::Marshal::FreeHGlobal(ptrToNativeString);
                return result;
            }
            catch (...)
            {
                System::Runtime::InteropServices::Marshal::FreeHGlobal(ptrToNativeString);
                throw;
            }
        };

        static void CleanUp()
        {
            festival_tidy_up();
        };
	};
}
