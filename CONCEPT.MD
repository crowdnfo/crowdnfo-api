# crowdNFO: Concept

> Functional concept for the **crowdnfo-api** rewrite. This describes *what* the product
> does (domain, roles, features, workflows), not *how* it is built. It supersedes the
> original `README.md` and reflects how crowdNFO has evolved since.

---

## 1. Overview

crowdNFO is a **crowdsourced database of release metadata** for scene/warez releases.
For any given release, users contribute the submissions that describe it (**NFO files**,
**MediaInfo** data, and **file lists**), and the platform keeps the best contributed
version visible to everyone.

On top of this crowdsourced layer, crowdNFO automatically **enriches releases with product
metadata**: it recognises which movie or TV series a release belongs to and attaches rich
information (titles, year, genres, ratings, seasons/episodes, etc.) pulled from external
sources. A background processing pipeline does this work without user intervention.

The platform is consumed both by **humans** (web frontend) and by **automated client tools**
(e.g. download/automation clients) that submit and read data via API.

**Core building blocks:**
- **Users** with roles and a trust weight.
- **Releases**, each with a canonical name, optional group, category, aliases and variants.
- **Submissions** of three kinds (NFO, MediaInfo, file list) attached to releases.
- **Products** (movies, TV series, Games, ...) linked to releases. Movies and TV series are
  enriched with metadata from external sources; other types are categories only for now.
- An **automated processing pipeline** that analyses, parses, and enriches everything.

---

## 2. Users, Roles & Trust

### Registration & activation
- New users apply through an **application form**. Applications are reviewed by admins.
- Accounts must be **activated** before they can be used.
- Users can optionally enable **two-factor authentication (2FA)** for their account.

### Roles
Roles are data-driven. Each role carries a **trust weight** (used purely as a ranking signal,
see §4) and a flag for admin-panel access.

| Role        | Trust weight | Admin access | Notes                                   |
|-------------|-------------:|:------------:|-----------------------------------------|
| Admin       | 100          | yes          | Full access to the admin panel          |
| TrustedBot  | 100          | no           | Internal automation; no interactive login |
| VIP         | 50           | no           |                                         |
| Trusted     | 40           | no           |                                         |
| **User**    | 15           | no           | **Default role for new members**        |

> The original concept used the role "Member"; the default role is now **User**. Trust
> weights are configurable; the values above are the current defaults.

### Profile visibility
Each user chooses who may see their identity. This rule is applied **everywhere a username
would otherwise appear** (highscore, submission credits, comments, profiles, API responses),
not in any single place. When a name may not be shown it is **masked** with a placeholder.

- **Public**: the username is always shown.
- **MembersOnly**: shown to logged-in members; masked for anonymous visitors.
- **Private**: masked for everyone; only admins can see who it is.

---

## 3. Releases

A **release** is the central entity. Every submission and product link hangs off a release.

A release has:
- A **canonical (primary) name**: the cleaned/normalised name.
- **Aliases**: alternative names that resolve to the same release.
- A **category**: Movies, TV, Games, Software, Music, Books, Audiobooks, Other, Unknown.
- An optional **release group**.
- **Variants**: distinct physical files that circulate under the *same* release name (e.g.
  different encodes/sources of the same movie). Each variant is identified by the **SHA-256
  hash of its main payload file** (e.g. the `.mkv`), and a submission attaches to a specific
  variant by that hash. Each variant ranks independently by its own cumulative trust grade
  (§4); the top-ranked variant's hash is recorded as the release's **canonical file hash**.
- An optional link to a **product** (the movie/series it represents, see §5), set
  automatically by the pipeline once it can be determined.

Users can **comment** on releases.

---

## 4. Submissions & file ranking

### Submission types
Users (and bots) can make three kinds of submission to a release:
1. **NFO**: the release's `.nfo` text file.
2. **MediaInfo**: structured media metadata, including per-track audio and subtitle details.
3. **FileList**: the directory/file structure of the release.

The same content can be submitted by multiple users; each user's submission is credited to
it independently.

### Trust-based ranking
A release can hold several competing submissions of the same kind (e.g. two different NFOs).
crowdNFO ranks them by trust and shows the strongest:

- Each submission accumulates a **cumulative trust grade** = the sum of the **distinct**
  submitting users' role trust weights.
- The submission with the highest cumulative trust grade is displayed as the
  **canonical/default** version for the release. Ties are broken by submission count (more
  distinct contributors first), then by the most recent submission.
- Beyond ranking, the trust grade can be used as a filter on demand (e.g. to show only
  "trusted-enough" submissions).

### Moderation
Submissions have a simple moderation state, independent of any trust math:
- **Visible** (default).
- **Removed**: hidden by an admin or as the result of an upheld report; removed submissions
  are not served for download and do not count toward profile/highscore totals.

### Reporting
- Users can **report** a submission or a comment, with a comment explaining why.
- Admins review reports and act (e.g. remove the submission/comment).

---

## 5. Products & enhancement

A **product** is the work a release represents, currently **movies** and **TV series**.
Releases are linked to products so users can browse "all releases of this movie/series".

### Product model
- **Product** is the common base; concrete types are **Movie** and **TvSeries**.
- A **TvSeries** has **Seasons**, each with **Episodes**.
- Products carry rich metadata: titles, **alternative titles**, year, **genres**,
  **countries**, ratings, runtime/plot, **release dates**, external IDs (e.g. IMDB, TVDB),
  and series status (Running, Ended, Cancelled, Hiatus, Unknown).

> **Games, Software, Music, Books** are recognised as release categories and reserved as
> future product types, but are **not yet enriched** as products.

### Enrichment sources
Products are enriched automatically from external data:
- **IMDB**: search by title/year, batch lookups for basic metadata, and detailed lookups
  for full metadata (release dates, alternative titles, seasons/episodes).
- **MediathekArr**: maps an IMDB ID to a TVDB ID for TV series.

Enrichment is performed by the background pipeline (§6), which respects each source's rate
limits and handles failures/retries gracefully.

---

## 6. Automated processing pipeline

A **background pipeline** moves each release from "just submitted" to "fully enriched
product" without user action. It is **condition-driven**: each step declares when it is
*ready* to run and when it is *not applicable*, so steps fire only when their inputs exist.
The pipeline is **code-driven**: each step is a class with its own readiness logic, not a
runtime-configurable rule set; deploying new code is how the pipeline changes.

### Steps
- **NFO analysis**: parses the best NFO of a release to extract external IDs (e.g. IMDB ID)
  and metadata; links the release to a product when an ID is found.
- **Release-title analysis**: when there is no NFO, or the NFO yields no product ID, parses
  the **release name** itself to derive a title/year and create or find a product. This
  ensures releases with no usable NFO still become products.
- **LLM analysis**: uses an AI model to extract additional metadata and tags from NFO
  content (§7).
- **Product enhancement**: IMDB search / batch / detail and MediathekArr enrichment of the
  linked product (§5).

### Behaviour
- Processing triggers on **submission** (as soon as the relevant input exists); it does not
  wait for any acceptance threshold.
- Each release/product tracks **per-step status** (e.g. ready → in progress → completed /
  failed), timing, error messages, and retry counts.
- The pipeline always works from the **best** submission (top-ranked by trust grade), and
  re-runs a step if the best submission changes.
- Steps that call external APIs are **rate-limited** and retried with backoff.

---

## 7. LLM analysis & tags

> **Low priority** for the rewrite.

crowdNFO runs **AI analysis over the NFO's notes** to surface qualities that aren't
structured data. For a release's NFO it produces:
- whether the NFO contains **notes**,
- a brief **summary** of those notes,
- one or more **note categories** (tags), with a confidence score.

It also assigns **tags** to releases/submissions, from two origins:
- **From the release name** (pattern-based): `PROPER`, `READNFO`, `REPACK`, `INTERNAL`.
- **From LLM analysis of the NFO**: `HAS_NOTES`, `SCENE_DRAMA`, `GROUP_NEWS`, `TECHNICAL`.

LLM analysis is optional/toggleable and rate-limited, since it depends on an external model
provider.

---

## 8. Profiles, highscore & support

### Profiles
- Each user has a profile showing their **submission counts** (subject to visibility, §2).
- Users can change their **password** and **profile visibility**.

### Highscore
- A public highscore ranks the **top 100 contributors** by number of submissions.
- **Removed** submissions do not count.
- Usernames follow the global profile-visibility rules (§2), so some entries may be masked.

### Support tickets
- Members can open **support tickets** and track their status.

---

## 9. Admin

Admins (roles with admin access) manage the platform:
- **Users**: review and **activate** accounts, assign roles, and **lock/unlock** accounts
  (an admin-imposed block on signing in).
- **Applications**: review and decide on registration applications; manage the
  application form.
- **Releases & submissions**: edit, and **remove** (moderate) submissions.
- **Reports**: review reported submissions/comments and act.
- **Tickets**: handle support requests.
- **Processing & configuration**: observe the processing pipeline, **re-evaluate / backfill
  processing for existing releases/products** (e.g. after a new step is added), and **reset
  stuck processing steps**.

---

## 10. External clients & access

crowdNFO is used by automated client tools as well as the web frontend.

- **Authentication:** interactive users authenticate with short-lived tokens plus refresh
  tokens; automated clients authenticate with an **API key**.
- **Rate limiting:** sensitive endpoints (login, registration/application, token refresh,
  password change) are rate-limited per client.
- **Client versioning:** known client tools report their version, and the platform can gate
  on a **minimum supported version** to keep integrations compatible.

---

## 11. Canonical naming & release-group rulesets

> **Planned**, still needs fleshing out.

The same release often circulates under **different name formats**. A group might release as
`A.TV.Show.S01E02.GERMAN.DL-ZeroThree`, but a tracker re-formats it to
`A TV Show - S01E02 - GERMAN DL - ZeroThree` (dots swapped for spaces/dashes, words
re-spaced, etc.). These are all **aliases** of one release (§3), and crowdNFO must decide
which form is the **canonical name**.

Picking the highest-trust name is not always correct: a re-formatted alias can out-score the
group's true format. To handle this, each **release group** can carry a **ruleset** (e.g.
regex patterns) describing its correct naming convention. When a candidate name matches the
group's ruleset, that match **takes precedence over trust** when choosing the canonical name.

> Note: this decides the canonical *name*; the trust ranking in §4 decides the canonical
> *submission* (file). They are separate "canonical" choices.

> Still to be designed: the rule format, precedence between competing rules, per-group
> management (who creates/edits rulesets), and the fallback when no rule matches (where
> trust-based selection still applies). The data model should leave room for this from the
> start.

---

## Appendix: key reference values

- **Default role:** User (trust weight 15). Other weights: Trusted 40, VIP 50, Admin 100,
  TrustedBot 100.
- **Submission types:** NFO, MediaInfo, FileList.
- **Release categories:** Movies, TV, Games, Software, Music, Books, Audiobooks, Other,
  Unknown.
- **Product types:** Movie, TvSeries (live); Game, Book, Music, Software (reserved/future).
- **Moderation states:** Visible, Removed.
- **Tags:** PROPER, READNFO, REPACK, INTERNAL (release name); HAS_NOTES, SCENE_DRAMA,
  GROUP_NEWS, TECHNICAL (LLM).

> Removed from the original concept: the cumulative-trust **acceptance threshold** and the
> Pending/Accepted/Rejected state machine. Trust weights now serve only as a ranking signal;
> moderation is a separate Visible/Removed state.
