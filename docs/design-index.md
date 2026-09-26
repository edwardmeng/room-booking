# Room Booking Design Set

## Delivery rule

This repository uses **four separately reviewable design packages and one implementation**.

- Phase 1 is the only implementation target for this delivery.
- Phases 2-4 are design-only architecture proposals.
- No source code, project file, dependency manifest, migration, workflow, script, generated image, or runtime configuration may be created until this complete design set is explicitly approved.
- Approval of this index alone is not approval of the design set.

## Package status

| Package | Scope | Detail level | Delivery status | Implementation status |
|---|---|---|---|---|
| [Phase 1: Recruitment core](phases/phase-1-core/design.md) | Four small reservation endpoints, SQLite, stored time-overlap validation, tests | Implementation-grade | Approved | Implemented |
| [Phase 2: Recurrence and lifecycle](phases/phase-2-lifecycle/architecture.md) | Drafts, holds, recurring series, edit/cancel, ETags, idempotency, ownership | Architecture-level | Ready for review | Design only - not implemented |
| [Phase 3: Multi-terminal and check-in](phases/phase-3-multiterminal/architecture.md) | PC/mobile/display flows, check-in, no-show release, QR capability | Architecture-level | Ready for review | Design only - not implemented |
| [Phase 4: Production hardening](phases/phase-4-production/architecture.md) | Provider evolution, observability, cache, Outbox, HA and operations | Architecture-level | Ready for review | Design only - not implemented |

## Dependency map

```text
Phase 1 core API and schema
  -> Phase 2 versioned lifecycle and recurrence
    -> Phase 3 terminal-specific check-in experience
      -> Phase 4 production hardening and provider evolution
```

Separately reviewable does not mean dependency-free. Every later package summarizes the upstream contracts it consumes and identifies compatibility or migration consequences.

## Required files

### Phase 1

- [Design](phases/phase-1-core/design.md)
- [Data dictionary](phases/phase-1-core/data-dictionary.md)
- [Test plan](phases/phase-1-core/test-plan.md)
- [System context](phases/phase-1-core/diagrams/01-system-context.mmd)
- [Core API flow](phases/phase-1-core/diagrams/02-core-api-flow.mmd)
- [Persistence ER](phases/phase-1-core/diagrams/03-core-er.mmd)
- [Reservation creation sequence](phases/phase-1-core/diagrams/04-reservation-create-sequence.mmd)
- [Overall layered design](phases/phase-1-core/diagrams/05-overall-layered-design.mmd)
- [Layered class diagram](phases/phase-1-core/diagrams/06-layered-class-diagram.mmd)

### Phase 2

- [Architecture](phases/phase-2-lifecycle/architecture.md)
- [Data model](phases/phase-2-lifecycle/data-model.md)
- [Test strategy](phases/phase-2-lifecycle/test-strategy.md)
- Diagrams under `phases/phase-2-lifecycle/diagrams`

### Phase 3

- [Architecture](phases/phase-3-multiterminal/architecture.md)
- [Data model](phases/phase-3-multiterminal/data-model.md)
- [Test strategy](phases/phase-3-multiterminal/test-strategy.md)
- [UI specification](phases/phase-3-multiterminal/ui-spec.md)
- Diagrams and independently editable wireframes under `phases/phase-3-multiterminal/diagrams`

### Phase 4

- [Architecture](phases/phase-4-production/architecture.md)
- [Data model](phases/phase-4-production/data-model.md)
- [Test strategy](phases/phase-4-production/test-strategy.md)
- [Observability](phases/phase-4-production/observability.md)
- [Reliability](phases/phase-4-production/reliability.md)
- [Operations outline](phases/phase-4-production/operations-outline.md)
- Diagrams under `phases/phase-4-production/diagrams`

## Completion checklist

A package is complete when:

- every required file exists and every relative link resolves;
- goals, non-goals, prerequisites, key contracts, invariants, risks, tests, status, and exclusions are stated;
- prose, tables, and Mermaid sources agree;
- upstream dependencies and compatibility effects are explicit;
- no blocking design decision remains;
- design-only packages contain no executable artifact.

The overall set is complete when all four packages meet this checklist and their cross-phase evolution is consistent.

## Approval record

| Field | Value |
|---|---|
| Design-set revision | `review-22` |
| Prepared date | 2026-09-25 |
| Approval state | **Approved through explicit user confirmation and subsequent directed refinements** |
| Approval scope | All four packages as linked from this revision |
| Permission after approval | Implement and test Phase 1; keep Phases 2-4 design-only |
| Phases 2-4 | Design only - not implemented |

Approval must be an explicit user statement that identifies this complete four-package design set. Partial feedback, silence, or approval of one package does not open the implementation gate.

The user explicitly approved `review-21` on 2026-09-25. Revision `review-22` incorporates the user's subsequent directed Phase 1 refinements: separate API, Business Contracts, Business Services, and Data Access projects; Domain Models consolidated into the `RoomBooking.BusinessContracts` namespace; Client Models retained in the API; the parameterized reservation collection route; removal of room identity from reservation response/domain models; and the shortened reservation field names. A later explicit instruction authorized the concrete Phase 1 implementation and tests. Phases 2-4 remain design-only.

## Change control

- A Phase 1 contract change updates Phase 1 and every affected downstream package.
- A shared-contract change requires renewed overall approval.
- A Phase 2-4 internal clarification that does not affect Phase 1 remains documentation-only but still updates the design-set revision.
- Breaking changes must be labeled as compatibility exceptions; they must not be described as additive evolution.
