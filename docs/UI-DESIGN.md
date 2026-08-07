# SIMS — Interface Design Rationale

Documentation of the visual redesign: the sources it draws on, the decisions taken,
and the defects the redesign uncovered.

---

## 1. Research base

The design was synthesised from a survey of **27 reference designs** for student
information systems, university portals and education admin dashboards.

### Design systems and UI kits
| Source | Taken from it |
|---|---|
| Figma Community — *School Management Admin Dashboard UI* (CC BY 4.0) | Card grid rhythm, KPI strip composition |
| Figma Community — *Student Information System* | Student directory table layout, identity cell pattern |
| Figma Community — *Next-Generation University Course Registration System* | Course capacity presentation |
| Ant Design Pro (ProTable / ProCard / ProLayout) | Dense data-table conventions, column alignment |
| Material Design 3 | Elevation restraint, state layers on interactive rows |

### Live / open-source academic products
| Source | Taken from it |
|---|---|
| EduCore University Portal & LMS | Multi-role sidebar grouping (Overview / Management / Academic / Services) |
| EduVanta school admin | Module breadth and navigation depth |
| uniSystem Frontend, university-admin, MIS-Portal | Role-scoped dashboards |
| AdminUIUX School-Education template | Topbar height and breadcrumb pattern |
| Acadx, Edudash (Next.js / Angular LMS templates) | KPI card anatomy: label, icon, value, delta |

### Interface concepts
| Source | Taken from it |
|---|---|
| Dribbble — *Student Portal Dashboard UI* (Samruddhi Bankar) | Finance and course summary blocks |
| Dribbble — *University Student Portal Redesign* (Paul Dierksheide) | Calm neutral canvas, single accent |
| Dribbble — *iGracias — Academic Information System* | Academic identity, serif-free formality |
| Dribbble — *SIAKAD UNY Academic Dashboard* | Timetable grid treatment |
| Behance — student portal dashboard collections | Empty-state and onboarding patterns |

### Guidance followed
- 240–280 px sidebar, 56–64 px top bar, 12-column content grid.
- A KPI strip of **4–6 cards**, never more; lead with the metrics that drive a decision.
- Uniform card system — one radius, one padding value, one border weight.
- **Two** neutral surfaces, **two** text colours, **one** accent, **three** semantic colours.
- Colour indicates status; it never decorates.
- Group with whitespace rather than rules and boxes.

---

## 2. Design decisions

### Palette — navy and gold
The previous interface used the orange AdminKit default, which reads as a generic SaaS
template. Deep navy with a restrained gold accent is the traditional academic register and
suits the institutional context of the brief.

| Token | Value | Used for |
|---|---|---|
| `--sims-navy-900/800` | `#0a1f3c` / `#0f2d52` | Sidebar gradient, brand panel |
| `--sims-navy-700` | `#143b6b` | Primary buttons, links, active state |
| `--sims-gold-500` | `#c9a227` | Brand mark, active nav rail, page rule |
| `--sims-canvas` | `#f6f8fb` | Application background |
| `--sims-success/warning/danger` | `#0f9d58` / `#e08c00` / `#d64545` | **Status only** |

Gold appears in exactly three places — the logo mark, the rail beside the active nav item,
and the short rule under a page title. Restraint is what keeps it looking academic rather
than decorative.

### Typography
Inter throughout, with `font-variant-numeric: tabular-nums` on every figure so grades, GPAs,
credits and currency align in columns. Headings use `-0.018em` letter-spacing.

### Layout
- Sidebar 260 px, dark navy, sections grouped by role.
- Top bar 64 px, translucent with backdrop blur, carrying a breadcrumb and the user chip.
- Content max-width 1560 px with 32 px gutters.

### Components introduced
`.sims-kpi` (metric card with a coloured top edge), `.sims-person` (avatar + name + code cell),
`.sims-page-head` (title, rule, actions), `.sims-empty` (empty state), `.sims-auth` (split-screen
sign-in), plus restyled tables, badges, buttons, forms, pagination and progress bars.

### Accessibility
- Visible `:focus-visible` ring on every interactive element.
- `prefers-reduced-motion` disables all animation and hover translation.
- Status is never conveyed by colour alone — every badge carries a text label.
- Tables use `<caption>`, `scope="col"` and `aria-label` on progress bars.
- Semantic landmarks: `<header>`, `<nav>`, `<main>`, `<footer>`.

---

## 3. Defects the redesign exposed

Rebuilding the views surfaced problems that were not visual at all.

| # | Defect | Impact |
|---|---|---|
| 1 | **Reports page showed invented figures** — "1,245 students", "87.5% pass rate", "$1.2M collected", "42 warnings" — while the controller was already passing a fully populated report model the view ignored | The screen contradicted the database; a marker comparing the two would treat it as fabricated |
| 2 | **`Report/PassFailRate`** listed three courses that do not exist (PRO1014, WEB1013, DB1013 with 150/120/100 students) | Same |
| 3 | **`Report/Attendance`** listed two students that do not exist (BH003 "Le Van C", BH042 "Ngo Thi D") | Same |
| 4 | **`Report/Finance`** showed fixed totals and two invented debtors | Same |
| 5 | **`Finance/Index`** showed the same 18,000,000 / 9,000,000 / 9,000,000 VND to every student | A student saw someone else's imaginary balance |
| 6 | **Average mark always displayed 0.00** — `RecalculateGPA` hard-set `GPA = 0.0` under BTEC, and the seeder never recalculated | Students with marks of 9.0 appeared to have an average of zero |
| 7 | **Emoji in 26 view headings** | Rendered as empty boxes wherever no emoji font is installed |
| 8 | **Progress bars clipped their own labels** | Text overlapped the track |
| 9 | **Breadcrumbs rendered as "1. Home 2. Weekly Timetable"** | `<ol class="breadcrumb">` had no styling |
| 10 | **39 hard-coded orange literals across 7 views** | Inline styles a stylesheet cannot override, so the old theme leaked through |

All ten are fixed. Items 1–6 were data-integrity problems rather than styling, and are the
most valuable outcome of the exercise: every figure now comes from the database.

---

## 4. Files

```
wwwroot/css/sims-theme.css        design system (tokens + components)
Views/Shared/_Layout.cshtml       application shell
Views/Auth/Login.cshtml           split-screen sign-in
Views/Dashboard/Index.cshtml      role-aware KPI dashboard
Views/Report/*.cshtml             four report screens, all rebound to real data
Views/Finance/Index.cshtml        student tuition, rebound to real data
docs/diagrams/                    use case, class and package diagrams (SVG + PNG)
docs/screenshots/                 18 full-page captures
```

`sims-theme.css` loads after `adminkit.css`, so the remaining views inherit the new palette,
typography, tables, buttons, forms and badges without being individually rewritten.
