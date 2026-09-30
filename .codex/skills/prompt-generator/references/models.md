# GPT-5.6 model and Codex execution guide

Use this catalog for task recommendations. It intentionally covers GPT-5.6 only; do not recommend or add GPT-6 models unless this reference is explicitly expanded.

## GPT-5.6 models

All GPT-5.6 models accept text and image input and produce text. They have a 1,050,000-token context window, a 128,000-token maximum output, and a February 16, 2026 knowledge cutoff. In the API, `gpt-5.6` is an alias for `gpt-5.6-sol`.

| Model | Model ID | Use for | Tradeoff |
| --- | --- | --- | --- |
| Sol | `gpt-5.6-sol` | Difficult architecture, ambiguous cross-system work, complex debugging, security-sensitive work, and quality-first reviews | Strongest and most expensive GPT-5.6 tier |
| Terra | `gpt-5.6-terra` | Typical repository implementation, review, refactoring, and multi-file work using established patterns | Balanced capability, latency, and cost |
| Luna | `gpt-5.6-luna` | Clear, localized, repeatable, high-volume, or mechanical work | Fastest and lowest-cost tier; less suitable for ambiguity |

Model availability in Codex depends on the plan, client, workspace configuration, and rollout. The API model IDs and capabilities do not by themselves guarantee Codex availability.

## Reasoning

GPT-5.6 supports these API reasoning efforts: `none`, `low`, `medium`, `high`, `xhigh`, and `max`. `medium` is the default when omitted, in both reasoning modes.

| Effort | Recommend for |
| --- | --- |
| `none` | Latency-critical classification, retrieval, or purely mechanical work with no multi-step reasoning or chained tool use |
| `low` | Well-scoped work where speed matters, including light planning, search, or tool use |
| `medium` | Default balanced choice for implementation, research, and work that needs planning or judgment |
| `high` | Difficult debugging, deep planning, complex agentic work, or high-value tasks |
| `xhigh` | Long-running, deeply investigative, security-review, or complex coding tasks when evaluation shows a benefit |
| `max` | The hardest quality-first tasks; compare with `xhigh` before making it the default |

Start at the lowest effort likely to meet the task's quality bar; increase it only when complexity, risk, or evaluation evidence justifies the extra latency and token use.

## Reasoning mode and continuity

- The Responses API has independent `reasoning.mode` and `reasoning.effort` controls. Use `standard` by default.
- Use `reasoning.mode: "pro"` for difficult, high-value tasks where a reliability gain is worth higher latency and token usage. Keep the chosen model and effort; do not switch to a separate “Pro” model ID.
- GPT-5.6 defaults `reasoning.context` to `all_turns`, allowing available prior reasoning to inform later turns. Use it when the goal, assumptions, and priorities remain stable; choose `current_turn` when earlier reasoning no longer applies.
- For multi-turn tool workflows, prefer the Responses API and continue with `previous_response_id` so available reasoning can be reused. Reasoning can be reused within the GPT-5.6 family, not across model families.

## Codex execution modes

These are Codex workflow controls, not API reasoning parameters.

- **Default Mode:** Use for small, self-contained work that can be safely completed in one focused pass.
- **Plan Mode:** Ask Codex to propose the implementation path before making changes. Use for underspecified, risky, or multi-system work where the boundaries and approach need review.
- **Goal Mode:** Set a durable outcome for work expected to span multiple iterations or turns. A goal preserves what must be true at completion while Codex works through implementation, verification, review feedback, and cleanup.
- Do not recommend Plan Mode and Goal Mode together. Use Plan Mode first for a risky change; after the plan is accepted, create a Goal only if sustained follow-through is needed.
- **Ultra, where available:** This is a Codex execution option rather than an API `reasoning.effort`. It uses subagents for independent workstreams. Recommend it only when the task divides cleanly into meaningful parallel parts; most tasks do not need it.

## Selection defaults for generated prompts

- Small, clear, low-risk task: Luna with `low`, Default Mode.
- Typical repository task: Terra with `medium`, Default Mode; use Plan Mode when it has meaningful dependencies or tradeoffs.
- High-risk or ambiguous task: Sol with `high`, Plan Mode.
- Long-running, multi-iteration objective: Sol with `high`, Goal Mode.
- Escalate to `xhigh`, `max`, `pro`, or Ultra only when the task specifically warrants the cost and latency.

## Sources

Verified September 30, 2026 from official OpenAI documentation.

- [GPT-5.6 model guidance](https://developers.openai.com/api/docs/guides/latest-model?model=gpt-5)
- [Reasoning models and modes](https://developers.openai.com/api/docs/guides/reasoning)
- [GPT-5.6 Sol model details](https://developers.openai.com/api/docs/models/gpt-5.6-sol)
- [Codex models and reasoning](https://learn.chatgpt.com/docs/models)
- [Using Plan and Goal in Codex](https://learn.chatgpt.com/blog/mastering-codex-remote-for-engineering)
