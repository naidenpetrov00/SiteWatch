# Test design

## Select coverage by risk

| Risk | Preferred first test |
|---|---|
| Pure business invariant or transformation | Unit test |
| Use-case orchestration across owned ports | Unit test with narrow substitutes |
| SQL translation, EF mapping, or transaction | Integration test with SQL Server |
| HTTP contract, middleware, auth, or serialization | API integration test |
| Rendered state and user interaction | Component test |
| Cross-application critical journey | End-to-end test |

Escalate only when the narrower surface cannot prove the risky behavior.

## Build a useful case set

Identify the representative success path, boundary values and transitions, invalid or unauthorized input, and failures from important external boundaries. Cover idempotency, concurrency, retry, or cancellation only where promised. Pair every case with a distinct risk; do not generate combinations mechanically.

## Arrange data clearly

Keep explanatory values inline and name their role. Add a builder when setup is repeated, noisy, or full of irrelevant defaults. Keep builders in the owning test feature and expose intent such as `Expired()`, `WithoutPermission()`, or `WithInvoiceTotal(...)`.

Avoid random defaults unless seeded randomness is useful. A failure must reproduce from source and output alone.

## Judge a mock

Ask whether the collaborator is an architectural boundary, replacing it improves determinism or safety, the assertion can focus on the subject's outcome, and a real lightweight implementation would be less clear. If most answers are no, keep it real or choose an integration test.

## Review quality

A strong test fails for the intended regression, survives behavior-preserving refactors, is isolated from order and machine state, uses the smallest realistic setup, rejects plausible incorrect implementations, and leaves no unexplained warnings or unobserved async work.
