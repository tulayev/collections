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

export { populateData }