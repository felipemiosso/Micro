# Honeycomb Design System

## Overview
The Honeycomb Design System establishes the visual and structural language for Micro ATS. It aims for a "Clean, Professional, and Sharp" aesthetic, utilizing a color palette inspired by honeycombs: deep ambers, bright golds, and sophisticated dark zincs (Ink).

## Design Principles
- **Clarity over Clutter**: Focus on whitespace and high-contrast typography.
- **Structured Hierarchy**: Use honeycomb-inspired geometry (sharp edges, hex-accents) to denote structure.
- **Responsive & Dynamic**: Full support for varying screen sizes.

## 1. Visual Language

### Color Palette (Tailwind Tokens)
- **Primary (Honeycomb)**: `honeycomb-500` (#f59e0b) for primary actions and highlights.
- **Dark Neutral (Ink)**: `ink-dark` (#0f172a / `slate-900`) for navigation and primary text.
- **Surface**: `surface-alt` (#f8fafc / `slate-50`) for backgrounds.

### Typography
- **Font Family**: Inter (Sans-serif).
- **Scales**:
    - `heading-1`: 4xl, Bold, Ink, Tracking-tight.
    - `subtitle`: sm, Medium, slate-400.

### Geometry
- **Radius**: `rounded-lg` (8px) for inputs/buttons, `rounded-xl` (12px) for modals, `rounded-2xl` (16px) for main cards.
- **Shadows**: `shadow-sm` for cards, `shadow-xl` for overlays/modals.

## 2. Technical Implementation (Tailwind CSS)

### Global Classes (`src/styles.css`)

#### Buttons
- `.btn-primary`: Amber background, white text, bold, hover transitions.
- `.btn-secondary`: White background, zinc border, ink text, hover border tint.

#### Data & Badges
- `.badge-status`: Pill-shaped, font-black, uppercase, tracking-wider.
    - `.badge-amber`: Drafts, Applied.
    - `.badge-blue`: Interviewing.
    - `.badge-green`: Finalized, Published, Offer.
    - `.badge-zinc`: Closed, Archive.
    - `.badge-red`: Critical/Deleted.

#### Containers
- `.card-honeycomb`: White background, thin slate border, soft shadow, transition on hover.
- `.lane-honeycomb`: Kanban column style, top border accent.

## 3. Interaction Patterns

### Dialogs & Modals
- **Confirmation**: Use `ConfirmDialogComponent`. Standard centered modal with icon, clear title, and primary/secondary action buttons.
- **Notifications**: Use `NotificationService` (SnackBars) for transient feedback.

### Navigation
- **Header**: Sticky dark header with `honeycomb-400` branding.
- **Active Links**: `honeycomb-400` text with a 2px bottom border.

### Animations
- **Deep Linking**: Items navigated via fragment scroll must use `.highlight-pulse` (a temporary amber shadow and scale effect).
- **Drag & Drop**: Archive zone appears as a floating bottom bar during active drag events.

## 4. Components & Icons
- **Icons**: Material Design Icons (`mat-icon`).
- **Inputs**: `.input-honeycomb` (full width, slate border, amber focus ring).
