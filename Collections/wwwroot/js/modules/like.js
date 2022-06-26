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
            'Accept': 'application/json',
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

export { getLikes, vote }