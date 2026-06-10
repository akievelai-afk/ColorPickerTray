# ColorPickerTray

Windows 轻量级屏幕拾色器项目。

## 项目结构
- `src/ColorPickerTray/`：C# WPF 应用源代码
- `docs/`：功能需求、架构、设计规范、开发流程
- `dev-daily/`：每日开发日志
- `CLAUDE.md`：项目指引与文档说明

## 当前进度
- 已搭建 WPF 项目骨架
- 实现系统托盘应用启动逻辑
- 实现托盘右键菜单和退出处理
- 实现全局 `F1` 热键注册与触发框架
- 实现拾色覆盖窗口和屏幕当前像素颜色采样
- 支持按 `C` 复制颜色值到剪贴板
- 实现颜色历史管理与 CSV 导出功能
- 实现设置窗口，支持主题、热键和显示格式配置

## 下阶段计划
- 优化拾色窗口主题与样式
- 改进热键自定义输入与验证
- 增加历史面板颜色预览缩略图
- 准备打包与运行测试流程

## 构建与运行
1. 安装 .NET 6 SDK
2. 在项目根目录运行：
   - `dotnet build src\\ColorPickerTray\\ColorPickerTray.csproj`
   - `dotnet run --project src\\ColorPickerTray\\ColorPickerTray.csproj`

## 备注
当前项目使用 `.NET 6.0` 和 WPF，已启用 Windows Forms 以支持系统托盘通知图标。