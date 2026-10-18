# Google PageSpeed Report: rooshakshop.ir

- **URL tested:** https://rooshakshop.ir (redirects to https://rooshakshop.ir/fa)
- **Run:** 2026-10-18, Lighthouse 13.5.0, PageSpeed Insights v5
- **Field data (CrUX):** none. Chrome traffic is too low to qualify, so all numbers below are lab data from a single run.

## Lighthouse scores

| Category | Mobile | Desktop |
|---|---|---|
| Performance | 97 | 100 |
| Accessibility | 100 | 100 |
| Best Practices | 96 | 96 |
| SEO | 100 | 100 |

## Lab metrics

| Metric | Mobile | Desktop | Rating (mobile) |
|---|---|---|---|
| LCP | 1.6 s | 0.7 s | Good |
| CLS | 0 | 0 | Good |
| TBT (lab proxy for INP) | 60 ms | 10 ms | Good |
| FCP | 1.3 s | 0.3 s | Good |
| Speed Index | 4.6 s | 0.5 s | Needs Improvement |
| Time to Interactive | 3.9 s | 0.9 s | Needs Improvement |

## Findings

| Priority | Issue | Evidence | Fix |
|---|---|---|---|
| High | Hero video is 14.4 MB, about 94% of the 15.3 MB page weight | `/uploads/landingslides/3/…_hero.mp4` | Re-encode it to about 1–2 MB (H.264/H.265 or WebM, 720p, lower bitrate, no audio track). Add `preload="none"` or `metadata` plus a `poster` image, and consider serving a static image on mobile. This is the main cause of the slow mobile Speed Index and TTI, and it costs mobile visitors a lot of data. |
| High | Root redirect costs about 780 ms on mobile | `/` redirects to `/fa` | Serve the default language straight at `/`, or use one server-level 301. Point canonical, internal links and the sitemap at the final URL. |
| Medium | Product images are larger than needed (about 370–430 KiB could be saved) | Product WebPs of 75–105 KB displayed at 236×294 px | Serve them through `next/image` with proper `sizes`, or generate smaller variants (about 480 px wide). |
| Medium | Placeholder image returns 400 | `/_next/image?url=/images/user-placeholder.png` | The file is probably missing from `/public/images/`, or the path is wrong. Add the file or fix the reference. |
| Low | Unused CSS (about 12 KiB) | Tailwind base layer | Minor. Check the Tailwind `content` paths. |

## Relation to lucaloupe.ir

This site appears to run on the same platform as lucaloupe.ir (Next.js, `store-*` classes, `/fa` locale redirect). Fixing the root redirect in the shared codebase fixes it on both sites. Unlike lucaloupe.ir, this site had no CSP font error.

## How to check the fixes worked

- **Video fix:** the next run's "Total size" drops below about 3 MB and mobile Speed Index falls under 3.4 s.
- **Redirect fix:** the "Avoid multiple page redirects" audit disappears.
- **Placeholder fix:** running `curl -I "https://rooshakshop.ir/_next/image?url=%2Fimages%2Fuser-placeholder.png&w=480&q=75"` returns 200.
- Lab scores vary between runs, so run each test 2–3 times before comparing.
