/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["./**/*.{razor,cs}"],
    theme: {
        extend: {
            backgroundImage: {
                'addressButtonBg': "linear-gradient(174.21deg, #00FFFF -1.29%, rgba(65, 251, 251, 0.06) -1.28%, rgba(13, 4, 53, 0.1) 102.19%)",
                'card-gradient': "radial-gradient(rgba(68,138,255, 0.2), transparent)",
                'nav-link-gradient': "linear-gradient(90deg, rgba(255, 255, 255, 0.06) 0%, rgba(255, 255, 255, 0) 100%)",
            },
            colors: {
                'primary': '#AF1CE2',
                'secondary': '#41FBFB',
                'subtext': 'var(--mud-palette-text-secondary)',
                'lines': 'var(--mud-palette-lines-default)',
                'info-darken': 'var(--mud-palette-info-darken)'
            }
        },
        screens: {
            "xs": "375px",
            "sm": "414px",
            "xm": "500px",
            "md": "768px",
            "lg": "1024px",
            "xl": "1440px",
            "xxl": "1700px",
            "2xl": "1920px"
        },
    },
    plugins: [],
}
