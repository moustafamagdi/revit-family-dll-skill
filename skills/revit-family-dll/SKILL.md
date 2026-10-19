---
name: revit-family-dll
description: Build compiled C# DLLs that generate source-based parametric Autodesk Revit families through Add-in Manager, with catalogue types, native geometry, MEP connectors where applicable, Desktop diagnostic logs, and runtime validation. Use for new or revised family generators from manufacturer PDFs, submittals, CAD, or reference RFAs; not for general Revit troubleshooting or direct model edits.
license: MIT
metadata:
  version: "1.0.0"
---

# Revit Family DLL Builder

Deliver a compiled `IExternalCommand` generator DLL that the user loads through
Add-in Manager in Manual Mode to create RFAs. Keep C# source and source notes
with the deliverable. Treat each product's source documents as authoritative.

## Establish the target

Use the requested Revit release and existing project conventions. If neither is
known, ask for the release while inspecting the supplied documents. The bundled
starter targets **Revit 2024 / .NET Framework 4.8 / x64**; this is an example target,
not a requirement for everyone. Read [build-and-run.md](references/build-and-run.md)
for compatibility, prerequisites, compilation, and Add-in Manager handoff.

Do not run a generator inside Revit, install an add-in, or edit the active model
unless the user authorizes that action. If the user reserves execution for
themselves, finish compilation and offline checks and hand over the DLL. An
unavailable Revit installation permits source preparation but limits build claims.

## Build from evidence

1. Inspect both rendered drawings and extracted tables. Record physical PDF page,
   printed page if different, units, size/configuration, and dimension meaning.
   Distinguish DN, OD, bore, centreline, invert, and overall envelope.
2. Keep a source/assumption record. Label estimated profiles, thicknesses,
   accessories, and internals. Resolve conflicting dimensions visibly; do not
   invent catalogue sizes or imply estimates are manufacturer data.
3. Choose one or several RFAs based on topology, category, hosting, and constraint
   reliability. Discrete catalogue geometry is valid if disclosed. Explain which
   parameters flex geometry, select sizes, or store information only.
4. Preserve original references and earlier builders. Embed required catalogue
   data in the DLL; avoid depending on its load directory or a private path.

## Implement

Read [family-patterns.md](references/family-patterns.md) when designing geometry,
parameters, materials, placement, or connectors. Prefer native forms and reliable
associations. An editable parameter alone is not parametric behavior.

Include an append-only UTF-8 log on the resolved Windows Desktop. Record assembly
version/MVID, Revit release/build, source, type, stage, transaction status,
failure IDs and element IDs, exception stacks, validation results, and saved
paths. Flush each stage so a crash leaves useful evidence. Explain logging
failures; do not claim a validated run when required logging failed.

Keep warnings in the log. Never suppress unknown failures to obtain success;
require committed transactions and fail on rollback. Use fresh output folders;
never overwrite an existing RFA without authorization.

For a new project, copy [assets/starter](assets/starter) into a working folder.
It contains a native box with a geometry-driven height, three synthetic types,
Desktop logging, and save/reopen checks. Replace geometry, data, identity, command,
and validations for the actual product. It has no MEP connectors and is not a
manufacturer family. Build it with [scripts/build.ps1](scripts/build.ps1); do not
edit installed skill assets in place.

## Verify and deliver

Read [validation.md](references/validation.md). Add runtime checks to the command:
regenerate, inspect actual geometry/connectors, flex important controls, restore
defaults, save, reopen, and recheck. Close owned temporary documents without
saving unrelated changes. Do not use the active project as an unauthorized test bed.

Compile and run meaningful offline data/geometry checks. Keep separate evidence
for compilation, offline checks, automated Revit checks, and user visual review.
A successful build is not a Revit test. New code does not inherit an earlier
version's runtime success.

Deliver in the user's language with a DLL link, exact namespace/class command,
target release, Add-in Manager Manual Mode steps, output behavior, Desktop log
location, editable/fixed dimensions, and honest test status. For a failure, match
version/MVID and the first failing stage before making a narrow fix. Normally
retain the DLL filename/path and increment its version; preserve earlier evidence.
