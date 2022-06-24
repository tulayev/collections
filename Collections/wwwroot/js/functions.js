// Dropdown

const populateData = async (input, list) => {
    try {
        const response = await fetch(`/api/search?keyword=${input}`)
        const data = await response.json()
        list.innerHTML = ''

        if (input.length > 0) {
            data.map(item => {
                list.innerHTML +=
                    `
                    <li class="border-b border-gray-700">
                        <a href="#" class="block hover:bg-gray-700 flex items-center transition ease-in-out duration-150 px-3 py-3">
                            <img src="https://cdn.myanimelist.net/images/anime/1079/109928.jpg?s=33c1c9c33504d732712ca3e1ebcafc7a" alt="cover" class="w-10">
                            <span class="ml-4">${item.name}</span>
                        </a>
                    </li>
                    `
            })
        } else {
            list.innerHTML = ''
        }
    } catch (e) {
        console.log(e.message)
    }
}

// Theme Switch

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

// Like/Dislike

const getLikes = async () => {
    try {
        const res = await fetch('/api/likes-count')
        const data = await res.json()
        document.querySelectorAll('.like-count').forEach(elem => {
            elem.innerText = data.likes
        })
    } catch (e) {
        console.log(e.message)
    }
}

const like = async (username, itemId) => {
    const settings = {
        method: 'POST',
        headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json',
        }
    }

    try {
        const res = await fetch(`/api/like?username=${username}&itemId=${itemId}`, settings)
        const data = await res.json()

        if (data.message === 'ok') {
            getLikes()
        }
    } catch (e) {
        console.log(e.message)
    }
}

export { populateData, themeCheck, themeSwitch, getLikes, like }