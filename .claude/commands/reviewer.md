---
description: Reviewer agent. Analyzes a branch for security issues, code quality, and CLAUDE.md violations. Reports findings, then pushes and creates a PR on user confirmation. Usage: /reviewer [branch-name or issue-number]
---

You are in **Reviewer mode** for: `$ARGUMENTS`

Your job is to review the implementation on a branch and decide whether it's ready to push as a PR to `staging`.

---

## Step 1 — Identify the branch

If `$ARGUMENTS` is a number, find the branch:
```
git branch -a | grep "/$ARGUMENTS-"
```
If `$ARGUMENTS` is already a branch name, use it directly.

Set `BRANCH` to the branch name for the rest of this workflow.

---

## Step 2 — Understand the scope

Find the linked issue number from the branch name (e.g. `feature/23-...` → issue #23).
Read the issue using `mcp__github__issue_read` to understand the intended scope.

---

## Step 3 — Analyze the changes

```
git log main...[BRANCH] --oneline
git diff main...[BRANCH] --stat
git diff main...[BRANCH]
```

Read each changed file in full using the Read tool to understand context beyond the diff.

---

## Step 4 — Review checklist

Work through each category. Mark findings as **Blocker**, **Warning**, or **Info**.

### Scope
- Does the implementation match what the issue asked for — no more, no less?
- Are there any unrelated changes mixed in?

### Architecture & Clean Code
- No cross-layer violations (EF Core / DbContext referenced in Application; business logic in Web layer)
- No duplicate logic — shared code extracted to Domain/Scoring, Application/Common, or a Razor partial as appropriate
- New entities follow the 8-step recipe in CLAUDE.md
- New request handlers follow the recipe in CLAUDE.md

### Result Pattern
- Handlers return `CommandResult.Ok/Fail/FailGeneral` — no exceptions thrown for control flow
- Pages check `result.Success` / `result.IsGeneralError` correctly
- No `throw` for expected failure scenarios (entity not found, validation failed, business rule violated)

### Security
- No raw SQL with user-supplied input (parameterized queries or EF only)
- No `Html.Raw()` with unescaped user data
- No sensitive data (passwords, tokens, PII) leaking into ViewModels or public pages
- All admin pages decorated with `[AdminOnly]`
- No new environment variables or secrets hardcoded in source

### UI
- No unnecessary JavaScript introduced
- No custom CSS frameworks — Bootstrap only
- Polish-language UI text is consistent with existing pages
- No new AJAX handlers unless unavoidable (see CLAUDE.md UI principles)

### General quality
- No speculative features or abstractions beyond the issue scope
- No unnecessary error handling for impossible scenarios
- No docstrings or comments added to unchanged code
- No backwards-compatibility hacks (unused variables, re-exported types, `// removed` comments)
- Build passes (check commit message or ask the user to confirm)

---

## Step 5 — Present findings

Organize your findings by severity:

```
### Blockers (must fix before merging)
- [file:line] Description

### Warnings (should fix)
- [file:line] Description

### Info (minor observations)
- [file:line] Description

### Verdict
Ready to push / Needs fixes first
```

If there are **Blockers**, stop here. Help fix them if the user asks, then re-run the review from Step 3.

If there are no blockers, ask the user:
> "No blockers found. Should I push the branch and open a PR to staging?"

---

## Step 6 — Push and create PR (only after explicit user confirmation)

1. Show a summary of what will be pushed:
   - Branch name
   - Commits (`git log main...[BRANCH] --oneline`)

2. Push:
   ```
   git push origin [BRANCH]
   ```

3. Create a PR targeting `staging` using `mcp__github__create_pull_request`:
   - **Title:** short and descriptive (under 70 characters)
   - **Base branch:** `staging`
   - **Body:** include `Closes #[issue-number]` — no test plan or verification checklist (per CLAUDE.md)
   - **Labels:** same type label as the issue

4. Return the PR URL to the user.
