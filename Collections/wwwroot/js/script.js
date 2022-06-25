import { populateData, themeCheck, themeSwitch, getLikes, vote, getComments, postComment } from './functions.js'

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

    // Burger menu

    const burgerBtn = document.getElementById('burgerBtn')
    const burgerContent = document.getElementById('burgerContent')

    burgerBtn.addEventListener('click', () => {
        burgerContent.classList.toggle('hidden')

        burgerBtn.querySelectorAll('img').forEach(img => {
            img.classList.toggle('hidden')
        })
    })

    // Like/Dislike

    const likeBtn = document.getElementById('like')
    const dislikeBtn = document.getElementById('dislike')
    const username = document.getElementById('username')
    const itemId = document.getElementById('itemId')

    const type = {
        'like': 1,
        'dislike': 0
    }

    if (likeBtn) {
        likeBtn.addEventListener('click', () => {
            vote(username.value, itemId.value, type.like)
        })
    }

    if (dislikeBtn) {
        dislikeBtn.addEventListener('click', () => {
            vote(username.value, itemId.value, type.dislike)
        })
    }

    getLikes()

    // Comments

    const commentBody = document.getElementById('commentBody')
    const commentSendBtn = document.getElementById('commentSendBtn')

    if (commentBody && commentSendBtn) {
        if (commentBody.value.length <= 0) {
            commentSendBtn.classList.add('cursor-not-allowed')
            commentSendBtn.disabled = true
        }

        commentBody.addEventListener('input', (e) => {
            if (e.target.value.length > 0) {
                commentSendBtn.classList.remove('cursor-not-allowed')
                commentSendBtn.disabled = false
            } else {    
                commentSendBtn.classList.add('cursor-not-allowed')
                commentSendBtn.disabled = true
            }
        })

        commentSendBtn.addEventListener('click', () => {
            postComment(username.value, itemId.value, commentBody.value)
        })
    }

    getComments()
})