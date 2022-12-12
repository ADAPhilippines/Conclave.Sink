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
            },
            maxWidth: {
                '4': '1444px',
                '3': '1081px',
                '2': '718px',
            }
        },
        screens: {
            "xs": "375px",
            "sm": "414px",
            "smtab": "600px",
            "md": "768px",
            "tab": "900px",
            "lg": "1024px",
            "laptop": "1220px",
            "xl": "1440px",
            "xxl": "1700px",
            "2xl": "1920px"
        },
    },
    plugins: [],
}
