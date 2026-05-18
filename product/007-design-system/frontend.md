# Honeycomb Design System — Frontend Design

## Overview
Implementation of the Honeycomb Design System using Tailwind CSS. Optimized for a clean, professional ATS experience with deep charcoals and rich ambers.

## Tailwind Configuration (Tailwind CSS 3.4+)
The `tailwind.config.js` is the source of truth for design tokens.

### Theme Tokens
- **Font**: Inter (Sans-serif) - Import via Google Fonts in `index.html`.
- **Colors**:
    - `amber`: Default Tailwind amber (for primary actions).
    - `zinc`: Default Tailwind zinc (for neutral scales).
    - `background`: White (#FFFFFF).
    - `surface`: Off-white/Light gray (#F9FAFB).
    - `text-primary`: Zinc-900 (#18181B).
    - `text-secondary`: Zinc-500 (#71717A).

## Global Component Classes (src/styles.css)

### Buttons
- `.btn-primary`: `bg-amber-600 hover:bg-amber-700 text-white font-semibold py-2 px-4 rounded-lg transition-colors duration-200 shadow-sm`
- `.btn-secondary`: `bg-white border border-zinc-300 text-zinc-700 hover:bg-zinc-50 font-semibold py-2 px-4 rounded-lg transition-colors duration-200 shadow-sm`
- `.btn-ghost`: `text-zinc-600 hover:text-zinc-900 hover:bg-zinc-100 py-2 px-4 rounded-lg transition-colors duration-200`

### Layout & Containers
- `.card`: `bg-white border border-zinc-200 rounded-xl shadow-sm overflow-hidden`
- `.container-main`: `max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8`
- `.section-header`: `flex items-center justify-between mb-8`

### Data & Feedback
- `.table-honeycomb`: Clean, borderless rows with subtle hover effects.
- `.badge`: `inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium uppercase tracking-wider`
- `.badge-amber`: `bg-amber-100 text-amber-800 border border-amber-200`
- `.badge-zinc`: `bg-zinc-100 text-zinc-800 border border-zinc-200`

### Typography
- `.heading-1`: `text-3xl font-bold text-zinc-900 tracking-tight`
- `.heading-2`: `text-xl font-semibold text-zinc-900`
- `.body-text`: `text-sm text-zinc-600 leading-relaxed`

## Interaction States
- **Focus**: `focus:outline-none focus:ring-2 focus:ring-amber-500 focus:ring-offset-2`
- **Hover**: Subtle transitions on interactive elements (200ms).
- **Empty States**: Centered illustrations with zinc-400 color and "Clean" iconography.

## Design Constraints
- **Radius**: Use `rounded-lg` (8px) or `rounded-xl` (12px) for a modern, soft feel.
- **Spacing**: Strictly follow Tailwind's spacing scale (4, 8, 12, 16, 24, 32).
- **Icons**: Use Lucide Angular (lucide-angular) with 1.5px stroke width.
