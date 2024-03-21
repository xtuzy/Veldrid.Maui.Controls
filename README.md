# Veldrid.Maui.Controls
[![NuGet version(Yang.MAUICollectionView)](https://img.shields.io/nuget/v/Veldrid.Maui.Controls?label=Veldrid.Maui.Controls)](https://www.nuget.org/packages/Veldrid.Maui.Controls)

It is extracted from [Veldrid.Samples](https://github.com/xtuzy/Veldrid.Samples)，this project focuses on optimizing controls
## How to use
### Install Veldrid.Maui.Controls
### Install Veldrid Packages
current official veldrid packages have some bugs on maui, for temparery fix they, i push some packages to nuget.org, you can install they to your app project to override offical dll.
- Base on [official/veldrid](https://github.com/veldrid/veldrid)
    - [Yang.Veldrid.Maui.Android](https://www.nuget.org/packages/Yang.Veldrid.Maui.Android/) change see: [build_for_maui_android](https://github.com/xtuzy/veldrid.maui/tree/build_for_maui_android)
    - [Yang.Veldrid.Maui.Windows](https://www.nuget.org/packages/Yang.Veldrid.Maui.Windows/) change see: [build_for_maui_windows](https://github.com/xtuzy/veldrid.maui/tree/build_for_maui_windows)
- Base on [ppy/veldrid](https://github.com/ppy/veldrid) change see: [build_for_maui_ios](https://github.com/xtuzy/veldrid.maui/tree/build_for_maui_ios)
    - [Yang.Veldrid.Maui.iOS](https://www.nuget.org/packages/Yang.Veldrid.Maui.iOS/), (also work on android and windows)

In order to replace official veldrid, you need to add these to .csproj (ExcludeAssets will delete officail dll):
```
<ItemGroup>
    <PackageReference Include="Veldrid.SPIRV" Version="1.0.15" ExcludeAssets="All"/>
    <PackageReference Include="Veldrid.Maui.SPIRV" Version="1.0.16.1" />

    <PackageReference Include="Veldrid" Version="4.9.0" ExcludeAssets="All"/>
    <PackageReference Include="Yang.Veldrid.Maui.iOS" Version="4.9.10-ga1b22d70a3" />
</ItemGroup>
```
## Create view
- Add `.UseVeldridView()` to your app.
- create view

  ```
   var veldridView = new VeldridView();
   veldridView.Backend = GraphicsBackend.OpenGLES;
   veldridView.AutoReDraw = true;// will loop render
   layout.Add(veldridView);
  ```
- create drawable

 see sample [HelloTriangle](https://github.com/xtuzy/Veldrid.Maui.Controls/blob/main/Veldrid.Maui.Controls.Samples.Core/LearnOpenGL/HelloTriangle.cs), you need learn how to use veldrid, see https://veldrid.dev/
 - set drawable to veldridView

## Advice GraphicsBackend
- iOS - Metal (don't test)
- Windows - D3D11
- Android - OpenGLES

** Current veldrid don't have a nice memory manager for Vulkan backend, will waste many memeory, i don't advice you try it.
