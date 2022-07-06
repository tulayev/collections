const populateData = async (input, list) => {
    try {
        const response = await fetch(`/api/search?keyword=${input}`)
        const data = await response.json()
        list.innerHTML = ''
        if (input.length > 0) {
            let listItem

            data.map(i => {
                if (i.item.image && i.item.image.length > 0) {
                    listItem = `
                    <li class="border-b border-gray-700">
                        <a href="/home/show/${i.item.slug}" class="block hover:bg-gray-700 flex items-center transition ease-in-out duration-150 px-3 py-3">
                            <img src="/uploads/${i.item.image}" alt="cover" class="w-10">
                            <span class="ml-4">${i.item.name}</span>
                        </a>
                    </li>
                `
                } else {
                    listItem = `
                    <li class="border-b border-gray-700">
                        <a href="/home/show/${i.item.slug}" class="block hover:bg-gray-700 flex items-center transition ease-in-out duration-150 px-3 py-3">
                            <div class="flex justify-center items-center w-10 h-14 bg-gray-900">
                                <span class="text-white text-sm">No Image</span>
                            </div>
                            <span class="ml-4">${i.item.name}</span>
                        </a>
                    </li>
                `
                }
                list.innerHTML += listItem
            })
        } else {
            list.innerHTML = ''
        }
    } catch (e) {
        console.log(e.message)
    }
}

export { populateData }