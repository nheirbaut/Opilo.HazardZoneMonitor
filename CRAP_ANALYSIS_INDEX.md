# CRAP Analysis Skill Reference - Complete Index

This directory contains a comprehensive analysis of the CRAP (Change Risk Anti-Patterns) Analysis skill from [Aaronontheweb/dotnet-skills](https://github.com/Aaronontheweb/dotnet-skills/blob/master/skills/crap-analysis/SKILL.md).

## 📄 Documents in This Reference

### 1. **CRAP_EXECUTIVE_SUMMARY.md** ⭐ START HERE
- **Length**: ~295 lines
- **Purpose**: Quick answers to your 6 core questions
- **Contains**:
  - ✅ Exact workflow it prescribes (commands, tools, formats)
  - ✅ How it uses OpenCover format and ReportGenerator
  - ✅ Complete coverage.runsettings file details
  - ✅ CRAP formula with 5 real calculation examples
  - ✅ Edge cases and how skill handles them
  - ✅ Directory contents (only SKILL.md, no auxiliaries)
  - Quick integration checklist
  - Key takeaways

### 2. **CRAP_REFERENCE_ANALYSIS.md** COMPREHENSIVE
- **Length**: ~498 lines  
- **Purpose**: Deep dive into every aspect of the skill
- **Contains** (10 detailed sections):
  1. Exact workflow (initialization + execution phases)
  2. OpenCover format & ReportGenerator integration
  3. Complete coverage.runsettings breakdown (all options explained)
  4. CRAP formula with example calculations
  5. Edge cases & how skill handles them
  6. Directory structure (no auxiliary files)
  7. CI/CD integration patterns (GitHub Actions + Azure Pipelines)
  8. Quick reference commands
  9. Recommended standards & thresholds
  10. OpenCover XML structure (key fields for complexity metrics)

## 🎯 Quick Navigation

### If you need to...

**Understand the complete workflow**
→ See Executive Summary section 1 + Reference Analysis section 1

**Set up coverage collection**
→ See Reference Analysis section 3 (complete coverage.runsettings with annotations)

**Understand CRAP scoring**
→ See Executive Summary section 4 + Reference Analysis section 4

**Handle edge cases (generated code, auto-properties, migrations)**
→ See Executive Summary section 5 + Reference Analysis section 5

**Integrate with CI/CD**
→ See Reference Analysis section 7 (GitHub Actions + Azure Pipelines)

**Understand OpenCover XML internals**
→ See Reference Analysis section 10 (structure with cyclomatic complexity attributes)

## 📚 Source Material

**Original Skill**: https://github.com/Aaronontheweb/dotnet-skills/blob/master/skills/crap-analysis/SKILL.md
- Repository: Aaronontheweb/dotnet-skills
- File: `skills/crap-analysis/SKILL.md`
- SHA: fa6d9ccfc666975b0ee7deef4fb53993ff62f000
- Size: 11,520 bytes (387 lines, 291 lines of content)
- Status: Single file (no auxiliary files in directory)

## 🔗 External Resources Linked in Skill

| Resource | URL | Purpose |
|----------|-----|---------|
| Coverlet Documentation | https://github.com/coverlet-coverage/coverlet | Coverage collection framework (XPlat Code Coverage) |
| ReportGenerator | https://github.com/danielpalme/ReportGenerator | Transforms OpenCover/Cobertura XML into HTML/text reports |
| CRAP Score Paper | http://www.artima.com/weblogs/viewpost.jsp?thread=215899 | Original CRAP concept by Curt Harbison |

## ⚡ TL;DR - The 30-Second Version

**What**: CRAP = Cyclomatic Complexity × (1 - Coverage)²  
**Why**: Identifies high-risk code (complex + untested = dangerous)  
**How**: 
1. Create `coverage.runsettings` (excludes tests, generated code, migrations)
2. Run: `dotnet test --settings coverage.runsettings --collect:"XPlat Code Coverage" --results-directory ./TestResults`
3. Generate report: `reportgenerator -reports:"TestResults/**/coverage.opencover.xml" -targetdir:"coverage" -reporttypes:"Html"`
4. View `coverage/index.html` → Risk Hotspots section
5. Target: CRAP < 30 for new code, > 80% line coverage

**Key**: OpenCover format (not Cobertura) required because it includes cyclomatic complexity metrics (`p1` attribute)

## 🚀 Implementation Checklist

- [ ] Review Executive Summary (section 1-6)
- [ ] Copy `coverage.runsettings` from Reference Analysis section 3 to repo root
- [ ] Install ReportGenerator: `dotnet tool install --global dotnet-reportgenerator-globaltool`
- [ ] Create GitHub Actions workflow (Reference Analysis section 7)
- [ ] Run test → collect → generate → analyze cycle
- [ ] Identify high-CRAP methods (> 30) in Risk Hotspots
- [ ] Plan testing or refactoring for high-risk code

## 📊 CRAP Risk Levels at a Glance

```
CRAP < 5      ✅ LOW       → Well-tested, safe to modify
5-30          ⚠️  MEDIUM    → Acceptable but watch it
> 30          🔴 HIGH      → Needs tests or refactoring
```

## 🎓 Key Insights

1. **Coverage has quadratic impact**: 50% coverage is 4× better than 0%, not 2×
2. **Complexity ≠ risk if tested**: A 45-line complex method with 95% coverage is safer than a 5-line untested getter
3. **OpenCover is mandatory**: Cobertura format lacks complexity metrics; ReportGenerator needs both to calculate CRAP
4. **Edge cases handled elegantly**: Generated code, auto-properties, and migrations excluded via patterns
5. **No scripts needed**: Everything is in dotnet test + ReportGenerator

---

**Last Updated**: Mar 14, 2026  
**Source Analysis Date**: Mar 14, 2026  
**Analyzed Skill Commit**: fa6d9ccfc666975b0ee7deef4fb53993ff62f000
