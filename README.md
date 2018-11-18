# FightingLandlords
使用ILruntime做热更新

使用ugui，剔除EventSystem，使用eventcallback，支持多点触控，委托式事件，使用事件注册模式，无需挂脚本

一套完整的ui框架，包含线程池，对象池 tcp udp 封包，自动断线重连，可以重定向

数据传输支持指针型，快速无需反射 databuffer

ui和assetbundle分离

ui和数据分离，数据驱动模式

增加emojitext，电脑端可以直接输入emoji

scorll使用数据绑定，循环回收，固定滚动，循环滚动，和回弹滚动

多线程gif解析

泛型页面管理器，方便页面间的切换

本框架不适合挂脚本，挂脚本需要在modelmanager中注册脚本类型，否则无法生效

热更新工程https://github.com/huqiang0204/HotFixGame

热更新类无法在unity中使用反射，但是可以间接反射

业余时间进行更新
