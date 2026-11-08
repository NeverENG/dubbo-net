# RFC conventions / RFC 约定

[English code map](en/index.md) · [中文代码地图](zh/index.md) · [Git-indexed changelog / Git 索引](../CHANGELOG.md)

**EN:** Write an RFC only for a real architecture decision with alternatives and trade-offs. An RFC explains why a decision was made; code and tests describe what the system currently does. Use the linked Issue and implementation PRs to track progress instead of maintaining an RFC work log. Do not create an empty RFC for a planned feature.

**中文：** 仅在存在真实架构决策、备选方案与取舍时编写 RFC。RFC 解释为什么作出决定；当前行为仍以代码和测试为准。进度交给关联 Issue 和实施 PR 追踪，不用 RFC 维护施工日志，也不为待办功能预造空 RFC。

## Naming and publication / 编号与发布

- **EN:** Reserve the next available four-digit ID when the first proposal is opened, such as `0001`; never reuse an ID. For a decision intended for both language audiences, create `Docs/rfcs/0001-topic.en.md` and `Docs/rfcs/0001-topic.zh.md` in the **same PR**, with the same ID, status, decision and links. Use a short lowercase hyphenated topic. Create `Docs/rfcs/` only when an actual RFC is proposed. A translation-only correction keeps the same ID and records its source PR.
- **中文：** 首次提出方案时取下一个未使用的四位编号（例如 `0001`），编号不复用。需要面向两种语言读者的决定，在**同一 PR** 中提供 `Docs/rfcs/0001-topic.en.md` 和 `Docs/rfcs/0001-topic.zh.md`，两版的编号、状态、决定及关联链接一致；主题用简短的小写连字符形式。仅在确有 RFC 时创建目录。单独修正翻译仍沿用原编号，并关联修改 PR。
- **EN:** Use `Proposed` while alternatives are under review, `Accepted` after a recorded decision, `Superseded` when replaced by a linked RFC, or `Withdrawn` when abandoned. Do not mark a proposal accepted because code happens to exist; link the decision and implementation separately. Update both language versions when status or outcome changes.
- **中文：** 备选方案讨论中为 `Proposed`；决策得到确认后为 `Accepted`；被后续 RFC 取代为 `Superseded`，需链接继任 RFC；放弃则为 `Withdrawn`。不能仅凭已有代码把提案标成已接受；决策与实施分别关联。状态或结论变化时同步两种语言。

## Minimum template / 最小模板

Copy the appropriate outline below only when drafting an actual RFC. Link to a concrete Issue, decision discussion and implementation PR/commit **when available**; label unimplemented work as such. A decision may be accepted before implementation, but the RFC must not imply the implementation has shipped.

仅在起草实际 RFC 时使用下列大纲。关联具体 Issue、决策讨论和**已有的**实施 PR/commit；尚未实施时明确写明。接受一个设计不等于代码已经发布。

```markdown
# RFC-NNNN: Short title

Status: Proposed | Accepted | Superseded | Withdrawn
Issue: link
Decision / review: link or "pending"
Implementation: PR/commit link or "not implemented"

## Context and constraints
What problem exists, and what must remain compatible?

## Options and trade-offs
Include the chosen option and at least one realistic alternative; explain costs.

## Decision
State the recorded choice, or "pending" while Proposed.

## Consequences
Describe limitations and follow-up work, without claiming unshipped behavior.
```

```markdown
# RFC-NNNN：简短标题

状态：Proposed | Accepted | Superseded | Withdrawn
问题：链接
决策／评审：链接或“待确认”
实施：PR/commit 链接或“尚未实施”

## 背景与约束
现有问题是什么，哪些兼容性必须保留？

## 方案与取舍
列出选定方案和至少一个真实备选方案，说明成本。

## 决定
写下已记录的选择；仍为 Proposed 时写“待确认”。

## 后果
记录限制和后续工作，不宣称未发布的行为。
```
