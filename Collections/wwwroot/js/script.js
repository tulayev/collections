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
})