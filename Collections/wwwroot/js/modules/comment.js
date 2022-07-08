const getComments = async (itemId) => {
    try {
        const res = await fetch(`/api/comments?itemId=${itemId}`)
        const data = await res.json()

        const commentsWrapper = document.getElementById('commentsWrapper')
        if (commentsWrapper) {
            if (data.comments.length > 0) {
                commentsWrapper.innerHTML = data.comments.map(c => {
                    return `
                        <div class="flex my-8">
                            <div class="w-[20%] lg:w-[5%]">
                                <img src="${c.user.image ?? '/images/avatar.png'}" class="w-10 rounded-full" alt="avatar" />
                            </div>
                                <div class="w-[80%] lg:w-[95%]">
                                <h4 class="text-blue-500 mb-2">
                                    ${c.user.name}
                                    <span class="dark:text-white text-gray-900 text-sm ml-6">${c.createdAt}</span>
                                </h4>
                                <p class="dark:text-white text-gray-900">${c.body}</p>
                            </div>
                        </div>
                    `
                }).join('')
            } else {
                const displayInfo = {
                    'en': 'No comments yet...',
                    'uz': 'Bu yerda izohlar hali yo\'q...'
                }
                commentsWrapper.innerHTML = `
                    <div class="flex my-8">
                        <p class="dark:text-white text-gray-900">${displayInfo[document.documentElement.lang]}</p>
                    </div>
                `
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
            getComments(itemId)
            document.getElementById('commentBody').value = ''
        }
    } catch (e) {
        console.log(e.message)
    }
}

export { getComments, postComment }