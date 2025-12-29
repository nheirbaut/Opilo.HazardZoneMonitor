You are GitHub Copilot (or another LLM) in evaluation mode.

Your task is to inspect the ENTIRE workspace and provide a critical evaluation of:
1) The application architecture
2) The test project setup and organization

Do NOT generate production code.
Do NOT generate tests.

This is an analysis-only task.

---

## Scope of the evaluation

### 1. Application architecture
Evaluate the project with respect to:

- Domain-Driven Design (non-anemic domain model)
- Vertical Slice Architecture fit
- CQRS clarity and enforcement
- Dependency direction and boundaries
- Coupling between API, Application, Domain, Infrastructure
- Suitability for Dapper + Sqlite
- Event persistence strategy (movements, alarms, activations, etc.)
- Read models / projections approach
- Transaction boundaries and consistency risks
- Minimal API organization and DTO usage
- OpenAPI / Swagger readiness
- Server-Sent Events readiness (without SignalR)
- Configuration strategy for floors, hazard zones, timeout values
- Startup validation and runtime mutability risks

---

### 2. Test project evaluation (MANDATORY)
Evaluate the CURRENT test projects in detail:

#### Test project structure
- Are the right test projects present?
- Are the project names correct and descriptive?
- Is there a clear separation between:
  - Unit / slice tests
  - Integration tests
  - API / HTTP tests
  - Persistence tests (Dapper + Sqlite)
- Are any test types mixed incorrectly in the same project?

#### Test project responsibilities
- Does each test project have a single, clear responsibility?
- Are London-style TDD principles supported by the structure?
- Are mocking boundaries clear and consistent?
- Are tests located in the correct project based on what they test?

#### Naming & conventions
- Do test project names reflect what they test?
- Do namespaces and folder structures reinforce intent?
- Are test names consistent and readable?
- Are there signs of accidental or historical drift?

#### Fitness for future work
- Does the setup support incremental, top-down TDD?
- Will adding new vertical slices be straightforward?
- Are there risks of slow, brittle, or overly broad test suites?
- Is the structure understandable for a new developer or LLM?

---

## Output format (MANDATORY)

Structure your response as follows:

1. **Summary**
   - High-level assessment of application + test setup

2. **What works well**
   - Concrete positives in architecture and test organization

3. **Problems / risks**
   - Concrete issues in:
     - Architecture
     - Test project structure
     - Naming or responsibility boundaries
   - Explain *why* each issue matters

4. **Recommendations**
   - Actionable, incremental improvements
   - Include:
     - Suggested test project names (if changes are needed)
     - Suggested responsibilities per test project
     - Guidance on where specific kinds of tests should live
   - NO code, NO tests — guidance only

5. **What NOT to change**
   - Things that are correct and should remain stable

---

## Ambiguity rule
If something is unclear (e.g. intent of a test project), ask questions instead of assuming.
