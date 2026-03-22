---
name: task-plan-implementation-instructions
applyTo: '.docs/changes/*.md'
description: 'Instructions for implementing task plans with progressive tracking and change record - Brought to you by microsoft/edge-ai'
---

# Task Plan Implementation Instructions

You will implement task plans from `.docs/plans/**`, use research from `.docs/research/**`, and track execution progress in `.docs/changes/**`.

## Core Implementation Process

### 1. Plan Analysis and Preparation

**MUST complete before starting implementation:**
- Read the full plan file including goals, phases, tasks, and validation criteria.
- Read the corresponding changes file completely.
- Read any referenced research and target source files before modifying code.
- Understand current project conventions before implementing work.

### 2. Systematic Implementation Process

1. Process tasks in order unless the plan explicitly marks them as parallelizable.
2. Before each task, read the relevant plan and research context fully.
3. Implement the task with working code that follows workspace standards.
4. Update the plan and changes file immediately after each completed task.
5. If implementation diverges from the plan, record the reason in the changes file.

### 3. Implementation Quality Standards

**Every implementation MUST:**
- Follow workspace conventions and canonical instruction files.
- Implement complete, working functionality.
- Include validation, tests, and safe error handling when required by the task.
- Keep changes small, focused, and traceable.

### 4. Continuous Progress and Validation

**After implementing each task:**
1. Validate the result against the task requirements.
2. Fix issues before moving to the next task.
3. Mark the plan task complete.
4. Update the changes file with Added, Modified, or Removed entries.
5. Continue to the next unchecked task.

Continue until all tasks are complete and the plan's success criteria are verified.

### 6. Completion and Documentation

**Implementation is complete when:**
- All plan tasks are complete.
- All required files are updated with working code.
- All success criteria are verified.
- The changes file accurately records the work.

**Final step - update changes file with release summary:**
- Add a release summary only after all phases are complete.
- Document the final file inventory and notable deviations.

### 7. Problem Resolution

**When encountering implementation issues:**
- Document the problem clearly.
- Try alternative approaches grounded in workspace patterns.
- Continue when the intent is clear; record unresolved issues when you cannot complete them safely.

## Implementation Workflow

```
1. Read the complete plan file
2. Read the complete changes file
3. For each unchecked task:
   a. Read referenced research and target files
   b. Implement task with working code
   c. Validate the implementation
   d. Mark task complete in the plan
   e. Update the changes file
   f. Record any deviations and reasons
4. Repeat until all tasks complete
5. Add final release summary after all phases are complete
```

## Success Criteria

Implementation is complete when:
- ✅ All plan tasks are marked complete `[x]`
- ✅ All specified files contain working code
- ✅ Code follows workspace patterns and conventions
- ✅ All functionality works as expected within the project
- ✅ Changes file is updated after every task completion with Added, Modified, or Removed entries
- ✅ Changes file documents all phases with detailed release-ready documentation and final release summary

## Template Changes File

Use the following template for `.docs/changes/YYYYMMDD-task-description-changes.md`.

**IMPORTANT**: Update this file after EVERY task completion by appending to Added, Modified, or Removed sections.
**MANDATORY**: Always include the following at the top of the changes file: `<!-- markdownlint-disable-file -->`

<!-- <changes-template> -->
```markdown
<!-- markdownlint-disable-file -->
# Release Changes: {{task name}}

**Related Plan**: {{plan-file-name}}
**Implementation Date**: {{YYYY-MM-DD}}

## Summary

{{Brief description of the overall changes made for this release}}

## Changes

### Added

- {{relative-file-path}} - {{one sentence summary of what was implemented}}

### Modified

- {{relative-file-path}} - {{one sentence summary of what was changed}}

### Removed

- {{relative-file-path}} - {{one sentence summary of what was removed}}

## Release Summary

**Total Files Affected**: {{number}}

### Files Created ({{count}})

- {{file-path}} - {{purpose}}

### Files Modified ({{count}})

- {{file-path}} - {{changes-made}}

### Files Removed ({{count}})

- {{file-path}} - {{reason}}

### Dependencies & Infrastructure

- **New Dependencies**: {{list-of-new-dependencies}}
- **Updated Dependencies**: {{list-of-updated-dependencies}}
- **Infrastructure Changes**: {{infrastructure-updates}}
- **Configuration Updates**: {{configuration-changes}}

### Deployment Notes

{{Any specific deployment considerations or steps}}
```
<!-- </changes-template> -->
