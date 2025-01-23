# Latihas Export

## 简介

FF14的钓鱼笔记(支持鱼糕导出)、成就、制作笔记(做赐福工具成就)等导出工具。ACT插件。

## 赛博精神洁癖注意!!!

本项目使用鲇鱼精(PostNamazu)进行内存操作，目前暂时仅有读取内存。

## 使用方法

### 配置

解压压缩包，文件结构不动，放入推荐路径`$(ActRoot)/Plugins/LatihasExport/`中，看起来像这样：

```
$(ActRoot)/
│   ...
└── Plugins/
	│   ...
	└── LatihasExport/
		├── Generated/
		│   └── xxx.json
		├── Definitions/
		│   └── xxx.json
		├── libs/
		│   ├── EntityFramework.dll
		│   └── LatihasExport.Core.dll
		└── LatihasExport.dll
```

会在`LatihasExport.dll`相同目录下生成out输出文件夹。

### 依赖

已验证的环境：`呆萌ACT`，`鲇鱼精1.3.5.3`

## 导出文件说明

### 钓鱼

`fish_all.csv`: 所有鱼信息

`fish_done.json`: 可以导入到鱼糕的信息

`fish_rest.csv`: 没抓到的鱼的信息

### 制作笔记

`recipe_rest.csv`: 剩余未完成配方信息

`recipe_rest_material.csv`: 完成所有剩余未完成配方所需的材料计算总和

### 成就

有的成就在游戏里似乎找不到

`achievement_all.csv`: 所有成就信息

`achievement_rest.csv`: 未完成成就信息

## 二次开发方法

不会就提交Issue。
有时候懒b了但是游戏更新了导致失效可以更新Definitions(SaintCoinach)文件夹内信息(比如即将到来的7.1需要改Recipe.json等)。
部分内存数据可能会在注释里含有新版信息，FFXIVClientStructs.dll引用的是XIVLauncherCN自带的，如有国际服需求等可以修改LatihasExport.Generator的Main中的路径。
要大改请修改.csproj中的ActRoot等信息。SaintCoinach设置的是国服。

编译流程：

先运行LatihasExport.Generator项目，然后多点几次项目构建就行了。

## 一些笔记

### 修改SaintCoinach

SaintCoinach拉下来仅保留该项目与DotSquish。删除SaintCoinach.csproj及目录下非.cs文件、DotSquish.csproj、Definitions文件夹、Libra文件夹下非.cs文件

添加

```
	<PropertyGroup>
		...
		<AllowUnsafeBlocks>true</AllowUnsafeBlocks>
	</PropertyGroup>
	<ItemGroup>
		...
		<Compile Include="SaintCoinach\**"/>
		<Compile Include="DotSquish\**"/>
	</ItemGroup>
```

修改`ARealmReversed.cs`中的`ReadDefinition`，将`Definitions`换成`$"{Main.ROOTDIR}/Definitions"`

## 参考文献

- 银山雀儿(Silver Dasher)，具体开源地址没找到
- https://github.com/aers/FFXIVClientStructs
- XIVLauncherCN
- https://github.com/Ariiisu/ExportFishLog
- Definitions文件夹从 https://github.com/xivapi/SaintCoinach (241125)拾取，该文件夹与SaintCoinach版本无关，国服不能用找最新版之前的几个Commit即可
- GreyMagic.dll是从 https://github.com/Natsukage/PostNamazu 拾取
