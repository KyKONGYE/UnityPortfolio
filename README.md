# Unity Portfolio · 孔业

个人 Unity 开发作品集，包含 3 个独立完成的可运行 Demo。

## 项目

| 目录 | 项目 | 类型 | 技术要点 |
|------|------|------|----------|
| `Portal_Demo/` | 传送门玩法复刻 | 第一人称空间解谜 | 射线检测放置传送门、坐标变换传送 |
| `TowerDefense/` | 3D 塔防 | 塔防 | NavMesh 寻路、Animator 状态机、UGUI、JSON 存档 |
| `PlaneShooter/` | 2D 飞机大战 | 弹幕射击 | 弹幕发射系统、NGUI、XML 存档、排行榜 |

## 环境

- Unity **2022.3.62f3c1**（2022.3 LTS）
- 语言：C#

## 运行方式

1. 用 Unity Hub「打开项目」，选择对应文件夹（如 `TowerDefense/`）
2. 首次打开 Unity 会自动重新生成 `Library` 等本地缓存（已通过 `.gitignore` 排除）
3. 打开 `Assets/Scenes` 下的场景即可运行

## 说明

- 三个项目均为独立 Unity 工程，可分别打开
- 每个项目根目录均有 `.gitignore`，仅同步 `Assets / Packages / ProjectSettings` 等核心文件，仅代码(无美术资源)
- 部分项目使用了第三方插件（NGUI、Spine 等），仅供学习演示
