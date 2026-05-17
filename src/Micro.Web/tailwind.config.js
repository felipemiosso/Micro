/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      colors: {
        honeycomb: {
          50: '#fffbeb',
          100: '#fef3c7',
          200: '#fde68a',
          300: '#fcd34d',
          400: '#fbbf24', // Gold
          500: '#f59e0b', // Amber
          600: '#d97706',
          700: '#b45309',
          800: '#92400e',
          900: '#78350f',
        },
        ink: {
          DEFAULT: '#111827', // Darker, more professional gray-900
          light: '#374151', // Gray-700
          dark: '#030712', // Near black
        },
        surface: {
          DEFAULT: '#ffffff',
          alt: '#f9fafb', // Gray-50 for backgrounds
        }
      },
      boxShadow: {
        'honeycomb': '0 4px 14px 0 rgba(245, 158, 11, 0.1)',
      }
    },
  },
  plugins: [],
}
