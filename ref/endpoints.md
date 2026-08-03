Service endpoints
================================

Reference for the service web routes. Two parts: the endpoints Mynatime
actually uses (confirmed from the client code and the captures in this
directory), and a broader map of navigation URLs collected for later.

All paths are **relative** to the service base URL — the same convention the
client uses (`CreateRequest(method, "presences/create/advanced")`, no leading
slash). The base URL is configured in one place in the client.

See [`NOTICE.md`](NOTICE.md) for how the HTML captures under this directory
are produced and sanitized.


Endpoints used by Mynatime
----------------------------------------------------------------

These are confirmed against the internal `WebClient` and the
sanitized captures in this directory. This is the full surface the client
touches.

### `GET /`

Homepage. Used as a session check (`GetHomepage`). A redirect / login page
instead of the dashboard is surfaced as the `LoggedOut` error, which triggers
an automatic re-login during `commit`.

### `GET presences/create/advanced`

Fetches the "advanced" activity-creation form (`GetNewActivityItemPage`).
Parsed for the request token, the current user id, and the list of available
activity categories. Capture: [`presences/create/advanced/default.html`](presences/create/advanced/default.html).

### `POST presences/create/advanced`

Submits a new activity (`PostNewActivityItemPage`). Request fields are named
`create[...]` (comment, start/end dates and times, activity/category, token) —
see the capture for the exact field set. The client overrides only
`create[comment]` at commit time to append the MNTM sync footer.

- **Success**: a redirect whose location starts with `/presences/create/`.
- **Business rejection**: the form is re-rendered with a validation message,
  surfaced as `InvalidForm/ValidationError`. One known case is an activity the
  account is not authorized to use ("Ce collaborateur n'est pas autorisé à
  utiliser cette activité."); `commit` converts such a rejected tracker
  activity into a standalone flagged item rather than corrupting the residual
  (see issue #30). Capture: [`presences/create/advanced/failed.html`](presences/create/advanced/failed.html).


Navigation URLs (reference, unverified)
----------------------------------------------------------------

Collected for later — **not** used by Mynatime and **not** verified against
the running service. These are navigation pages (interface URLs), not
a technical API map, gathered from support
articles. Treat as leads to confirm, not as contract. Paths are relative, as
above.

| Area | Purpose | Path |
|---|---|---|
| Human resources | Manage collaborators, profiles, roles, contracts | `human-resources/users` |
| Directory | Browse collaborators | `human-resources/users/directory` |
| Absences — submit | File an absence / leave | `utilisateur/absences/create` |
| Absences — search | Search / export absences (per rights) | `utilisateur/absences/search` |
| Presence — search | Search logged presences / activities | `presences/search` |
| Presence — activities | Search / export activities | `presences/tasks/search` |
| Hours — balances | Search / adjust hour balances | `utilisateur/overtimes/balances/search` |
| Hours — notifications | Configure hour notifications | `utilisateur/overtimes/notifications` |
| Expenses — dashboard | View / file expense reports | `utilisateur/expenses/tickets/dashboard` |
| Expenses — search | Search / edit expense tickets | `utilisateur/expenses/tickets/search` |
| Expenses — vehicles | Manage vehicles for mileage expenses | `utilisateur/expenses/vehicles` |
| Events — create | Add an event to the schedule | `utilisateur/events/create` |
| Events — search | Search / edit events | `utilisateur/events/search` |
| Documents | Add an HR document | `utilisateur/dms/create` |
| Interviews — dashboard | Team dashboards | `interviews/dashboard/team` |
| Interviews — search | Search interviews | `interviews/search` |
| Interviews — personal note | Create a personal note | `interviews/grades/create` |
| HR insights | HR indicators | `human-resources/insights/payroll` |
| HR insights — age pyramid | Age pyramid | `human-resources/insights/agePyramid` |
| Planning | Planning / calendar view | *not confirmed* |

