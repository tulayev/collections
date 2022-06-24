import { populateData, themeCheck, themeSwitch } from './functions.js'

document.addEventListener('DOMContentLoaded', () => {
    // Locale switcher
    const localeSwitcher = document.getElementById('localeSwitcher')
    const localeDropdown = document.getElementById('localeDropdown')

    localeSwitcher.addEventListener('click', () => {
        localeDropdown.classList.toggle('hidden')
    })

    window.addEventListener('click', (e) => {
        if (!e.target.matches('#localeSwitcher') && !e.target.matches('#localeDropdown')) {
            if (!localeDropdown.classList.contains('hidden')) {
                localeDropdown.classList.add('hidden')
            }
        }
    })

    // Search dropdown
    const searchInput = document.getElementById('searchInput')
    const searchResultList = document.getElementById('searchResultList')

    searchInput.addEventListener('input', (e) => {
        populateData(e.target.value, searchResultList)
    })

    // Theme switcher
    const moonIcon = document.getElementById('moon')
    const sunIcon = document.getElementById('sun')

    const userTheme = localStorage.getItem('theme')
    const systemTheme = window.matchMedia('(prefers-color-scheme: dark)').matches

    moonIcon.addEventListener('click', () => {
        themeSwitch(moonIcon, sunIcon)
    })

    sunIcon.addEventListener('click', () => {
        themeSwitch(moonIcon, sunIcon)
    })

    themeCheck(userTheme, systemTheme, moonIcon, sunIcon)
})