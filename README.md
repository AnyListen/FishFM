# FishFM
A Cross Platform Music Discovery FM

> 『鱼声FM』遇见属于你的声音

《鱼声FM》是一款以发现音乐为核心的跨平台音乐软件，音乐类型主要偏纯音乐、后摇滚、电子乐、国风音乐等，如果你喜欢主要面向喜欢「鱼声音乐精选」公众号推荐的音乐，那么这个软件会让你发现更多宝藏音乐。


目前仅支持PC端，支持Win、Linux、Mac，希望体验预览版的可以文末扫码入群。

下载链接: https://pan.baidu.com/s/18lu3-ltuZV0N1vzukbD5yg 提取码: updc

![Demo](https://img.ifish.fun/WX20220412-212126%402x.png)

## 自助编译
### macos
运行以下命令，然后在`项目目录/bin/Release/publish`下可以找到 `FishFM.app`
```shell
~/.dotnet/dotnet restore -r osx-x64
~/.dotnet/dotnet msbuild -t:BundleApp -p:RuntimeIdentifier=osx-x64 -property:Configuration=Release -p:UseAppHost=true
```

### win-x86
运行以下命令，然后在`项目目录/bin/Release/publish`下可以找到 `FishFM.app`
```shell
~/.dotnet/dotnet restore -r win-x86
~/.dotnet/dotnet publish -r win-x86 -c Release --self-contained true -property:Configuration=Release -p:UseAppHost=true -p:DebugSymbols=false 
```

