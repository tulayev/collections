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
        const likesCount = data.likes_count
        const dislikesCount = data.dislikes_count
        const total = likesCount + dislikesCount

        document.querySelectorAll('.like-count').forEach(elem => {
            elem.innerText = likesCount
        })

        document.querySelectorAll('.dislike-count').forEach(elem => {
            elem.innerText = dislikesCount
        })

        if (total > 0) {
            document.getElementById('likeProgressbar').style.width = `${likesCount / total * 100}%`
            document.getElementById('dislikeProgressbar').style.width = `${dislikesCount / total * 100}%`
        } else {
            document.getElementById('likeProgressbar').style.width = '0%'
            document.getElementById('dislikeProgressbar').style.width = '0%'
        }
    } catch (e) {
        console.log(e.message)
    }
}

const vote = async (username, itemId, type) => {
    const settings = {
        method: 'POST',
        headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json',
        }
    }

    try {
        const res = await fetch(`/api/like?username=${username}&itemId=${itemId}&type=${type}`, settings)
        const data = await res.json()

        if (data.message === 'ok') {
            getLikes()
        }
    } catch (e) {
        console.log(e.message)
    }
}

// Comments

const getComments = async () => {
    try {
        const res = await fetch('/api/comments')
        const data = await res.json()

        if (data.comments.length > 0) {
            const commentsWrapper = document.getElementById('commentsWrapper')
            if (commentsWrapper) {
                console.log(data.comments)
                commentsWrapper.innerHTML = data.comments.map(c => {
                    `
                        <div class="flex my-4">
                            <div class="w-[20%] lg:w-[5%]">
                                <img src="~/images/${c.user.image}" class="w-10 rounded-full" alt="avatar" />
                            </div>
                             <div class="w-[80%] lg:w-[95%]">
                                <h4 class="text-blue-500 mb-2">
                                    ${c.user.name}
                                    <span class="dark:text-white text-gray-900 text-sm ml-6">09-12-12 00:12</span>
                                </h4>
                                <p class="dark:text-white text-gray-900">This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment This is my comment</p>
                            </div>
                        </div>
                    `
                })
            }
        }
    } catch (e) {
        console.log(e.message)
    }
}

const postComment = async (username, itemId, body) => {
    const settings = {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
        }
    }

    try {
        const res = await fetch(`/api/comments/post?username=${username}&itemId=${itemId}&body=${body}`, settings)
        const data = await res.json()

        if (data.message === 'ok') {
            getComments()
            document.getElementById('commentBody').value = ''
        }
    } catch (e) {
        console.log(e.message)
    }
}

export { populateData, themeCheck, themeSwitch, getLikes, vote, getComments, postComment }