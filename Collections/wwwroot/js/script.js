import { populateData } from './functions.js'

document.addEventListener('DOMContentLoaded', () => {
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

    const searchInput = document.getElementById('searchInput')
    const searchResultList = document.getElementById('searchResultList')

    searchInput.addEventListener('input', (e) => {
        populateData(e.target.value, searchResultList)
    })
})