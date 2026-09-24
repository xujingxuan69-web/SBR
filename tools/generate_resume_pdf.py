from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.units import mm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import Paragraph, SimpleDocTemplate, Spacer


ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "output" / "pdf"
OUT_DIR.mkdir(parents=True, exist_ok=True)
OUT_FILE = OUT_DIR / "秋招简历_优化版.pdf"

FONT = r"C:\Windows\Fonts\msyh.ttc"
FONT_BOLD = r"C:\Windows\Fonts\msyhbd.ttc"
pdfmetrics.registerFont(TTFont("MSYH", FONT))
pdfmetrics.registerFont(TTFont("MSYH-Bold", FONT_BOLD))


def esc(text: str) -> str:
    return (
        text.replace("&", "&amp;")
        .replace("<", "&lt;")
        .replace(">", "&gt;")
    )


def p(text, style):
    return Paragraph(text, style)


styles = {
    "title": ParagraphStyle(
        "title",
        fontName="MSYH-Bold",
        fontSize=17.2,
        leading=19,
        alignment=1,
        spaceAfter=2.5,
    ),
    "contact": ParagraphStyle(
        "contact",
        fontName="MSYH",
        fontSize=9.4,
        leading=11.4,
        alignment=1,
        textColor=colors.HexColor("#222222"),
        spaceAfter=3,
    ),
    "section": ParagraphStyle(
        "section",
        fontName="MSYH-Bold",
        fontSize=11.8,
        leading=13.6,
        textColor=colors.HexColor("#111111"),
        borderWidth=0,
        borderPadding=0,
        spaceBefore=4.2,
        spaceAfter=2.4,
    ),
    "entry": ParagraphStyle(
        "entry",
        fontName="MSYH-Bold",
        fontSize=10.1,
        leading=12.1,
        textColor=colors.HexColor("#111111"),
        spaceBefore=2.2,
        spaceAfter=1.5,
    ),
    "meta": ParagraphStyle(
        "meta",
        fontName="MSYH",
        fontSize=8.45,
        leading=10.25,
        textColor=colors.HexColor("#333333"),
        spaceAfter=1.2,
    ),
    "body": ParagraphStyle(
        "body",
        fontName="MSYH",
        fontSize=8.45,
        leading=10.25,
        textColor=colors.HexColor("#222222"),
        firstLineIndent=0,
        leftIndent=0,
        spaceAfter=1.1,
    ),
    "bullet": ParagraphStyle(
        "bullet",
        fontName="MSYH",
        fontSize=8.15,
        leading=10.1,
        textColor=colors.HexColor("#222222"),
        leftIndent=9,
        firstLineIndent=-6,
        bulletIndent=0,
        spaceAfter=1.15,
    ),
    "edu": ParagraphStyle(
        "edu",
        fontName="MSYH-Bold",
        fontSize=9.4,
        leading=11.3,
        textColor=colors.HexColor("#111111"),
        spaceAfter=1,
    ),
}


story = []
story.append(p("Unity客户端开发", styles["title"]))
story.append(p("徐靖轩｜男｜21岁｜173-5795-5209｜1609413762@qq.com｜github.com/xujingxuan69-web", styles["contact"]))

story.append(p("教育背景", styles["section"]))
story.append(p("浙江中医药大学 - 本科 - 计算机科学与技术（2023.09 - 2027.07）", styles["edu"]))
story.append(p("学业成绩前 10%，获校级二等奖学金、三等奖学金、学习优秀奖；大学生服务外包竞赛三等奖（2024），CET-6", styles["body"]))

story.append(p("实习经历", styles["section"]))
story.append(p("南京红薯网络科技有限公司 - 小游戏技术开发（2026.08 - 至今）", styles["entry"]))
intern_bullets = [
    "参与 Unity、Cocos 小游戏客户端工程维护与渠道适配，负责渠道 SDK 接入、平台参数配置、广告逻辑替换、构建打包和运行时问题修复。",
    "处理渠道初始化、排行榜异常、退出窗口无法调起、包体限制、本地化遗漏、关卡删除后存档异常等问题；完成 100+ 次 SDK 替换出包，累计修复 20+ 个项目问题。",
    "深度使用 Codex 辅助项目级代码检索、调用链分析、差异对比、构建日志定位和自动化脚本开发；结合代码差异、模拟器预览和真机测试验证结果，提升多项目并行交付效率。",
]
for item in intern_bullets:
    story.append(p("• " + esc(item), styles["bullet"]))

story.append(p("项目经验", styles["section"]))

story.append(p("Steel Ball Run - 3D 竞速骑乘 Demo（2026.07 - 至今）", styles["entry"]))
story.append(p("<b>技术栈</b>：Unity、C#、CharacterController、New Input System、Animator、Cinemachine、Blender、UniTask、asmdef", styles["meta"]))
story.append(p("<b>项目简介</b>：独立开发 3D 骑乘竞速 Demo，重点实现马匹控制、上下坡移动、动画表现、输入优化和能力系统原型。", styles["meta"]))
sbr_bullets = [
    "基于 CharacterController 实现马匹前进、后退、转向、跳跃与碰撞处理，并根据速度比例动态影响转向速度和跳跃力度，优化骑乘手感。",
    "通过 OnControllerColliderHit 获取碰撞法线，计算坡度角、下滑方向与重力分量；结合坡度缓冲和接地吸附优化上下坡浮空、误判滑落等问题。",
    "封装泛型状态机 EntityStateMachine&lt;T&gt; 管理 Grounded、Jump、Air 等状态，并与 Animator 参数联动，降低移动、动画和输入逻辑耦合。",
    "接入 New Input System 封装输入读取层，实现跳跃预输入和土狼时间；实现马头转向、障碍物避让和尾巴骨骼随速度动态摆动。",
    "使用事件、OverlapSphereNonAlloc、缓存数组和 UniTask 控制异步过渡并减少分配；使用 asmdef 独立 GAS 模块程序集，正在实现轻量级 Attribute / Ability / Effect 原型。",
]
for item in sbr_bullets:
    story.append(p("• " + item, styles["bullet"]))

story.append(p("Dream & Life - 2D Roguelite 独立 Demo（2026.03 - 2026.06）", styles["entry"]))
story.append(p("<b>技术栈</b>：Unity、C#、UGUI、2D Physics、Animator、ScriptableObject、状态机、对象池、协程", styles["meta"]))
story.append(p("<b>项目简介</b>：独立开发 2D Roguelite Demo，完成角色控制、战斗技能、装备词条、背包仓库、Buff/DOT 与 UI 刷新等核心系统。演示视频：www.bilibili.com/video/BV1nXTo6UEcM", styles["meta"]))
dream_bullets = [
    "封装玩家状态机，管理 Idle、Move、Jump、Air、Dash、WallSlide、Attack、CounterAttack、Squat、Tears 等 10+ 状态，拆分移动、战斗、形态和动画逻辑。",
    "设计角色数值系统，使用 Stat + Modifier 管理基础属性、装备加成、临时 Buff、DOT 伤害和暴击计算，并通过事件通知 UI 更新。",
    "基于 ScriptableObject 配置物品、技能和装备效果；装备穿脱时动态修改角色属性，支持不同品质、装备类型和随机词条扩展。",
    "实现背包、仓库、装备栏、消耗品栏等 UI 交互，使用 Dictionary 维护物品数据与槽位映射；通过对象池复用子弹、特效等高频对象，减少实例化开销。",
]
for item in dream_bullets:
    story.append(p("• " + esc(item), styles["bullet"]))

story.append(p("药韵八味 - 2D 卡牌经营小游戏（2025.07 - 2025.08）", styles["entry"]))
story.append(p("<b>技术栈</b>：Unity、C#、UGUI、ScriptableObject、AI 接口、Android 适配", styles["meta"]))
medicine_bullets = [
    "双人协作开发传统文化题材卡牌经营小游戏，负责 UI 搭建、资源适配、卡牌合成和部分系统逻辑实现。",
    "使用 ScriptableObject 配置卡牌数据、材料消耗和合成配方；对接大模型 API 实现智能 NPC 对话，并适配 PC 与 Android 双端操作。",
]
for item in medicine_bullets:
    story.append(p("• " + esc(item), styles["bullet"]))

story.append(p("专业技能/其他", styles["section"]))
skill_bullets = [
    "<b>Unity / C#</b>：熟悉 Unity 客户端开发流程，掌握角色控制、物理碰撞、动画状态机、UGUI、ScriptableObject、协程、事件与委托。",
    "<b>工程与性能</b>：了解 Unity 基础资源管理与模块划分，能使用对象池、缓存数组、NonAlloc API、事件解绑等方式减少运行时开销。",
    "<b>基础能力</b>：掌握 List、Dictionary、HashSet、栈、队列、链表等常用数据结构，了解排序、二分查找、双指针、DFS/BFS 等基础算法。",
]
for item in skill_bullets:
    story.append(p("• " + item, styles["bullet"]))

doc = SimpleDocTemplate(
    str(OUT_FILE),
    pagesize=A4,
    leftMargin=16 * mm,
    rightMargin=16 * mm,
    topMargin=9 * mm,
    bottomMargin=9 * mm,
)
doc.build(story)
print(OUT_FILE)
