# Honeycomb Design System — Frontend Design

## Overview
Implementation of the Honeycomb Design System using Tailwind CSS. Optimized for a clean, professional ATS experience with deep charcoals and rich ambers.

## Tailwind Configuration
The `tailwind.config.js` is the source of truth for design tokens.

### Theme Tokens
- **Font**: Inter (Sans-serif)
- **Colors**:
    - `honeycomb`: Amber/Gold scale (50-900).
    - `ink`: Professional charcoal scale (`DEFAULT`, `light`, `dark`).
    - `surface`: Background colors (`DEFAULT`, `alt`).
- **Shadows**: `honeycomb` (soft amber glow).

## Global Styles (src/styles.css)

### Core Components
- `.btn-primary`: Amber solid, white text, refined padding/rounding.
- `.btn-secondary`: White background, amber border/text.
- `.card-honeycomb`: White slate-bordered card with soft shadows.
- `.input-honeycomb`: High-contrast input with amber focus ring.
- `.lane-honeycomb`: Surface-alt background, amber accent top border.
- `.badge-honeycomb`: Tinted amber pill with borders.
- `.badge-status`: Technical status badge (small, bold, tracking-wider).

### Layout Elements
- `.nav-container`: Sticky ink-dark header with amber accent.
- `.brand`: Bold white text with amber span highlights.

### Utilities
- `.bg-hex-pattern`: Subtle SVG hexagonal background motif.
