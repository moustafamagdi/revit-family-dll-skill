# Family implementation patterns

## Geometry and parameters

- Choose category and hosting/placement behavior before designing constraints.
  A valve, pump, tank, and duct unit need different decisions.
- Prefer native extrusions, revolutions, blends, sweeps, and voids when practical.
  Disclose editing/material/visibility limitations of imports or DirectShape.
- Internal lengths are feet (`mm / 304.8`). Use explicit conversions or
  `UnitUtils`; area, volume, pressure, and flow need their own specifications.
- Query parameters before adding `Manufacturer`, `Model`, etc. Check existing
  specification and type/instance scope. Do not set formula-driven values
  directly. Use invariant-culture numbers and explicit units in formulas.
- Associate form parameters or label stable reference dimensions. Extrusion
  start/end associations often avoid fragile axial sketch constraints. Use
  reference planes/dimensions for profiles that must flex. Radial constraints
  alone do not preserve a pipe's invert.
- Separate fixed catalogue dimensions, size selectors, installation controls,
  and informational fields. Disclose effective/clamped ranges. A text-only
  dimension is not geometry control.
- Per-size solids with type-driven visibility can represent discrete catalogue
  variants. Explain that arbitrary intermediate sizes are unsupported. Validate
  visibility and switching. Bounds including hidden catalogue solids do not
  establish the visible product envelope.
- Use practical subcategories, material associations, and detail controls. Keep
  service-clearance solids separate from physical envelope/volume checks.
- Persist component identities across save/reopen, e.g. using Extensible Storage
  with stable schemas/part keys. Do not rely on creation order, in-memory IDs, or
  a Comments parameter that may be absent on forms.

## MEP connectors

Create actual connectors when required; holes and pipe-like solids are not
connectors. Select domain, shape, classification, flow direction, and linking
from product semantics and the target SDK, not another product's defaults.

Place connectors at documented connection planes and associate size and carrier
geometry with the intended drivers. Distinguish nominal DN from actual bore/OD.
Keep stable inlet/outlet roles. Check count, size, origin, outward axes, domain,
and required properties after regeneration, type/control changes, and reopening.

Validate what a nested family's parent actually exposes in a scratch project.
Family-document `MEPModel` may not expose project connectors. Check stored
classification/direction on family `ConnectorElement` definitions; a deliberately
unconnected project connector's system-calculated properties may be undefined.
A standalone check does not prove connected-network performance. Run network
tests only when in scope and authorized.

Compare origins in local coordinates with the inverse instance transform, and
outward axes in project coordinates with the instance transform. Do not compare
rotated world coordinates to unrotated local expectations.

Hosting/category rules can reject out-of-plane rotation. Use supported methods;
if a workaround is required, verify full transform, handedness, instance count,
and connector pose. Do not impose one category's workaround on all families.

## Transactions, logs, and output

Use owned family/scratch documents. Never close or save the user's active
document as cleanup. Close temporary documents in `finally`; log cleanup failures
without hiding the original error.

Attach `IFailuresPreprocessor` to record severity, definition GUID, description,
failing IDs, and additional IDs. Log warnings before justified selective
suppression. Keep warnings otherwise. Roll back unknown errors and check
`Start`/`Commit` results: `RolledBack` and `Pending` are not success. Do not silently
skip failed geometry/types.

Resolve Desktop with `Environment.SpecialFolder.DesktopDirectory` to support
redirected/OneDrive Desktops. Append UTF-8 records and flush stages. Include
version/MVID, Revit build, sources, types, assumptions, transaction outcomes,
failures, exception `ToString()`, validation, and output paths. Do not transmit
logs externally unless asked. Review/redact paths before public sharing.

Honor a requested destination or let users choose output. Use unique folders and
`OverwriteExistingFile=false` by default. Failed validation means failure, even
if an RFA exists. Report multi-product partial results per product. Write success
markers only after the corresponding runtime checks actually pass.
