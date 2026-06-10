# 打包与发布说明

## 当前方案
本项目基于 `.NET 6.0` 和 WPF 开发，目标是生成一个 Windows 桌面可执行程序。

## 构建步骤
1. 安装 .NET 6 SDK
2. 在项目根目录执行：
   - `dotnet build src/ColorPickerTray/ColorPickerTray.csproj`
3. 运行程序：
   - `dotnet run --project src/ColorPickerTray/ColorPickerTray.csproj`

## 发布建议
- 可以使用 `dotnet publish` 生成独立可执行文件：
  `dotnet publish src/ColorPickerTray/ColorPickerTray.csproj -c Release -r win-x64 --self-contained true`
- 也可以生成框架依赖发布：
  `dotnet publish src/ColorPickerTray/ColorPickerTray.csproj -c Release`

## 目录输出
发布后的文件可在以下位置找到：
- `src/ColorPickerTray/bin/Release/net8.0-windows/win-x64/publish/`

## 后续打包方案
- 若需要安装程序，可进一步使用 WiX、Inno Setup 或微软 MSIX 进行安装包制作。
- 也可基于发布目录打包 ZIP 并附带使用说明。
