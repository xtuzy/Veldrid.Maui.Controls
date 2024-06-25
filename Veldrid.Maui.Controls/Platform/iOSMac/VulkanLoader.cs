using Silk.NET.Core.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veldrid.Maui.Controls.Platform.iOSMac
{
    using Vk = Silk.NET.Vulkan.Vk;
    public class VulkanLoader
    {
        public static Vk GetApi()
        {
            var context = CreateIOSContext();
            Vk ret = new Vk(context);

            return ret;
        }

        static INativeContext CreateIOSContext(string text = "__Internal")
        {
            var _foldSilkNETVulkanVkPInvokeOverride0 = AppContext.TryGetSwitch("SILK_NET_VULKAN_VK_ENABLE_PINVOKE_OVERRIDE_0", out var isEnabled) && isEnabled;
            if (_foldSilkNETVulkanVkPInvokeOverride0 && text == "__Internal")
            {
                return null;
            }
            if (IOSNativeContext.TryCreate(text, out var context))
            {
                return context;
            }

            throw new FileNotFoundException("Could not load from any of the possible library names! Please make sure that the library is installed and in the right place!");
        }

        class IOSNativeContext : INativeContext, IDisposable
        {
            private readonly IntPtr _libraryHandle;

            public IntPtr NativeHandle => _libraryHandle;

            public static bool TryCreate(string name, out IOSNativeContext context)
            {
                var library = ObjCRuntime.Dlfcn.dlopen(null, 0x002);//copy from xamarin-macios

                context = new IOSNativeContext(library);
                return true;
            }

            IOSNativeContext(IntPtr library)
            {
                _libraryHandle = library;
            }

            public nint GetProcAddress(string proc, int? slot = null)
            {
                return ObjCRuntime.Dlfcn.dlsym(NativeHandle, proc);
            }

            public bool TryGetProcAddress(string proc, out nint addr, int? slot = null)
            {
                addr = ObjCRuntime.Dlfcn.dlsym(NativeHandle, proc);
                return true;
            }

            public void Dispose()
            {
                ObjCRuntime.Dlfcn.dlclose(NativeHandle);
            }
        }
    }
}
