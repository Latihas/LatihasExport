# Latihas Export

## 简介

FF14的钓鱼笔记(支持鱼糕导出)、成就、制作笔记(做赐福)等导出工具。ACT插件。

## 赛博精神洁癖注意!!!

本项目使用鲇鱼精(PostNamazu)进行内存操作。其中大部分操作仅有读取内存，导出制作笔记时使用写内存调用游戏内函数。

## 使用方法

### 配置

将Definitions文件夹放在ACT根目录(以下简称`$(ActRoot)`)。`LatihasExport.dll`可随意放置，推荐路径`$(ActRoot)\Plugins\LatihasExport\LatihasExport.dll`。会在`LatihasExport.dll`相同目录下生成out输出文件夹。

### 依赖

已验证的环境：`呆萌ACT`，`鲇鱼精1.3.5.3`

## 导出文件说明

### 钓鱼

`{time}_fish_all.csv`: 所有鱼信息
`{time}_fish_done.json`: 可以导入到鱼糕的信息
`{time}_fish_rest.csv`: 没抓到的鱼的信息

### 制作笔记

`{time}_recipe_rest.csv`: 剩余未完成配方信息
`{time}_recipe_rest_material.csv`: 完成所有剩余未完成配方所需的材料计算总和

### 成就

有的成就在游戏里似乎找不到
`{time}_achievement_all.csv`: 所有成就信息
`{time}_achievement_rest.csv`: 未完成成就信息

## 二次开发方法

不会就提交Issue，有时候懒b了但是游戏更新了导致失效可以更新Definitions文件夹内信息(比如即将到来的7.1需要改Recipe.json等)，要大改请修改.csproj中的ActRoot等信息。SaintCoinach设置的是国服。

## 参考文献

- https://github.com/aers/FFXIVClientStructs
- https://github.com/Ariiisu/ExportFishLog
- Definitions文件夹从 https://github.com/xivapi/SaintCoinach (最后一个.Net4版本)拾取，该文件夹与.Net版本无关，国服不能用找最新版之前的几个Commit即可
- GreyMagic.dll是从 https://github.com/Natsukage/PostNamazu 拾取
