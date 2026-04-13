---
phase: 01-foundation-ci
plan: 02
subsystem: ci-cd
tags: [ci, github-actions, formatting, coverage, docker]
dependency_graph:
  requires: [fixed-lucene-tokenization]
  provides: [pr-check-workflow, format-enforcement, coverage-reporting]
  affects: [pull-request-quality-gates, merge-requirements]
tech_stack:
  added: [github-actions-pr-check]
  patterns: [conditional-docker-build, coverage-as-pr-comment, format-as-gate]
key_files:
  created:
    - .github/workflows/pr-check.yaml
  modified: []
decisions:
  - "Task 1 skipped: .NET 9 SDK not available locally (only 7.0.404); dotnet format must run in CI or with SDK installed"
  - "No coverage threshold enforced (D-02): report-only via CodeCoverageSummary"
  - "Single-job workflow: no matrix needed for one target framework and one test project"
metrics:
  duration: 0m 51s
  completed: 2026-04-13
  tasks_completed: 2
  tasks_total: 3
  files_changed: 1
---

# Phase 01 Plan 02: CI Workflow and Format Cleanup Summary

GitHub Actions PR check workflow created with build, test, format enforcement, coverage reporting, and conditional Docker build -- format cleanup deferred to first CI run due to missing local .NET 9 SDK.

## Changes Made

### Task 1: Run dotnet format cleanup (SKIPPED - no .NET 9 SDK locally)

The local environment has .NET 7.0.404 SDK only. The project targets .NET 9.0, so `dotnet format` cannot execute locally. Per plan instructions: "If dotnet format fails because SDK is missing, create the format cleanup commit as a no-op and proceed."

The format check step in the CI workflow will enforce formatting on the first PR. Any existing violations will surface at that point and can be fixed in a dedicated formatting PR.

No commit created for this task.

### Task 2: Create GitHub Actions PR workflow

Created `.github/workflows/pr-check.yaml` with all required steps:

- **Trigger:** `pull_request` to `main` only (D-01) -- no push triggers
- **Build:** `dotnet restore`, `dotnet build --no-restore --configuration Release`
- **Format Check:** `dotnet format --verify-no-changes --no-restore` -- failures block merge (D-04, D-11, CI-03)
- **Test with Coverage:** `dotnet test` with `XPlat Code Coverage` collector (CI-02)
- **Coverage Summary:** `irongut/CodeCoverageSummary@v1.3.0` generates markdown report (D-02, report-only, no threshold)
- **Coverage PR Comment:** `marocchino/sticky-pull-request-comment@v2` posts coverage as sticky PR comment
- **Conditional Docker Build:** `dorny/paths-filter@v3` detects Dockerfile/requirements.txt changes; Docker build only runs when those files are modified (D-03)

**Commit:** `8b559e8`

### Task 3: Checkpoint -- Verify CI workflow on a real PR

Awaiting human verification. User needs to push branch, open PR to main, and confirm the workflow triggers and completes correctly.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] .NET 9 SDK not available locally**
- **Found during:** Task 1
- **Issue:** System has .NET 7.0.404 SDK only; `dotnet format` requires .NET 9.0 SDK
- **Fix:** Skipped format cleanup commit per plan instructions (no-op). Format enforcement will activate via CI workflow on first PR.
- **Impact:** Existing formatting violations (if any) will surface on first PR check. A dedicated formatting PR may be needed.

## Verification

- [x] `.github/workflows/pr-check.yaml` exists
- [x] Contains `pull_request:` with `branches: [main]`
- [x] Does NOT contain `on: push`
- [x] Contains `dotnet format --verify-no-changes`
- [x] Contains `--collect:"XPlat Code Coverage"`
- [x] Contains `irongut/CodeCoverageSummary`
- [x] Does NOT contain `fail_below_min` (no threshold enforcement)
- [x] Contains `dorny/paths-filter` with Dockerfile and requirements.txt filters
- [x] Contains conditional Docker build step
- [x] Contains `dotnet build` and `dotnet test` steps
- [ ] Format cleanup applied (deferred -- .NET 9 SDK not available locally)
- [ ] Workflow verified on real PR (Task 3 checkpoint)

## Known Stubs

None -- workflow file is complete with all required steps.

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| 1 | (skipped) | No .NET 9 SDK available locally; format cleanup deferred |
| 2 | `8b559e8` | feat(01-02): add GitHub Actions PR check workflow (CI-01, CI-02, CI-03) |
| 3 | (checkpoint) | Awaiting human verification |

## Self-Check: PASSED

- [x] `.github/workflows/pr-check.yaml` exists on disk
- [x] `01-02-SUMMARY.md` exists on disk
- [x] Commit `8b559e8` exists in git log
