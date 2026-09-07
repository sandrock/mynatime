Local store: profile.json → SQLite
================================

Status: **draft / for review** — not yet approved. Author intent captured
2026-09-02. This is a design spec, not an implementation; the "Open decisions"
section lists what must be agreed before any code is written.


Goal
----------------------------------------------------------------

Replace the per-profile `profile.json` file store with a single local SQLite
database. The current store works but is a flat JSON blob; a relational store
is the foundation for new features. Two things drive the change:

1. **Copy presence activity data for local use** — pull the user's logged
   presences/activities from the service into a local table so they can be
   listed, searched, and (later) consulted **offline**.
2. **Multi-user readiness (groundwork only)** — every row carries a `user_id`
   (and `tenant_id`) so a future multi-user mode is cheap. For now the app is
   strictly **single-user**: it uses the one `user` row present, and **fails
   fast if more than one user row exists** (or if none does). There is **no CLI
   option to select a user** at this stage.

Non-goals for the first iteration: changing the CLI's staging/commit UX, going
online-first, or building the offline reader UI. Those come after the store
exists.


Guiding principles
----------------------------------------------------------------

- **Mirror the server's shape.** The Manatime service runs one database for
  many customers. Replicating that multi-tenant schema locally is explicitly
  acceptable and preferred over inventing a divergent model.
- **Every row is owned and tenanted.** Every domain row carries `user_id` and
  `tenant_id`. One user maps to exactly one tenant (confirmed); tenant is the
  Manatime company, surfaced today as `GroupId`.
- **Offline-first stays.** The DB is the source of truth locally; the network
  is a sync channel, never a read dependency.
- **JSON columns where they earn their keep.** The schema is not fully
  normalised. Opaque or rarely-queried data (session cookies, change-item
  payloads, nested identity sub-objects, role lists) is stored as a JSON text
  column to keep the schema small. The rule: **anything we index or search gets
  real columns; everything else may be a JSON blob.** The presence copy is the
  case that must be columnar; cookies and identity sub-objects are the cases
  that stay blobs.
- **One store drives ops at a time.** The app never has both stores driving
  operations. It ships in two phases (below): first JSON runs ops while the DB
  is built *only* to validate the migration, then a one-way cutover moves ops
  onto the DB. No dual-write.


Current state (what we migrate from)
----------------------------------------------------------------

The store today is a set of `profile.*.json` files in
`~/.config/mynatime/` (`MynatimeConfiguration.ConfigDirectory`). Each file is
one online account, discovered by `MynatimeProfiles.DiscoverProfileFiles` and
loaded via `MynatimeProfile.LoadFromFile`; `-p/--profile` picks one and
`IsDefault == true` marks the active fallback. This migration folds those files
into rows of one shared DB. In the new model there is **one** usable `user` row
and no CLI user selector (see Goal 2); a legacy directory holding several
profiles will therefore migrate into several `user` rows and then require the
user to keep just one. A separate dev/test profile is better handled by pointing
at a different DB file than by a user selector.

A profile file (`MynatimeProfile`, a Newtonsoft `JObject` wrapper) holds:

| JSON path | Meaning | Target below |
|---|---|---|
| `__manifest`, `DateCreated`, `UniqueId` | file identity | `user` / drop |
| `LoginUsername`, `LoginPassword` | credentials | `user` |
| `Cookies[]` (`Domain`, `Name`, `Path`, `Value`, `Expires`, `HttpOnly`) | session | `user.cookies_json` |
| `UserId`, `Identity{email,name,firstName,lastName,gender,birthday,address,company,createdAt,roles[]}` | who | `user` |
| `GroupId`, `Group{}` | customer/company | `tenant` |
| `IsDefault`, `IsEnabled`, `ConfirmLocalSave`, `ConfirmServiceSave` | flags | `user` |
| `Data.ActivityCategories[]` (`Id`, `Name`, `Alias`, `Created`, `LastUpdated`, `Deleted`) | activity catalogue | `activity_category` |
| `Transaction.Items[]` | staged local changes | `change_item` (staged) |
| `Commits.Items[]` | committed changes | `change_item` (committed) |

A transaction item (`MynatimeProfileTransactionItem`) has `TimeCreatedUtc`,
`TimeCommittedUtc`, `ObjectType`, `CommitId`, `CommitItemId`, plus a typed
payload. **Two** typed payloads exist today (both self-register with
`MynatimeProfileTransactionManager`):

- `MynatimeCLI.ActivityStartStop` — an array of events `{Time, Mode, CategoryId,
  Comment}`.
- `Mynatime.Client.NewActivityItemPage` — a serialized web form (`FormData`)
  plus an optional `SaveFailedReason` (a server-rejected item kept standalone).

There is **no** presence/activity history stored locally today. That is net-new
(feature 1).


Architecture: code layers (target)
----------------------------------------------------------------

The current project layering is inside-out and gets straightened as part of this
work. Verified from the `.csproj` references and source:

- `Mynatime.Client.csproj` → `Mynatime.Infrastructure` (the service client
  depends on the local store), **and** `Infrastructure/ProfileTransaction/
  ActivityStartStop.cs` reaches back into the `Mynatime.Client` namespace for
  `ClientConstants` — a bidirectional source coupling.
- `Mynatime.Domain` depends on `Mynatime.Client` only to borrow
  `NewActivityItemPage` as a data type (it makes no HTTP calls).
- The shared model (`ActivityStartStop`, `ITransactionItem`,
  `MynatimeProfileData`) lives in *Infrastructure*, so the "infrastructure"
  project is really the domain kernel and "Domain" is an edge app-service.

Root cause: `NewActivityItemPage` is a god-type — a service DTO (wraps an HTML
`WebForm`), a transaction item (`ITransactionItem`), and the de-facto domain
model, all at once.

Target — a one-way dependency graph (no cycles), service SDK as a standalone
leaf:

```
   Mynatime.ServiceClient   ← standalone SDK, NO internal deps (leaf)
        ▲                       IManatimeWebClient, DTOs, HTTP/HTML, ClientConstants
        │
   Mynatime.Domain          ← references ServiceClient; reuses its DTOs
        ▲                       entities, operations, ChangeItem/txn model, store PORTS
        │
   Mynatime.Persistence     ← references Domain; SQLite (Dapper+DBUp) + JSON store (P1)
        ▲                       implements the store ports, config/paths
        │
   Mynatime.CLI / Mynatime.GUI  ← reference Domain + Persistence + ServiceClient
```

**Two renames:** `Client → ServiceClient` (disambiguation) and `Infrastructure
→ Persistence`. The second is justified by moving `Infrastructure`'s shared
vocabulary — constants (`MynatimeConstants`), resources — **into Domain**; what
remains (repository impls, the Phase-1 JSON store, config/paths) is purely
persistence, and SQLite is just the current engine (so *not* `Mynatime.Sqlite`,
which would age badly). `Domain` keeps its name. Note `Persistence` now depends
on `Domain` (today `Infrastructure` was the base with no deps) — the
conventional clean-architecture arrangement. GUI is abandoned; if resumed it
references Domain too (see the GUI note below).

**Preserve history:** do every rename/move with `git mv`, in move-only commits
kept separate from content edits, so git's rename detection stays clean and
`git log --follow` keeps working across the reshape.

Key decisions:
- **ServiceClient is a standalone SDK** consumable with no extra dependencies.
  This requires **dropping `ITransactionItem` from `NewActivityItemPage`** (it
  becomes a pure DTO) and **moving the transaction model into Domain**
  (`ITransactionItem`, `MynatimeProfileTransactionItem`, `ActivityStartStop`
  → a Domain `ChangeItem`). That removes the backwards `Infrastructure → Client`
  reference; `Domain → ServiceClient` then makes `ClientConstants` usage
  legitimate.
- **Domain may reference ServiceClient** and reuse its DTOs directly, rather than
  a separate mapping/anti-corruption layer — a deliberate trade to avoid mapping
  friction. Consequence: a Manatime API change to those DTO shapes can touch
  Domain. Accepted for an app this size. Storage entities (`User`, `Tenant`,
  `ActivityCategory`, `PresenceActivity`) stay Domain-owned POCOs, so mapping
  exists only where the DB needs it — not doubled for service calls.
- **Persistence implements Domain's store ports.** The Phase 1 → Phase 2 flip
  (JSON store → SQLite store) is a DI registration change behind those ports,
  not a rewrite of callers.

### Read model vs change model (and the future GUI)

The two features want two storage strategies, and that is deliberate — a
CQRS-ish split:

- **`presence_activity` is the columnar READ model** — the committed activity
  copied from the service, indexed and range-queryable. This is what a time-axis
  view renders efficiently.
- **Change items are the flexible JSON WRITE model** — staged edits, schema-free
  and easy to evolve (activity tracking is the "beast"; JSON keeps it malleable).

A future GUI is planned where users see/draw activity by dragging delimiters on a
time axis. That timeline = **render committed `presence_activity`, overlaid with
pending JSON changes**; a drag produces/updates a change item, which on commit
syncs to the service and refreshes the `presence_activity` cache. The merge
("committed rows + pending changes → one timeline") is a **Domain**
responsibility reused by both CLI and GUI — the GUI stays a renderer. Note for
when that work starts: give `presence_activity` sortable/absolute start/end
values for axis math (today they are local wall-clock text).


Proposed schema
----------------------------------------------------------------

One SQLite file, default `~/.config/mynatime/mynatime.db`. All tables carry
`tenant_id`; all user-owned tables also carry `user_id`. DDL below is a sketch
to review, not final — exact types/constraints settle once the data-access
approach (decision 1) is chosen.

### Conventions

- `id` — local autoincrement primary key, stable across syncs.
- `server_id` — the service's id for the row, nullable until the row has been
  synced/created upstream. Local rows can exist before the server knows them.
- `public_id` (forward-looking, not added to the sketches yet) — when server
  mirroring lands (see "Future direction"), locally-originating rows will need a
  globally-unique id (GUID/ULID) alongside the local PK so they merge into a
  multi-user server without collisions. Profiles already have `UniqueId` as the
  template; extend the pattern to presence edits and change items then.
- Timestamps stored as ISO-8601 UTC text (`*_utc`) unless noted; local
  wall-clock values that the service treats as local keep a `_local` suffix.
- Soft-delete via a nullable `deleted_utc`, matching the current category model.

### Core identity, credentials & session

Credentials and cookies live **on the `user` row** (no separate tables): one
user has one set of each, and cookies are never queried field-by-field, so they
ride along as a JSON blob.

```sql
CREATE TABLE schema_version (
    version     INTEGER NOT NULL,
    applied_utc TEXT    NOT NULL
);

-- The Manatime customer/company. This IS the tenant, so no tenant_id column.
CREATE TABLE tenant (
    id              INTEGER PRIMARY KEY,
    server_group_id TEXT,              -- profile.GroupId
    name            TEXT,
    created_utc     TEXT,
    last_synced_utc TEXT
);

CREATE TABLE user (
    id                  INTEGER PRIMARY KEY,
    tenant_id           INTEGER NOT NULL REFERENCES tenant(id),
    server_user_id      TEXT,          -- profile.UserId
    unique_id           TEXT,          -- profile.UniqueId
    email               TEXT,
    display_name        TEXT,
    first_name          TEXT,
    last_name           TEXT,
    gender              TEXT,
    birthday            TEXT,
    address_json        TEXT,          -- Identity.address {street,postalCode,city,country}
    company_json        TEXT,          -- Identity.company {id,name,employee_count,plan}
    roles_json          TEXT,          -- Identity.roles[]
    -- credentials (secrets stay plaintext for now; see below)
    login_username      TEXT,
    login_password      TEXT,
    password_storage    TEXT,          -- 'plaintext' today; future: 'dpapi', 'keychain', ...
    cookies_json        TEXT,          -- Cookies[] verbatim
    -- flags
    confirm_local_save   INTEGER,
    confirm_service_save INTEGER,
    is_default          INTEGER,       -- active profile
    is_enabled          INTEGER,
    created_utc         TEXT,
    last_synced_utc     TEXT
);
```

Secrets remain **plaintext**, unchanged from today. The `password_storage`
column records *how* the password is stored so a stronger scheme (OS keystore,
etc.) can be introduced later without a schema change — old rows read as
`plaintext`, new rows can declare a different codec.

### Activity categories (with the sub-category evolution)

Confirmed from a real profile (`profile.recovered.json`, 117 categories). The
service changed over time:

- **Plain (old) categories** — the id is a bare integer string, e.g. `9561`,
  `10369`. Flat, no parent.
- **Composite (new) categories** — the id is `PARENT***CHILD`, a literal `***`
  separator, e.g. `15753***1634`, `16229***1635`, `16476***1636`.
  - **Left** (`15753`, `16229`, `16476`, `16967`) is the **project** id.
  - **Right** is a **shared sub-activity taxonomy** reused across projects:
    `1634`=ASR, `1635`=NLP&Sémantique, `1636`=Intégration, `1653`=Pilotage,
    `1654`=Charges transverses. The same right-hand ids recur under every
    project.
  - The **project id never appears as its own category row** — only the
    composites do. A self-referential `parent_id` FK would therefore dangle, so
    we do **not** use one. The parent is a project reference, not a selectable
    category.

Detection is by the `***` in the **id**, not the name — the ` / ` in names like
`Project-COVA / ASR` is a display convention only (plain rows such as
`Avant-vente / BCA` also use ` / `). We keep a **flat** table and store the id
parts as strings:

```sql
CREATE TABLE activity_category (
    id               INTEGER PRIMARY KEY,
    tenant_id        INTEGER NOT NULL REFERENCES tenant(id),
    user_id          INTEGER NOT NULL REFERENCES user(id),
    server_id        TEXT,             -- full id: '9561' (plain) or '15753***1634' (composite)
    parent_server_id TEXT,             -- left of '***' (project id); NULL when plain
    sub_server_id    TEXT,             -- right of '***' (shared sub-activity id); NULL when plain
    name             TEXT,
    alias            TEXT,
    created_utc      TEXT,
    last_updated_utc TEXT,             -- per-item; collection-level LastUpdated → sync bookkeeping
    deleted_utc      TEXT
);
```

Possible later refinement: a small lookup table mapping the shared sub-activity
id (`1634` → "ASR", …) so composites can be grouped by phase across projects.
Not needed for the first cut.

### Presence activity (the feature — columnar for search)

Net-new. Fully columnar on purpose: this is the table we index and search, so no
JSON blob.

```sql
CREATE TABLE presence_activity (
    id                   INTEGER PRIMARY KEY,
    tenant_id            INTEGER NOT NULL REFERENCES tenant(id),
    user_id              INTEGER NOT NULL REFERENCES user(id),
    server_id            TEXT,         -- presence id on the service
    activity_category_id INTEGER REFERENCES activity_category(id),
    date_start_local     TEXT,
    date_end_local       TEXT,
    in_at_local          TEXT,         -- time-of-day
    out_at_local         TEXT,
    duration_minutes     INTEGER,
    comment              TEXT,
    source               TEXT,         -- 'server' | 'local'
    is_dirty             INTEGER,      -- local edit not yet pushed
    created_utc          TEXT,
    last_synced_utc      TEXT
);

CREATE INDEX ix_presence_user_date  ON presence_activity(user_id, date_start_local);
CREATE INDEX ix_presence_category   ON presence_activity(activity_category_id);
CREATE UNIQUE INDEX ux_presence_server ON presence_activity(user_id, server_id);
-- Free-text search over comments (SQLite FTS5) is a candidate, deferred until
-- the reader command is designed.
```

### Staging (git-style stage/commit)

Preserves today's `Transaction` (staged) / `Commits` (committed) split. The
`change_item` table below is the **container** (metadata common to every change
type). *Resolved (decision 4):* the typed body stays a **JSON blob** — activity
tracking evolves often and benefits from a schema-free representation. Each event
inside the payload carries a **stable id** so the future GUI can edit/delete a
single delimiter by id without needing SQL rows.

```sql
CREATE TABLE change_item (
    id                 INTEGER PRIMARY KEY,
    tenant_id          INTEGER NOT NULL REFERENCES tenant(id),
    user_id            INTEGER NOT NULL REFERENCES user(id),
    object_type        TEXT,           -- 'MynatimeCLI.ActivityStartStop' | 'Mynatime.Client.NewActivityItemPage'
    state              TEXT,           -- 'staged' | 'committed'
    payload_json       TEXT,           -- typed body, serialized as-is (blob variant)
    time_created_utc   TEXT,
    time_committed_utc TEXT,
    commit_id          INTEGER,
    commit_item_id     INTEGER
);
```

### Sync bookkeeping (shape TBD)

Sync is **not** a full pull of everything. The intent is **table-centered,
date-range-focused, on-demand** syncs (e.g. "fetch my presences for March"). A
natural fit is a record of which ranges have been fetched per entity, so the app
knows what it already has:

```sql
-- Tentative — sync design is not settled (decision 5).
CREATE TABLE sync_range (
    id           INTEGER PRIMARY KEY,
    tenant_id    INTEGER NOT NULL REFERENCES tenant(id),
    user_id      INTEGER NOT NULL REFERENCES user(id),
    entity_type  TEXT,                 -- 'presence_activity', 'activity_category', ...
    range_start  TEXT,                 -- inclusive (date)
    range_end    TEXT,                 -- exclusive (date)
    fetched_utc  TEXT
);
```

This table is a placeholder to make the idea concrete, not a committed design.


Migration & rollout: two phases
----------------------------------------------------------------

The DB arrives in two shipped phases. At no released point do JSON and the DB
both drive operations — the first phase writes the DB purely to *validate* the
migration; the second flips ops onto the DB for good.

### Phase 1 — shadow migration, JSON still runs ops

- All app operations continue to read and write `profile.*.json`, unchanged.
- A new `migrate` command builds the DB from the JSON as a **dry run**: it
  creates the schema (Dapper + DBUp), imports every profile (each → a `tenant`
  deduped by `server_group_id`, a `user`, its categories and change items), and
  writes a short **migration report**, then stops. It does **not** touch the
  JSON, and the app does **not** read the DB for anything.
- The command is **repeatable**: each run rebuilds the DB from scratch, so users
  re-run it after each release and re-check the report.
- Users are explicitly invited to run it and eyeball the report. This is how we
  gain confidence the mapping is correct on real data before anyone depends on
  it.

The report is deliberately tiny — enough to confirm "it worked". Written to a
file (e.g. `~/.config/mynatime/migration-report.md`) and echoed to the console:

- one line per source profile: username, tenant, and counts (categories, staged
  changes, committed changes);
- totals, plus the DB path and schema version;
- **warnings** — anything that didn't map cleanly: an unknown `ObjectType`, a
  category id that parsed as neither plain nor `PARENT***CHILD`, a duplicate
  tenant, and crucially **"N user rows — keep exactly one before Phase 2"** when
  a directory holds several profiles.

### Phase 2 — cutover, DB only, no way back

- `migrate` becomes the real, **one-way** cutover: it (re)builds the DB, then
  **disables the JSON** (rename `profile.*.json` → `*.json.migrated`, kept as a
  backup, not deleted) and records that the cutover happened.
- From then on **all** operations use the DB exclusively; there is no JSON read
  path and no reverse migration.
- If the app starts with legacy JSON still active and no cutover recorded, it
  refuses normal commands and points the user at `migrate` (message wording —
  decision 6). After cutover it runs normally against the DB. `IsDefault` →
  `user.is_default`.
- The single-user rule applies at cutover: if migration produced more than one
  `user` row, the cutover refuses until the user has reduced to one (the Phase 1
  report warned them).


Concurrency
----------------------------------------------------------------

Commands are short. A second command that starts while another holds the write
lock should simply **wait** for the DB to be released, not fail. That means:
set a **`busy_timeout`** (e.g. 5s) on every connection. The default rollback
journal is fine — **WAL is not needed** (WAL mainly helps readers run
concurrently with a writer, which we don't require here). One small caveat to
note: a truly hung command could make another wait out the timeout; 5s is
comfortably above any real command's duration.


Native SQLite library & packaging
----------------------------------------------------------------

The only native dependency SQLite adds is `e_sqlite3` (the SQLite C library),
delivered via **SQLitePCLRaw**. This ships cleanly given how Mynatime already
publishes.

Today's release (`release.yml`) runs `dotnet publish -r linux-x64
--no-self-contained` and `-r win-x64 --no-self-contained` — **framework-
dependent but RID-specific**, two assets, with `install.sh` installing the .NET
runtime and fetching the matching asset. The current output is pure managed
DLLs.

With **Dapper + `Microsoft.Data.Sqlite`**, `Microsoft.Data.Sqlite` transitively
pulls `SQLitePCLRaw.bundle_e_sqlite3`, which carries the native `e_sqlite3` for
each RID. Because the publish is **RID-specific**, the correct native file is
flattened into the publish folder automatically:

- Linux tarball gains `libe_sqlite3.so`; Windows zip gains `e_sqlite3.dll`.
- No packaging changes, no per-platform handling. Framework-dependent is fine —
  third-party native libs always ship app-local; only the managed BCL comes
  from the shared runtime.
- Cost: ~1.5–2 MB per asset. New RIDs (`osx-x64`, `arm64`) get their natives the
  same way.

Avoid the old `System.Data.SQLite` provider (mixed-mode `SQLite.Interop.dll`,
fights RID-specific publish). If at-rest encryption is ever wanted, swap the
bundle to `SQLitePCLRaw.bundle_e_sqlcipher` — same shipping mechanism, different
native lib — which dovetails with the `password_storage` hook.


Future direction: a Mynatime server
----------------------------------------------------------------

A hosted **Mynatime server** may come later — mirroring many users' data to offer
advanced service usage, automated tasks, reports, and perhaps a personal web
tracking page. No server work is scheduled; this section only records how the
schema keeps that door open, and it validates decisions already made.

Why the current design already fits:
- **Multi-tenant, every row user-owned** — `tenant_id` and `user_id` everywhere.
  The local DB is essentially a single-user slice of what a server holds for
  many; the server just drops the "single-user, fail if many" guard.
- **Server-shaped schema** — surrogate `id` + `server_id`, soft-deletes, sync
  bookkeeping. Replicating the service's multi-customer shape was accepted from
  the start, so lifting the schema to a server RDBMS is a small step.
- **`presence_activity` is already a reporting/timeline read model** — the same
  columnar shape a server would render for a personal web tracking page (and the
  Domain timeline-merge logic is reusable server-side).

Concrete nudges to keep it cheap later (not implemented now):
1. **Globally-unique ids on locally-originating rows** (`public_id`, GUID/ULID)
   so they push/merge into a multi-user server without PK collisions — profiles
   already do this with `UniqueId`; extend to presence edits and change items.
2. **Keep DDL reasonably portable** — avoid SQLite-only idioms so the schema can
   move to a server RDBMS (Postgres/MySQL) later.

If a second sync target (the Mynatime server) is ever added, sync bookkeeping
would grow a "peer" dimension then — deliberately **not** modelled now (one peer
today; YAGNI).


Open decisions (need answers before coding)
----------------------------------------------------------------

Reply by number. Several earlier questions are now **resolved** and recorded at
the end.

1. **Data-access approach.** PetaPoco (used before) is a micro-ORM — fine for
   mapping, but it does **not** do schema migrations, so that is a separate
   choice (decision 2). Options:
   1. **PetaPoco** (or Dapper — very similar) for data access. Familiar, thin,
      hand-written SQL. Pairs with a separate migration tool.
   2. **Microsoft.Data.Sqlite** raw ADO.NET — no mapper, most explicit, fewest
      dependencies.
   3. **EF Core** (`Microsoft.EntityFrameworkCore.Sqlite`) — mapper **and**
      migrations in one package, LINQ, change-tracking; heavier, more magic.

   *Resolved: **Dapper** (option 1) + `Microsoft.Data.Sqlite`; EF Core ruled
   out.* It is the current, actively
   maintained micro-ORM that feels like PetaPoco — you keep writing SQL, which
   suits "replicate the server schema" and the JSON-column approach, with a tiny
   dependency footprint that fits a CLI. `Microsoft.Data.Sqlite` is the ADO.NET
   provider Dapper sits on, so it comes along regardless. Choose **EF Core**
   instead only if you'd rather have the ORM own change-tracking and generate
   migrations and are happy replacing SQL with LINQ — more productive for
   CRUD-heavy features, but more to (re)learn and heavier at runtime. PetaPoco
   still works but the ecosystem moved to Dapper. See the chat for the fuller
   comparison.

2. **Schema definition & upgrades.** How we create the schema and evolve it:
   1. **Hand-rolled runner** — ordered `NNNN_*.sql` scripts as embedded
      resources, a `schema_version` table, each applied once in a transaction.
      Tiny, works with any of the option-1 libraries. *(Recommended with
      PetaPoco/Dapper/Microsoft.Data.Sqlite.)*
   2. **DBUp** — a small library that does exactly the above (runs SQL scripts,
      tracks which ran). Less code to own than hand-rolling.
   3. **EF Core Migrations** — only if we pick EF Core in decision 1; C#-defined
      migrations with tooling.

   *Resolved: **DBUp** (option 2)* — versioned SQL scripts, dead simple, nothing
   to hand-maintain, pairs with Dapper.

3. **Activity-category composite id.** *Resolved by inspection* — composites are
   `PARENT***CHILD` (literal `***`); store `server_id` whole and split into
   `parent_server_id` / `sub_server_id`; no self-FK (project ids aren't rows).
   See the Activity categories section. (Kept for the record.)

4. **Activity-transaction representation.** *Resolved:* **JSON blob** in
   `change_item.payload_json` — activity tracking evolves often and JSON keeps it
   malleable. Each event carries a **stable id** in the payload so single-event
   edit/delete (and the future GUI drag) works without SQL rows. Normalised event
   tables were considered and rejected as premature. (Kept for the record.)

5. **Sync model.** Confirm the "on-demand, table-centered, date-range" direction
   and whether the `sync_range` bookkeeping table (or something else) is how we
   track what's been fetched. Can stay open until we build the first sync
   (presence pull).

6. **Migrate command name & wording.** *Resolved:* the command is **`mynatime
   migrate`** — it converts the *local data store* (JSON → DB). It is distinct
   from the internal DBUp *schema* migrations, which run automatically and are
   never invoked by the user. `upgrade` was rejected (reads as "update the app
   binary"). Wording:
   - **Phase 1** (non-blocking hint, JSON still authoritative): *"A local
     database preview is available. Run `mynatime migrate` to build it and
     review the report — your data keeps working unchanged."*
   - **Phase 2** (blocking, before cutover): *"Your local data needs a one-time
     upgrade before continuing. Run `mynatime migrate` (a backup of your current
     files is kept)."*
   - **After cutover**: *"Migration complete. N profile(s) upgraded; previous
     files kept as `*.json.migrated`. Report: `~/.config/mynatime/
     migration-report.md`."*
   - **Multi-user block** (at cutover, >1 user row): *"Migration found N
     profiles; this version supports one. Keep a single profile and re-run
     `mynatime migrate`."*

7. **Presence data source.** Which endpoint feeds `presence_activity`? The
   navigation map lists `presences/search` / `presences/tasks/search`
   (unverified); a capture + parser is prerequisite work for the presence pull.

### Resolved (recorded)

- **Re-layer to a one-way (acyclic) dependency graph.** Renames: `Client →
  ServiceClient` and `Infrastructure → Persistence` (its shared constants/
  resources move to Domain; `Domain` keeps its name). `Persistence` now depends
  on `Domain`. ServiceClient becomes a standalone SDK (`NewActivityItemPage` →
  pure DTO, transaction model moves to Domain); Domain may reference
  ServiceClient; Persistence implements Domain store ports. All moves via
  `git mv` in move-only commits.
- **Activity tracking stays JSON** (change items are a blob with stable per-event
  ids); `presence_activity` is the columnar read model. Split serves the future
  time-axis GUI (render committed rows + overlay pending changes, merged in
  Domain).
- **Data access = Dapper + `Microsoft.Data.Sqlite`; migrations = DBUp.** EF Core
  ruled out. Native `e_sqlite3` ships automatically via the RID-specific publish.
- **Two-phase rollout.** Phase 1: JSON runs ops, `migrate` is a repeatable dry
  run that builds the DB and writes a report. Phase 2: `migrate` is a one-way
  cutover that disables the JSON and moves ops to the DB.
- **One DB**, at `~/.config/mynatime/mynatime.db`. Every row is
  user/tenant-owned.
- **Single-user for now.** The app uses the one `user` row and fails fast if
  several exist; no CLI user selector. Multi-user is groundwork only.
- **Tenant = `GroupId`.** One user ↔ one tenant.
- **Category ids** are plain int or `PARENT***CHILD`; stored whole + split into
  `parent_server_id`/`sub_server_id`, flat table, no self-FK.
- **No dual-write.** Single operating mode; a dedicated `migrate` command does a
  one-shot conversion, legacy JSON kept as `*.json.migrated` backup.
- **Passwords stay plaintext**, with a `password_storage` column to allow a
  better scheme later without a schema change.
- **Credentials & cookies on the `user` row** (cookies as `cookies_json`); no
  separate `account_credential` / `session_cookie` tables.
- **Concurrency via `busy_timeout`**, no WAL.


Test strategy (to agree alongside the above)
----------------------------------------------------------------

- Unit tests for the `migrate` mapping — each `profile.*.json` field lands in
  the right column — using the existing recovered profile fixtures.
- Migration-runner tests: fresh DB, and upgrade across `schema_version` steps.
- Round-trip tests for `change_item` staging, equivalent to today's transaction
  tests (whichever representation decision 4 picks).
- Category tests covering both the old plain-int id and the newer composite
  (sub-category) id.
- Manual: run `migrate` on a real profile, verify categories/credentials/
  cookies, then run an existing command against the DB-backed store.
