/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["./**/*.{razor,cs}"],
    theme: {
        extend: {
        },
        screens: {
            "xs": "375px",
            "sm": "414px",
            "md": "768px",
            "lg": "1024px",
            "xl": "1440px",
            "xxl": "1700px",
            "2xl": "1920px"
        },
    },
    plugins: [],
}