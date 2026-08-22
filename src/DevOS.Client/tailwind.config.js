/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        'devos-dark': '#0a0a0f',
        'devos-surface': '#131318',
        'devos-border': '#1f1f28',
        'devos-primary': '#6366f1',
        'devos-secondary': '#8b5cf6',
        'devos-accent': '#06b6d4',
      },
    },
  },
  plugins: [],
}