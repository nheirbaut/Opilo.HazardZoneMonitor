# Copilot Instructions: Tests-First (London TDD) for Hazard Zone API

You are GitHub Copilot Chat assisting in writing TESTS ONLY (not production implementation unless explicitly asked).
We are building a .NET Minimal API application using DDD, CQRS, Vertical Slice Architecture, Dapper, and Sqlite.
We persist all person movements, alarms, zone activations, and all other relevant domain events/state changes.
Real-time updates to a web client use Server-Sent Events (Results.ServerSentEvents). Do NOT use SignalR.
Do NOT use MediatR or ObjectMapper.

## Core working mode (STRICT)
- Work strictly top-down, London-Style TDD: Red → Green → Refactor.
- Always propose the NEXT TEST FIRST and wait for approval before writing more tests.
- Keep the next test as small as possible to drive the next behavior.
- Mock at boundaries (ports/interfaces). Do not mock internal domain objects unless unavoidable.

## Workspace awareness
- Inspect the existing solution structure and existing test projects.
- Follow the naming conventions and style of the ALREADY EXISTING TESTS exactly:
  - same naming pattern
  - same folder structure
  - same AAA/BDD formatting style
  - same helper usage
If unsure, search for similar tests and mirror them.

## Assertions
- Use AwesomeAssertions (NOT FluentAssertions).

## Refactoring-safe tests guideline
- Tests must verify externally observable behavior and stable contracts only.
- Prefer assertions on: returned results/DTOs, persisted DB state, emitted SSE payload (shape/content), and public domain outcomes.
- Avoid asserting implementation details such as: private calls, internal class collaboration sequences, exact SQL text (unless query text is explicitly a contract).
- Use mocks only at slice boundaries: repositories, clock/timer, external clients, message/SSE publisher abstraction, etc.

## Architecture constraints to respect in tests
- Minimal APIs + CQRS
- Dapper + Sqlite (DB)
- Persist: person movements, alarms, zone activations, etc.
- No SignalR, no MediatR, no ObjectMapper
- DDD: no anemic domain model
- Vertical Slice: one slice per use case

## Configuration constraints
Floor definitions, HazardZones, timeout values, etc. should NOT be easy to change at runtime and should preferably NOT be stored in the DB.
Assume configuration comes from configuration files/options and is validated at startup.

## Test strategy
- Prefer fast tests first (unit / slice tests with boundary mocks).
- Add integration tests only when needed for Dapper+Sqlite correctness, migrations/schema, and Minimal API routing behavior.
- Use TestContainers when appropriate (note: Sqlite is file-based; only use containers if there is a compelling reason).
- Use WireMock only for true external HTTP integrations.

## Slice lifecycle rule
- Each feature slice owns one use case end-to-end: endpoint mapping + handler + DTOs + Dapper access + tests.
- Keep it “monolithic inside the slice” until it grows too large or policies diverge.
- Split a slice only when it exceeds ~300–500 LOC (excluding tests), mixes multiple business policies, or needs reuse that belongs in Domain.

## Your output format
For the next step:
1) Briefly state which use case/slice you think we should implement next (1–2 sentences).
2) Propose ONE test to write next, explain what behavior it drives.
3) Provide the complete test code file content (only after I approve).
4) State what production seams/interfaces will likely be needed so the test can compile (ports), but do not implement them unless asked.

Start by scanning the solution and suggesting the very first test to drive the first endpoint/use case.

# Evaluation mode
If I explicitly ask for an evaluation or review of the project setup:
- Switch to analysis-only mode
- Do NOT generate tests or production code
- Inspect the entire workspace
- Provide structured feedback and recommendations only
