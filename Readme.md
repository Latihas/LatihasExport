# Latihas Export

## 简介

FF14的钓鱼笔记(支持鱼糕导出)、成就、制作笔记等导出工具

## 赛博精神洁癖注意!!!

本项目使用鲇鱼精(PostNamazu)进行内存操作。其中大部分操作仅有读取内存，导出制作笔记时使用写内存调用游戏内函数。

## 使用方法

将Definitions文件夹放在ACT根目录(以下简称\$(ActRoot))。LatihasExport.dll可随意放置，推荐路径$(ActRoot)
\Plugins\LatihasExport\LatihasExport.dll。

## 二次开发方法

最好是能联系开发者群(556836295)，有时候懒b了但是游戏更新了导致失效可以更新Definitions文件夹内信息(
比如即将到来的7.1需要改Recipe.json等)，要大改请修改.csproj中的ActRoot等信息。

## 参考文献

- https://github.com/Ariiisu/ExportFishLog
- https://github.com/xivapi/SaintCoinach (最后一个.Net4版本)
- GreyMagic.dll是从 https://github.com/Natsukage/PostNamazu 拾取
- Definitions文件夹从 https://github.com/aers/FFXIVClientStructs 拾取