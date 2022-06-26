const iconToggle = (moonIcon, sunIcon) => {
    moonIcon.classList.toggle('hidden')
    sunIcon.classList.toggle('hidden')
}

const themeCheck = (userTheme, systemTheme, moonIcon, sunIcon) => {
    if (userTheme === 'dark' || (!userTheme && systemTheme)) {
        document.documentElement.classList.add('dark')
        moonIcon.classList.add('hidden')
        return
    }

    sunIcon.classList.add('hidden')
}

const themeSwitch = (moonIcon, sunIcon) => {
    if (document.documentElement.classList.contains('dark')) {
        document.documentElement.classList.remove('dark')
        localStorage.setItem('theme', 'light')
        iconToggle(moonIcon, sunIcon)
        return
    }

    document.documentElement.classList.add('dark')
    localStorage.setItem('theme', 'dark')
    iconToggle(moonIcon, sunIcon)
}

export { themeCheck, themeSwitch }