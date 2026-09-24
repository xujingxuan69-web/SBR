# Unity客户端开发

徐靖轩｜男｜21岁｜icon:phone 173-5795-5209｜ icon:email 1609413762@qq.com

icon:github  github.com/xujingxuan69-web

## 教育背景

**浙江中医药大学 - 本科 - 计算机科学与技术（2023.09-2027.07）**

**学业成绩前10%，获校级二等奖学金、三等奖学金、学习优秀奖；大学生服务外包竞赛三等奖（2024），CET-6**

## 实习经历

### **南京红薯网络科技有限公司 - 小游戏技术开发 -（2026.08 - 至今）**

- 参与 Unity、Cocos 小游戏客户端工程维护与渠道适配，负责渠道 SDK 接入、平台参数配置、广告逻辑替换、构建打包和运行时问题修复。
- 处理渠道初始化、排行榜异常、退出窗口无法调起、包体限制、本地化遗漏、关卡删除后存档异常等问题；截至目前完成 100+ 次 SDK 替换出包，累计修复 20+ 个项目问题。
- 深度使用 Codex 辅助项目级代码检索、调用链分析、差异对比、构建日志定位和自动化脚本开发；结合 Git diff、模拟器预览和真机测试验证修改结果，提升多项目并行交付效率。

## 项目经验

### **Steel Ball Run - 3D 竞速骑乘 Demo -（2026.07 - 至今）**

**技术栈**：Unity、C#、CharacterController、New Input System、Animator、Cinemachine、Blender、UniTask、asmdef

**项目简介**：独立开发 3D 骑乘竞速 Demo，重点实现马匹控制、上下坡移动、动画表现、输入优化和能力系统原型。

- 基于 **CharacterController** 实现马匹前进、后退、转向、跳跃与碰撞处理，并根据速度比例动态影响转向速度和跳跃力度，优化骑乘手感。
- 通过 **OnControllerColliderHit** 获取碰撞法线，计算坡度角、下滑方向与重力分量；结合坡度缓冲和接地吸附优化上下坡浮空、误判滑落等问题。
- 封装泛型状态机 **EntityStateMachine&lt;T&gt;** 管理 Grounded、Jump、Air 等状态，并与 Animator 参数联动，降低移动、动画和输入逻辑耦合。
- 接入 New Input System 封装输入读取层，实现跳跃预输入和土狼时间；实现马头转向、障碍物避让和尾巴骨骼随速度动态摆动。
- 使用事件、**OverlapSphereNonAlloc**、缓存数组和 UniTask 控制异步过渡并减少分配；使用 **asmdef** 独立 GAS 模块程序集，正在实现轻量级 Attribute / Ability / Effect 原型。

### **Dream & Life - 2D Roguelite 独立 Demo -（2026.03 - 2026.06）**

**技术栈**：Unity、C#、UGUI、2D Physics、Animator、ScriptableObject、状态机、对象池、协程

**项目简介**：独立开发 2D Roguelite Demo，完成角色控制、战斗技能、装备词条、背包仓库、Buff/DOT 与 UI 刷新等核心系统。**演示视频**：[www.bilibili.com/video/BV1nXTo6UEcM](http://www.bilibili.com/video/BV1nXTo6UEcM/)

- 封装玩家状态机，管理 Idle、Move、Jump、Air、Dash、WallSlide、Attack、CounterAttack、Squat、Tears 等 10+ 状态，拆分移动、战斗、形态和动画逻辑。
- 设计角色数值系统，使用 Stat + Modifier 管理基础属性、装备加成、临时 Buff、DOT 伤害和暴击计算，并通过事件通知 UI 更新。
- 基于 **ScriptableObject** 配置物品、技能和装备效果；装备穿脱时动态修改角色属性，支持不同品质、装备类型和随机词条扩展。
- 实现背包、仓库、装备栏、消耗品栏等 UI 交互，使用 Dictionary 维护物品数据与槽位映射；通过对象池复用子弹、特效等高频对象，减少实例化开销。

### **药韵八味 - 2D 卡牌经营小游戏 -（2025.07 - 2025.08）**

**技术栈**：Unity、C#、UGUI、ScriptableObject、AI 接口、Android 适配

- 双人协作开发传统文化题材卡牌经营小游戏，负责 UI 搭建、资源适配、卡牌合成和部分系统逻辑实现。
- 使用 **ScriptableObject** 配置卡牌数据、材料消耗和合成配方，实现数据与逻辑分离；对接大模型 API 实现智能 NPC 对话，并适配 PC 与 Android 双端操作。

## 专业技能/其他

- **Unity / C#**：熟悉 Unity 客户端开发流程，掌握角色控制、物理碰撞、动画状态机、UGUI、ScriptableObject、协程、事件与委托。
- **工程与性能**：了解 Unity 基础资源管理与模块划分，能使用对象池、缓存数组、NonAlloc API、事件解绑等方式减少运行时开销。
- **基础能力**：掌握 List、Dictionary、HashSet、栈、队列、链表等常用数据结构，了解排序、二分查找、双指针、DFS/BFS 等基础算法。
