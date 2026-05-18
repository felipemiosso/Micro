# Honeycomb Design System

## Overview
The Honeycomb Design System establishes the visual and structural language for Micro ATS. It aims for a "Clean, Professional, and Sharp" aesthetic, utilizing a color palette inspired by honeycombs: deep ambers, bright golds, and sophisticated zincs.

## Design Principles
- **Clarity over Clutter**: Focus on whitespace and high-contrast typography.
- **Structured Hierarchy**: Use honeycomb-inspired geometry (sharp edges, hex-accents) where appropriate to denote structure.
- **Responsive & Dynamic**: Full support for varying screen sizes.

## 1. Visual Language

### Color Palette
- **Primary (Amber)**: `amber-600` (#D97706) for primary actions, success states, and highlights.
- **Neutral (Zinc)**: `zinc-900` (#18181B) for text, and `zinc-200` (#E4E4E7) for borders.
- **Backgrounds**:
    - `background`: White (#FFFFFF).
    - `surface`: Off-white/Light gray (#F9FAFB / `zinc-50`).

### Typography
- **Font Family**: Inter (Sans-serif). Import via Google Fonts.
- **Scales**:
    - `heading-1`: 3xl, Bold, Zinc-900, Tracking-tight.
    - `heading-2`: xl, Semibold, Zinc-900.
    - `body-text`: sm, Zinc-600, Leading-relaxed.

### Geometry
- **Radius**: `rounded-lg` (8px) for components, `rounded-xl` (12px) for cards.
- **Shadows**: `shadow-sm` for standard cards, `shadow-md` for interactive elements.

## 2. Technical Implementation (Tailwind CSS)

### Global Classes (`src/styles.css`)

#### Buttons
- `.btn-primary`: `bg-amber-600 hover:bg-amber-700 text-white font-semibold py-2 px-4 rounded-lg transition-colors duration-200 shadow-sm`
- `.btn-secondary`: `bg-white border border-zinc-300 text-zinc-700 hover:bg-zinc-50 font-semibold py-2 px-4 rounded-lg transition-colors duration-200 shadow-sm`
- `.btn-ghost`: `text-zinc-600 hover:text-zinc-900 hover:bg-zinc-100 py-2 px-4 rounded-lg transition-colors duration-200`

#### Layout & Containers
- `.card`: `bg-white border border-zinc-200 rounded-xl shadow-sm overflow-hidden`
- `.container-main`: `max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8`
- `.section-header`: `flex items-center justify-between mb-8`

#### Data & Badges
- `.badge`: `inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium uppercase tracking-wider`
- `.badge-amber`: `bg-amber-100 text-amber-800 border border-amber-200`
- `.badge-zinc`: `bg-zinc-100 text-zinc-800 border border-zinc-200`

## 3. Interaction & Constraints
- **Focus States**: `focus:outline-none focus:ring-2 focus:ring-amber-500 focus:ring-offset-2`.
- **Spacing**: Strictly follow Tailwind's spacing scale (4, 8, 12, 16, 24, 32).
- **Icons**: Use **Material Design Icons** (`mat-icon`) for all interface elements. Prefer standard symbols like `assignment`, `work`, `people`, and `view_kanban`.
- **Empty States**: Centered illustrations with `zinc-400` color.
