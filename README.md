# ONI-MODS

 上游仓库：[@Glampi42 EN](https://github.com/Glampi42/ONI-Mods)

此仓库仅汉化翻译

## Highlight Overlay|高亮显示!CN

<div align=center>
    <img src="workshop\HighlightOverlay\zh_image\0.png" width=400 height=400>
</div>

创意工坊地址：[点击跳转](https://steamcommunity.com/sharedfiles/filedetails/?id=3170592482)

一个 QoL mod，用于可视化以某种方式与所选对象相关的事物。 

### 如何使用

1. 选择一个对象（地图上的任何对象）
2. 打开高亮显示
3. 您可以关闭所选对象的详细信息窗口，使其不与高亮显示菜单重叠。
4. 在高亮显示菜单中配置应该突出显示的内容

如上所述，该模式可让您找到与所选对象相关的事物并将其可视化。关系类型包括（但不限于）： 

- 该对象的同类物品（在地图上出现的所有同类物品）
- 该对象的消费者（如果可以消费的话）
- 该对象的生产者
- 该对象的消耗材料（如果该对象可以消耗物品）
- 此对象的产品

您还可以应用筛选器，只突出显示某些特定种类的事物。例如，如果您想高亮显示铺在地板上的所有煤炭，您可以选择煤炭（碎屑或天然瓷砖）并高亮显示其同类物品。然后，您就可以配置 "高亮过滤器"，使其只高亮显示铺在地板上的物品，这样就可以了！

为了防止游戏卡顿，该修改器只扫描一次整个地图，然后高亮显示所有相应的物品。这就意味着，如果气体在房间里移动、蒸汽凝结成水、新建筑建成等，高亮显示都不会更新。这就是为什么默认情况下叠加功能只能在游戏暂停时使用--这样地图上的任何东西都不会改变。您可以在 MOD 的配置中禁用此功能，但要注意的是，随着模拟运行时间的推移，此 MOD 显示的信息将变得不正确。

-------
### 提示

以下是一些更具体的信息，供真正想从本程序中获得所有功能的人参考。

所有以 "考虑" 开头的高亮选项（考虑总体状态等）也适用于所选对象中没有考虑选项的类型。例如，如果您选择了任何元素并勾选了 "考虑具体情况 "选项，然后又选择了一个建筑（例如玻璃锻造厂）并高亮显示其生产的元素，那么只有当这些元素的具体情况与该建筑出口的元素相同时，它所生产的元素才会被高亮显示（在玻璃锻造厂的情况下--只有液态玻璃才会被高亮显示）。

如果关闭“保持高亮显示”选项，会保留高亮显示对象。但是，如果您取消游戏暂停，它仍会删除所有信息（除非您在 mod 配置中启用了游戏未暂停时的叠加使用）。

您可以选择位于仓库/建筑中的对象，并高亮显示与之相关的内容。为此，请选择储藏箱（或其他任何建筑），然后向下滚动其详细信息菜单，直到看到 "内容 "选项卡。在那里，您可以点击任何条目并选择存储的物品！

如果您想突出显示某些特定的物品，可以使用储藏箱来实现。选择一个空的储存箱，然后将其储存过滤器设置为你想突出显示的任何物品。然后只需突出显示其 "消耗品 "即可。

-------
### 特别感谢

... 感谢 Peter Han 的 PLib，它使覆盖菜单的创建变得更容易！

......感谢 ONI discord 社区的所有开发者回答了我的所有问题！

源代码位于我的 GitHub[https://github.com/Glampi42/ONI-Mods/tree/main/HighlightOverlay]。

您可以在评论中或 GitHub 上创建问题，留下您的反馈、请求或发现的错误。


### 如何使用-直观说明
<details>
  <summary>轮播图片</summary>
  <img src="workshop\HighlightOverlay\zh_image\1.png" width=1000 height=400>
  <img src="workshop\HighlightOverlay\zh_image\2..png" width=1000 height=400>
  <img src="workshop\HighlightOverlay\zh_image\2.5.png" width=1000 height=400>
  <img src="workshop\HighlightOverlay\zh_image\3.png" width=1000 height=400>
  <img src="workshop\HighlightOverlay\zh_image\31.jpg" width=1000 height=400>
  <img src="workshop\HighlightOverlay\zh_image\32.jpg" width=1000 height=400>
  <img src="workshop\HighlightOverlay\zh_image\33.jpg" width=1000 height=400>
  <img src="workshop\HighlightOverlay\zh_image\34.jpg" width=1000 height=400>
</details>