# CLAUDE Guidance for Windows Tray Screen Color Picker

## 项目概述
这是一个面向 Windows 的轻量级屏幕拾色器，运行在系统托盘中，支持热键启动、实时拾色、HSV/RGB/HEX 显示、历史记录、CSV 导出、主题切换等功能。

## 目录说明
- `docs/`：项目开发相关规范和标准文件
- `dev-daily/`：每日开发日志，记录完成事项、待办、风险与计划
- `CLAUDE.md`：本文件，说明各文档路径和协作指引
 - `src/`：应用源代码和项目文件

## 主要文档指南
- `docs/requirements.md`：功能需求与产品需求
- `docs/architecture.md`：技术选型、模块划分、系统架构
- `docs/design-guidelines.md`：UI 设计规范、交互风格、色彩方案
- `docs/development-process.md`：开发流程、迭代计划、验收标准

## 开发流程建议
1. 首先阅读 `docs/requirements.md`，确认功能范围和优先级
2. 再阅读 `docs/architecture.md`，确定技术方案和实现路线
3. 在 `src/ColorPickerTray/` 中开发和迭代核心程序
3. 按照 `docs/development-process.md` 逐步推进开发，将功能拆成小阶段
4. 每天在 `dev-daily/` 中记录当日完成事项与待办

## 日志规范
- 新建日志文件：`dev-daily/YYYY-MM-DD.md`
- 推荐内容结构：
  - 完成事项
  - 今日待办
  - 风险/问题
  - 明日计划

## 目标
保持项目稳步推进、每次迭代安全可控、开发结果可验证。
