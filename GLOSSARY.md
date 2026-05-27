# crowdnfo-api: Glossary (Ubiquitous Language)

One row per term: the definition, where it lives in code, and the legacy name it replaces (the
sibling `../crowdNFO` app). Terms that were deliberately dropped are listed under "Retired legacy
terms" so old wording is not silently reused.

Verified against `src/Domain` as of 2026-05-27.

## Releases and naming

| Term | Definition | Code | Legacy term |
|---|---|---|---|
| Release | Aggregate root of the submission graph. Owns its aliases, variants, assets and name tags, and holds the two canonical choices. | `Domain/Releases/Release.cs` | `Release` |
| Canonical Name | The release's chosen primary name. Aliases resolve to it; it can never be removed. | `Release.CanonicalName` | canonical name |
| Canonical File Hash | The file hash of the top-ranked variant, i.e. the release's representative media file. Chosen by ranking, not by a threshold. | `Release.CanonicalFileHash` | canonical / accepted version |
| Release Alias | A purely textual alternate name for a release. Searching any alias resolves to the release. Never changes trust scores or creates variants. | `Domain/Releases/ReleaseAlias.cs` | `ReleaseAlias` |
| Release Variant | A distinct physical media file under the same release name, keyed by the SHA-256 of its main payload (e.g. the MKV/MP4). It has no submissions of its own: its cumulative trust score is aggregated from the asset submissions that supplied this media hash. The top-ranked variant's hash becomes the release's canonical file hash. | `Domain/Releases/ReleaseVariant.cs` | `ReleaseVariant` (had `AcceptedAt`) |
| Release Group | The scene or p2p group behind a release. A per-group naming ruleset is a planned future entity. | `Domain/Releases/ReleaseGroup.cs` | `ReleaseGroup` |

## Assets and submissions

| Term | Definition | Code | Legacy term |
|---|---|---|---|
| Asset | The deduplicated submitted content, one row per content hash. Abstract table-per-type base; mutated only through the `Release` aggregate. | `Domain/Assets/Asset.cs` | `File` (split across `NFOFile`, `MediaInfoFile`, `FileList`) |
| NFO Asset | Asset subtype for an `.nfo` file: file name, size, text, LLM tags. | `Domain/Assets/NfoAsset.cs` | `NFOFile` |
| MediaInfo Asset | Asset subtype for a MediaInfo report: video summary fields, raw `MediaInfoData` JSON, owned audio and subtitle tracks. | `Domain/Assets/MediaInfoAsset.cs` | `MediaInfoFile` |
| FileList Asset | Asset subtype for a release's file listing, owning its `FileListEntry` rows. | `Domain/Assets/FileListAsset.cs` | `FileList` |
| Content Hash | Server-computed SHA-256 of the asset content (NFO / MediaInfo / FileList), used for deduplication and trust aggregation. Never the media file hash. | `Asset.ContentHash` | content hash |
| Submission | One user's act of crediting an asset, at most once per user per asset. Optionally linked to a variant via the media file hash. | `Domain/Assets/Submission.cs` | `FileSubmission`, `FileListSubmission` |
| Original Submitter | The first user to submit an asset. Basis for the highscore "First" counters. | `Asset.OriginalSubmitterUserId` | implicit |
| Moderation State | `Visible` or `Removed`, shared by assets and comments. Removed content is excluded from ranking, which is then recomputed. | `Domain/Moderation/ModerationState.cs` | ad-hoc removal (did not re-rank) |

## Trust and ranking

| Term | Definition | Code | Legacy term |
|---|---|---|---|
| Trust Score | A user's per-submission weight, derived purely from their role. | `Role.TrustScore` | `TrustWeight` |
| Trust Score at Submit | Snapshot of the submitter's trust score at submit time. Later role changes do not retro-edit existing credits. | `Submission.TrustScoreAtSubmit` | `FileTrustGrade`, `FileListTrustGrade` tables |
| Cumulative Trust Score | Sum of the distinct submitters' snapshot scores on an asset or variant. A pure ranking signal with no acceptance threshold. | `Asset.CumulativeTrustScore`, `ReleaseVariant.CumulativeTrustScore` | `CumulativeTrustGrade` (threshold >= 100) |
| Ranking | The canonical choice. Order by cumulative trust score, then submission count, then oldest first (`CreatedAt`, then `Id`). Recomputed by the aggregate on every submit, remove and restore. | `Release.RecalculateRankings` | threshold-based acceptance |

## Products and enrichment

| Term | Definition | Code | Legacy term |
|---|---|---|---|
| Product | Abstract table-per-type enrichment target a release can link to. Carries title, year, genres, countries and external ids. | `Domain/Products/Product.cs` | `Product` |
| Movie / TvSeries | The two concrete product types. `TvSeries` owns `Season` to `Episode` and series air dates. | `Domain/Products/Movie.cs`, `TvSeries.cs` | `Movie`, `TvSeries` |
| Product External ID | A normalized external identifier on a product (`Source` + `Value`), used by the pipeline to find or create a product. | `Domain/Products/ProductExternalId.cs` | external id fields |

## Processing pipeline

| Term | Definition | Code | Legacy term |
|---|---|---|---|
| Processing Step | A code-driven pipeline stage (the steps are classes, not runtime config). | `Domain/Processing/ProcessingStep.cs` | processing condition |
| Processing Status | One row per owner and step, tracking state, idempotency hash, retry backoff and observability fields. | `Domain/Processing/ReleaseProcessingStatus.cs`, `ProductProcessingStatus.cs` | `ProcessingStatusBase` |

## Tags

| Term | Definition | Code | Legacy term |
|---|---|---|---|
| Tag | Fixed enum of release and NFO markers. Origin is encoded by the value object carrying it, not a separate field. | `Domain/Tags/Tag.cs` | `Tag`, `FileTag` |
| Release Tag | A name-derived tag (from the release name only), living on the release. | `Domain/Releases/ReleaseTag.cs` | name tag |
| NFO Tag | An LLM-derived tag with a confidence score, living on the NFO asset. | `Domain/Assets/NfoTag.cs` | LLM tag |

## Identity and community

| Term | Definition | Code | Legacy term |
|---|---|---|---|
| Role | Reference entity carrying `TrustScore` and `HasAdminAccess`. Seeded and data-driven. | `Domain/Users/Role.cs` | `Role` |
| Profile Visibility | `Public`, `MembersOnly` or `Private`. Username display is masked at read time per this setting. | `Domain/Users/ProfileVisibility.cs` | Private / Members-only / Public |
| User Application | Registration application with opaque JSON answers and a `Pending` / `Approved` / `Rejected` status. | `Domain/Users/UserApplication.cs` | `UserApplication` |
| Highscore | Denormalized per-user counters per area, recomputed from source via domain events, never incremented in place. | `Domain/Highscores/Highscore.cs` | `Highscore` |

## Enumerations

- **Role names and scores** (`RoleNames`, seeded in `Role`): `Admin` 100 (admin), `TrustedBot` 100, `VIP` 50, `Trusted` 40, `User` 15 (default).
- **ReleaseCategory**: `Unknown`, `Movies`, `TV`, `Games`, `Software`, `Music`, `Books`, `Audiobooks`, `Other`.
- **SubmissionType**: `Nfo`, `MediaInfo`, `FileList`.
- **ProcessingStep**: `NfoAnalysis`, `ReleaseTitleAnalysis`, `LlmAnalysis`, `ImdbSearch`, `ImdbBatch`, `ImdbDetail`, `MediathekArr`.
- **ProcessingState**: `NotApplicable`, `Ready`, `InProgress`, `Completed`, `Failed`.
- **ExternalIdSource**: `Imdb`, `Tvdb`, `Tmdb`.
- **ProfileVisibility**: `Public`, `MembersOnly`, `Private`.
- **ModerationState**: `Visible`, `Removed`.
- **Tag**: name-derived `Proper`, `ReadNfo`, `Repack`, `Internal`; LLM-derived `HasNotes`, `SceneDrama`, `GroupNews`, `Technical`.

## Retired legacy terms

Words from legacy rowdNFO backend that have no equivalent here. They were removed on purpose.

| Legacy term | Status | Replacement |
|---|---|---|
| Acceptance Threshold (cumulative >= 100) | Removed | Pure ranking; the highest cumulative trust score wins |
| Accepted / `AcceptedAt` | Removed | Canonical file hash is the top-ranked variant, with no accepted state |
| Candidate | Removed | No pending state; an asset is visible from the first submission |
| Pending / Accepted / Rejected (content) | Removed | Only `ModerationState` (`Visible` / `Removed`) exists for content |
| `FileTrustGrade` / `FileListTrustGrade` (cache tables) | Removed | Scores live directly on `Asset` and `ReleaseVariant` |
| Trust changes via moderator review (per the legacy FAQ) | Not implemented | Trust score is role-derived only and never adjusted per submission |
