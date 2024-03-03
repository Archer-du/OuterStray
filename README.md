# OuterStray
---
## OuterStray新手小教程
### 0. 开局准备
1. 点击中间的建筑，进入构建初始卡组的界面，目前的测试版本将全卡库开放给测试玩家，
每张卡牌限选三张进入自己的初始卡组，目前卡组卡牌张数没有限制
2. 点击右侧的建筑，进入选择基地车的界面，目前只有初始基地车，随便选一个即可，点击
下方确认按钮（目前无文本，为白框），即进入第一关
### 1. 关卡
每一个关卡由一些相连的节点组成，进入节点可触发不同的事件：
1. 补给节点：四选一加入自己卡组
2. 气矿节点：获得一定金币
3. 医疗节点：在左侧将卡组中受伤的单位卡拖入右侧的框中，点击恢复耐久为其恢复生命值
4. 商店节点：消耗金币买卡加入自己卡组
5. 战斗节点：每层的最后一个节点，战斗胜利后选择奖励并进入下一层
### 2. 卡牌
#### 2.1. 单位卡
##### 2.1.1. 兵种
每张卡牌隶属于某个兵种，通过卡框颜色。每个兵种有不同的特性:
1. 轻装（绿色）：无
2. 轰击（橙色）：每次攻击时随机选取一个敌方单位进行攻击
3. 重装（灰色）：嘲讽相邻的敌方单位，即自身若处在敌方单位攻击范围，敌方单位会优先
攻击自身
4. 机动（蓝色）：移动后攻击计数器-1
5. 建筑（紫色）：无法移动
##### 2.1.2. 单位卡属性
卡面左上方数字代表费用；卡面上下方三个数字，从左到右依次代表：攻击力/攻击计数器/
生命值，卡牌的明暗代表操作计数器是否为0
费用即部署该卡需耗费的能源（能源用左下角圆框中的数字表示）
攻击计数器每回合-1，归零即进行一次攻击
生命值归零则从卡组中去除
##### 2.1.3. 单位卡效果
部分封装成字段的效果如下：
1. 顺劈：攻击对象（范围）为相邻所有敌人，具体可看战斗时卡牌上的箭头指示
2. 越野：可移动至不相邻的可部署战线上（PS：若0战线为蓝，1、2战线为红，也可直接移
动至2战线）
3. 护甲：被攻击时受到的实际伤害=敌方攻击-护甲
4. 格挡：免疫下一次伤害
5. 突击：部署该单位卡不消耗本回合操作计数器
#### 2.2. 指令卡
指令卡目前可以通过卡图是否有背景以及卡框来区别于单位卡（指令卡卡图有背景）
#####2.2.1. 指令卡属性
卡面上下方的数字为指令卡耐久，每使用一次耐久-1，耐久归零则从卡组中去除
### 3. 战斗
#### 3.1. 玩家操作
##### 3.1.1. 部署
将单位卡拖动至支援战线（即最下方的战线）即可实现部署，部署后该卡操作计数器归零（表
现为卡面加蒙版）
##### 3.1.2. 移动
拖动可操作卡牌（操作计数器不为零，表现为卡面未加蒙版）至可操作战线（蓝色框的战线以
及与蓝色框相邻的红色框战线）
##### 3.1.3. 释放指令
将指令卡拖至战场右侧，即进入释放状态，无目标指令直接生效，有目标指令需选目标（请仔
细阅读卡牌效果确认是否需选择目标），选目标通过直接点击卡牌实现
#### 3.2. 战场信息
##### 3.2.1. 前线
己方占据的最前方战线即为前线（用透明矩形框来表示）
##### 3.2.2. 能源
左下角圆框中的数字即为能源，其下侧的+x为下回合将获得的能源

## 演示
![cat](https://github.com/Archer-du/OuterStray/blob/master/Assets/demo/main_menu.gif)
![cat](https://github.com/Archer-du/OuterStray/blob/master/Assets/demo/base_select.gif)
![cat](https://github.com/Archer-du/OuterStray/blob/master/Assets/demo/battle_start.gif)
![cat](https://github.com/Archer-du/OuterStray/blob/master/Assets/demo/battle_turnstart.gif)
![cat](https://github.com/Archer-du/OuterStray/blob/master/Assets/demo/battle_deploy.gif)
![cat](https://github.com/Archer-du/OuterStray/blob/master/Assets/demo/battle_move.gif)
![cat](https://github.com/Archer-du/OuterStray/blob/master/Assets/demo/battle_inspect.gif)
![cat](https://github.com/Archer-du/OuterStray/blob/master/Assets/demo/battle_attacked.gif)
![cat](https://github.com/Archer-du/OuterStray/blob/master/Assets/demo/battle_win.gif)
