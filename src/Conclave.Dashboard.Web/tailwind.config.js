/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["./**/*.{razor,cs}"],
    theme: {
        extend: {
            backgroundImage: {
                'addressButtonBg': "linear-gradient(174.21deg, #00FFFF -1.29%, rgba(65, 251, 251, 0.06) -1.28%, rgba(13, 4, 53, 0.1) 102.19%)",
                'card-gradient': "radial-gradient(rgba(68,138,255, 0.2), transparent)"
            }
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
