# CRAP Analysis Skill - Reference Analysis

**Source**: https://github.com/Aaronontheweb/dotnet-skills/blob/master/skills/crap-analysis/SKILL.md

---

## 1. EXACT WORKFLOW

### Initialization Phase
1. **Create `coverage.runsettings`** in repository root (XML config for coverage collection)
2. **Install ReportGenerator** as local tool via `.config/dotnet-tools.json` or globally
3. **Define coverage thresholds** (optional) in `coverage.props`

### Execution Phase (Complete Workflow)
```bash
# Step 1: Clean previous results
rm -rf coverage/ TestResults/

# Step 2: Run unit tests with coverage collection
dotnet test tests/MyApp.Tests.Unit \
  --settings coverage.runsettings \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults

# Step 3: (Optional) Run integration tests
dotnet test tests/MyApp.Tests.Integration \
  --settings coverage.runsettings \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults

# Step 4: Generate HTML report with Risk Hotspots
dotnet reportgenerator \
  -reports:"TestResults/**/coverage.opencover.xml" \
  -targetdir:"coverage" \
  -reporttypes:"Html;TextSummary;MarkdownSummaryGithub"

# Step 5: View results
cat coverage/Summary.txt          # Text summary
open coverage/index.html           # Full HTML report (macOS)
xdg-open coverage/index.html       # Full HTML report (Linux)
start coverage/index.html          # Full HTML report (Windows)
```

### Key Tools & Commands

| Tool | Command | Purpose |
|------|---------|---------|
| **dotnet test** | `--collect:"XPlat Code Coverage"` | Collects coverage in OpenCover format |
| **ReportGenerator** | `reportgenerator` | Generates HTML report from OpenCover XML |
| **Settings** | `--settings coverage.runsettings` | Specifies coverage collection config |
| **Results Dir** | `--results-directory ./TestResults` | Where coverage.opencover.xml is written |

---

## 2. OPENCOVER FORMAT & REPORTGENERATOR

### Coverage Collection
- **Format**: OpenCover XML format (required for complexity metrics)
- **Output File**: `TestResults/**/coverage.opencover.xml` (multiple test runs create multiple XML files)
- **Also Collected**: Cobertura format (simpler, line coverage only)

### ReportGenerator Integration
```bash
dotnet reportgenerator \
  -reports:"TestResults/**/coverage.opencover.xml" \    # Input: OpenCover XML
  -targetdir:"coverage" \                                # Output directory
  -reporttypes:"Html;TextSummary;MarkdownSummaryGithub"  # Output formats
```

**Report Output Artifacts**:
- `coverage/index.html` — Full interactive HTML report with Risk Hotspots section
- `coverage/Summary.txt` — Plain text coverage summary
- `coverage/SummaryGithub.md` — GitHub-compatible markdown (for PR comments)
- `coverage/Cobertura.xml` — Merged Cobertura format (for CI tools)
- `coverage/badge_*.svg` — Coverage badges for README

### What OpenCover XML Contains
The OpenCover format **embeds cyclomatic complexity metrics** that Cobertura does not:
- **Cyclomatic Complexity** per method
- **NPath Complexity** (acyclic execution paths)
- **Coverage %** (line/branch coverage)
- **Hit counts** (how many times each line executed)
- **Branch data** (for conditional coverage)

This is why the skill *requires* OpenCover format — CRAP calculation needs complexity data.

---

## 3. COVERAGE.RUNSETTINGS FILE

### Full Configuration

```xml
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <!-- CRITICAL: Include both cobertura AND opencover formats -->
          <Format>cobertura,opencover</Format>

          <!-- Exclude test/benchmark assemblies by pattern -->
          <Exclude>[*.Tests]*,[*.Benchmark]*,[*.Migrations]*</Exclude>

          <!-- Exclude generated code, obsolete, compiler-generated, and explicit exclusions -->
          <ExcludeByAttribute>
            Obsolete,
            GeneratedCodeAttribute,
            CompilerGeneratedAttribute,
            ExcludeFromCodeCoverageAttribute
          </ExcludeByAttribute>

          <!-- Exclude source-generated files, Blazor code, migrations -->
          <ExcludeByFile>
            **/obj/**/*,
            **/*.g.cs,
            **/*.designer.cs,
            **/*.razor.g.cs,
            **/*.razor.css.g.cs,
            **/Migrations/**/*
          </ExcludeByFile>

          <!-- Don't count test assemblies -->
          <IncludeTestAssembly>false</IncludeTestAssembly>

          <!-- Optimization flags -->
          <SingleHit>false</SingleHit>           <!-- Count multiple hits (not just "executed") -->
          <UseSourceLink>true</UseSourceLink>     <!-- Use SourceLink for GitHub repos -->
          <SkipAutoProps>true</SkipAutoProps>     <!-- Don't count auto-property branches -->
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

### Configuration Breakdown

| Option | Value | Purpose |
|--------|-------|---------|
| `Format` | `cobertura,opencover` | Collect both formats; OpenCover required for CRAP |
| `Exclude` | `[*.Tests]*,[*.Benchmark]*,[*.Migrations]*` | Exclude test/benchmark/migration assemblies |
| `ExcludeByAttribute` | `Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverageAttribute` | Skip generated/obsolete code and explicit opt-outs |
| `ExcludeByFile` | `**/obj/**/*,**/*.g.cs,**/*.designer.cs,**/*.razor.g.cs,**/*.razor.css.g.cs,**/Migrations/**/*` | Skip all auto-generated files |
| `IncludeTestAssembly` | `false` | Don't instrument test assemblies |
| `SingleHit` | `false` | Count all executions, not just presence |
| `UseSourceLink` | `true` | Enable GitHub source link integration |
| `SkipAutoProps` | `true` | Don't count trivial auto-property branches |

### What Gets Excluded

| Pattern | Reason |
|---------|--------|
| `[*.Tests]*` | Test assemblies aren't production code |
| `[*.Benchmark]*` | Benchmark projects |
| `[*.Migrations]*` | Database migrations (auto-generated) |
| `GeneratedCodeAttribute` | Source generators |
| `CompilerGeneratedAttribute` | Compiler-generated code |
| `ExcludeFromCodeCoverageAttribute` | Developer opt-out attribute |
| `*.g.cs` | Generated C# files |
| `*.designer.cs` | Designer-generated files |
| `*.razor.g.cs` | Blazor component generated code |
| `*.razor.css.g.cs` | Blazor CSS isolation generated code |
| `**/Migrations/**/*` | EF Core migrations |

---

## 4. CRAP FORMULA & CALCULATION

### Official Formula

```
CRAP Score = Cyclomatic Complexity × (1 - Coverage)²
```

Where:
- **Cyclomatic Complexity** = Number of independent code paths (if/else, switch, loops, && , || operators)
- **Coverage** = Decimal 0-1 (e.g., 52% = 0.52)

### Example Calculations

| Method | Complexity | Coverage | Formula | CRAP | Risk Level |
|--------|-----------|----------|---------|------|-----------|
| `GetUserId()` | 1 | 0% | 1 × (1 - 0)² = 1 × 1 | **1.0** | ✅ Low |
| `ParseToken()` | 54 | 52% | 54 × (1 - 0.52)² = 54 × 0.23 | **12.4** | ✅ Medium |
| `ValidateForm()` | 20 | 0% | 20 × (1 - 0)² = 20 × 1 | **20.0** | ⚠️ Medium |
| `ProcessOrder()` | 45 | 20% | 45 × (1 - 0.20)² = 45 × 0.64 | **28.8** | 🔴 High |
| `ImportData()` | 80 | 10% | 80 × (1 - 0.10)² = 80 × 0.81 | **64.8** | 🔴 High |

### Risk Level Interpretation

| CRAP Score | Risk Level | Action |
|-----------|-----------|--------|
| < 5 | ✅ Low | Well-tested, safe to modify |
| 5-30 | ⚠️ Medium | Acceptable but watch complexity |
| > 30 | 🔴 High | Needs tests or refactoring immediately |

### Key Insight

The formula emphasizes that **coverage has quadratic impact**:
- 0% coverage → multiplier = 1.0 (full penalty)
- 50% coverage → multiplier = 0.25 (very effective)
- 100% coverage → multiplier = 0 (no penalty)

This means testing high-complexity code is exponentially more valuable than testing simple code.

---

## 5. EDGE CASES & HANDLING

### Issue: Complex but Well-Tested Code
```
OrderProcessor.Calculate()
- Complexity: 28
- Coverage: 85%
- CRAP: 28 × (1 - 0.85)² = 28 × 0.0225 = 0.63 ✅ LOW

→ Acceptable! Complex ≠ risky if well-tested.
```

### Issue: Simple but Untested Code
```
GetUserId()
- Complexity: 1
- Coverage: 0%
- CRAP: 1 × (1 - 0)² = 1.0 ✅ LOW (borderline acceptable)

→ May not flag as high-risk, but still should be tested.
```

### Issue: Code with Generated Attributes
**Solution**: Mark with `[ExcludeFromCodeCoverage]` attribute
```csharp
[ExcludeFromCodeCoverage]
public record PersonAddedEventArgs(Guid PersonId, string Name);
```

This is automatically excluded by `coverage.runsettings` via:
```xml
<ExcludeByAttribute>ExcludeFromCodeCoverageAttribute</ExcludeByAttribute>
```

### Issue: Auto-Generated Files
**Solution**: Already excluded in `coverage.runsettings`:
```xml
<ExcludeByFile>
  **/*.g.cs,                    <!-- Source generators -->
  **/*.designer.cs,             <!-- Designer generated -->
  **/*.razor.g.cs,              <!-- Blazor components -->
  **/*.razor.css.g.cs,          <!-- Blazor CSS isolation -->
  **/Migrations/**/*             <!-- EF Core migrations -->
</ExcludeByFile>
```

### Issue: Auto-Properties Being Counted as Branches
**Solution**: Enable `SkipAutoProps`
```xml
<SkipAutoProps>true</SkipAutoProps>
```

This prevents:
```csharp
public string Name { get; set; }  // Don't count as branch coverage
```

### Issue: Third-Party Wrapper Code
**Skill recommendation**: Lower thresholds temporarily (document in README)

**Never lower for**:
- "Too hard to test" → refactor instead
- "We'll add tests later" → add them now
- New features → should meet standards from start

---

## 6. OTHER FILES IN DIRECTORY

The `skills/crap-analysis/` directory contains **only one file**:
- `SKILL.md` (the skill document itself)

**No auxiliary files** like templates, scripts, or helper tools.

---

## 7. CI/CD INTEGRATION PATTERNS

### GitHub Actions Example

```yaml
name: Coverage

on:
  pull_request:
    branches: [main, dev]

jobs:
  coverage:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Restore tools
        run: dotnet tool restore

      - name: Run tests with coverage
        run: |
          dotnet test \
            --settings coverage.runsettings \
            --collect:"XPlat Code Coverage" \
            --results-directory ./TestResults

      - name: Generate report
        run: |
          dotnet reportgenerator \
            -reports:"TestResults/**/coverage.opencover.xml" \
            -targetdir:"coverage" \
            -reporttypes:"Html;MarkdownSummaryGithub;Cobertura"

      - name: Upload coverage report
        uses: actions/upload-artifact@v4
        with:
          name: coverage-report
          path: coverage/

      - name: Add coverage to PR
        uses: marocchino/sticky-pull-request-comment@v2
        with:
          path: coverage/SummaryGithub.md
```

### Azure Pipelines Example

```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run tests with coverage'
  inputs:
    command: 'test'
    arguments: '--settings coverage.runsettings --collect:"XPlat Code Coverage" --results-directory $(Build.SourcesDirectory)/TestResults'

- task: DotNetCoreCLI@2
  displayName: 'Generate coverage report'
  inputs:
    command: 'custom'
    custom: 'reportgenerator'
    arguments: '-reports:"$(Build.SourcesDirectory)/TestResults/**/coverage.opencover.xml" -targetdir:"$(Build.SourcesDirectory)/coverage" -reporttypes:"HtmlInline_AzurePipelines;Cobertura"'

- task: PublishCodeCoverageResults@2
  displayName: 'Publish coverage'
  inputs:
    codeCoverageTool: 'Cobertura'
    summaryFileLocation: '$(Build.SourcesDirectory)/coverage/Cobertura.xml'
```

---

## 8. QUICK REFERENCE COMMANDS

### One-Liner (Full Analysis)
```bash
rm -rf coverage/ TestResults/ && \
dotnet test --settings coverage.runsettings \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults && \
dotnet reportgenerator \
  -reports:"TestResults/**/coverage.opencover.xml" \
  -targetdir:"coverage" \
  -reporttypes:"Html;TextSummary"
```

### View Summary
```bash
cat coverage/Summary.txt
```

### Open Report
```bash
# macOS
open coverage/index.html

# Linux
xdg-open coverage/index.html

# Windows
start coverage/index.html
```

### Filter by Test Class
```bash
dotnet test --settings coverage.runsettings \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults \
  -- --filter-class "*MyTests"
```

### Filter by Test Method
```bash
dotnet test --settings coverage.runsettings \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults \
  -- --filter-method "*ShouldThrow*"
```

---

## 9. RECOMMENDED STANDARDS

### Coverage Thresholds

| Coverage Type | Target | Purpose |
|--------------|--------|---------|
| **Line Coverage** | > 80% | Ensure most code paths are tested |
| **Branch Coverage** | > 60% | Catch conditional logic gaps |
| **CRAP Score** | < 30 | No method should exceed maximum risk |

### For New Code
- Line Coverage: **80%+**
- Branch Coverage: **60%+**
- Maximum CRAP: **30**

### For Legacy Code (Gradual Improvement)
- Line Coverage: **60%+** (improve to 80%+)
- Branch Coverage: **40%+** (improve to 60%+)
- No specific CRAP target (document exceptions)

---

## 10. OPENCOVER XML STRUCTURE (Key Fields)

Based on Coverlet documentation, OpenCover XML contains:

```xml
<CoverageSession>
  <Modules>
    <Module>
      <Classes>
        <Class>
          <Methods>
            <Method>
              <Name>GetUserId</Name>
              <MetadataToken>...</MetadataToken>
              <FileRef uid="1" />
              <SequencePoints>
                <SequencePoint ... />
              </SequencePoints>
              <MethodPoint
                vc="5"                    <!-- Visit count -->
                uspid="1"                 <!-- Unique sequence point ID -->
                ordinal="0"
                offset="0"
                p1="3"                    <!-- Cyclomatic Complexity! -->
                p2="0"
                p3="1"
                p4="0"
                p5="0"
                p6="..."
                p7="0"
                p8="0"
                p9="0"
                p10="1"
              />
            </Method>
          </Methods>
        </Class>
      </Classes>
    </Module>
  </Modules>
</CoverageSession>
```

**Key Attributes**:
- `vc` = Visit count (how many times executed)
- `p1` = **Cyclomatic Complexity** (the key value for CRAP)
- SequencePoints = Individual executable statements

This is what makes OpenCover special — `p1` attribute contains complexity.

---

## SUMMARY

| Aspect | Details |
|--------|---------|
| **Workflow** | Clean → Test (with coverage) → Generate Report → Analyze |
| **Tools** | dotnet test (XPlat Code Coverage), ReportGenerator |
| **Format** | OpenCover XML (required for CRAP) + Cobertura |
| **Configuration** | `coverage.runsettings` at repo root |
| **Formula** | CRAP = Complexity × (1 - Coverage)² |
| **Risk Thresholds** | < 5 (low), 5-30 (medium), > 30 (high) |
| **Edge Cases** | Generated code, auto-properties, third-party wrappers |
| **Auxiliary Files** | None (only SKILL.md) |
| **CI Integration** | GitHub Actions, Azure Pipelines (examples provided) |

