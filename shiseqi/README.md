# 屏幕拾色器 (ColorPickerTray)

Windows 托盘屏幕拾色器 — 解压即用，无需安装 .NET，任何 Windows 电脑都能运行。

## 📥 下载

**[⬇ 下载最新版（免安装 ZIP）](https://github.com/akievelai-afk/ColorPickerTray/releases/latest)**

1. 下载 `屏幕拾色器-免安装版.zip`
2. 解压到任意位置
3. 双击 `ColorPickerTray.exe` 运行
4. 右下角出现红黄蓝托盘图标

## 🎯 功能

- **Ctrl+F1** 全局热键取色，鼠标所在位置实时拾取
- **色轮 + HSV 色板** 可视化显示当前颜色
- **HSV / RGB / HEX** 多格式颜色值，可选复制格式
- **Ctrl+H** 打开历史记录窗口，支持 CSV 导出
- **Ctrl+Z** 撤回最近一次取色记录
- **Alt 键** 锁定/解锁拾色窗口位置
- **迷你色块面板**（2×5）快速查看最近 10 个颜色
- **多主题**：淡蓝 / 深色 / 浅色 / 海洋蓝 / 森林绿 / 珊瑚橙 / 薰衣草紫 / 石墨灰
- **DPI 感知**，高分屏也不偏移

## ⌨️ 快捷键

| 快捷键 | 功能 |
|--------|------|
| Ctrl+F1 | 启动拾色模式 |
| Ctrl+H | 打开历史记录 |
| 左键点击 / C 键 | 复制当前颜色值 |
| 右键点击 / Esc | 退出拾色模式 |
| Alt | 锁定/解锁拾色窗口 |
| Ctrl+Z | 撤回最近记录 |

## 🛠 构建（开发者）

```bash
# 需要 .NET 6 SDK
dotnet build src/ColorPickerTray/ColorPickerTray.csproj

# 发布便携版
dotnet publish src/ColorPickerTray/ColorPickerTray.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false
```

## 📁 项目结构

- `src/ColorPickerTray/` — C# WPF 应用源代码
- `docs/` — 需求、架构、设计文档
- `dev-daily/` — 开发日志

## 📄 许可证

MIT License
