/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./Pages/**/*.{html,js,cshtml,razor,cs}", "./Shared/**/*.{html,js,cshtml,razor,cs}"],
  theme: {
    screens: {
      "xs": "420px",
      "sm": "640px",
      "md": "768px",
      "lg": "1024px",
      "xl": "1280px",
      "2xl": "1536px"
    },
    extend: {},
  },
  plugins: [],
}
