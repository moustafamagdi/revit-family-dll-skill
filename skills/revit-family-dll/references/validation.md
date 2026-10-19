# Evidence and validation

## Offline checks

Compile against the actual release API with Copy Local disabled. Check assembly
version, command, embedded resources, and dependencies. Parse embedded data under
the target runtime when practical. Validate units, supported sizes, unique sorted
type names, positive dimensions, completeness, bounds, and impossible arrangements.

Useful geometry maths checks include closed planar profiles, nondegenerate
segments, thickness, bolt PCD/hole fit, analytic envelopes, and orthonormal
transforms with correct handedness. Scope checks to the product, without invented
engineering certification claims. Offline maths does not prove Revit creation.

## Inside the generator

For every type, select it in a transaction, regenerate, require commit, and inspect
actual forms/solid geometry. Assert meaningful tolerances, positive solids,
visibility, materials, and identities. Bounds can overestimate rotated geometry;
use analytic faces, vertices, or volume when bounds cannot establish the claim.

Flex advertised controls at defaults, useful extremes, and representative values.
Test invalid inputs where clamping/rejection is promised. Inspect geometry and
connectors, not just parameter values. Restore defaults/types before saving.

Save without overwriting, close, reopen, and recheck type inventory, geometry,
identities, associations, and connector definitions. If project behavior matters,
load into an owned scratch project; test placement, type switching on the same
instance, installation controls, and supported orientations. Close it unsaved.

On failure, log type/stage and return failure. A diagnostic RFA may remain but
must not get a success marker. Do not activate it in the user's project as a fix.

## Evidence levels

| Evidence | Permitted claim |
| --- | --- |
| Source review | Prepared; build/runtime unverified |
| Successful compiler output | Compiled for the named release |
| Offline checks | Only named inputs/calculations passed |
| User successful-run report | User-reported success for that build |
| Matching version/MVID runtime log | Named automated Revit checks passed |
| User visual/flex review | Only reviewed cases/behavior |
| Connected-network test | Only tested system behavior |

Runtime assertions in source are not evidence of executed Revit tests.

## Revisions

Inspect the matching Desktop log: version/MVID, Revit build, first failing stage,
type/source, transaction, failure GUID, and stack. Reproduce offline if practical;
fix the narrow cause and increment version. If a new log identifies an old
assembly, resolve stale loading before altering geometry again. Keep working
source/data and previous evidence intact.

## Maintainer review scenarios

These are acceptance scenarios, not executed Revit tests:

- Scanned source PDF: inspect drawings and label uncertain dimensions.
- Revit 2025 requested: adapt .NET/API target; do not rename a net48 DLL.
- User will run DLL: compile, include assertions, and mark runtime pending.
- DN changes but connector stays fixed: fail flex validation and fix association.
- Transaction rolls back or reopened associations disappear: report failure even
  when a saved RFA exists.
- Returned log contains private paths: use locally; exclude from public release.
