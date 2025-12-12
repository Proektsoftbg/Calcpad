function snakeCase(text) {
    return text.toLowerCase().split(" ").join("_");
}
function scanHeaders(parent) {
    if (parent === void 0) { parent = document.body; }
    /*
     * Iterate through header tags in page
     * - Side effect: If header has no ID tag, generate one based by snake-casing
     *   the inner text
     *  - Otherwise, add ID to set of processed IDs
     *  - Also, if generated name collides with another one, append a number
     *
     * Return: An array of IDs
     *
     */
    var headers = parent.querySelectorAll("h1, h2, h3, h4, h5, h6");
    var processed = Object();
    var temp = new Array();
    for (var i = 0; i < headers.length; i++) {
        var h = headers[i];
        var processed_id = h.getAttribute("id");
        // No ID found
        if (!processed_id) {
            var snake_cased = snakeCase(h.innerText);
            if (snake_cased in processed) {
                processed_id = snake_cased + "_" + ++processed[snake_cased];
            }
            else {
                processed_id = snake_cased;
                processed[processed_id] = 1;
            }
            h.setAttribute("id", processed_id);
        }
        else {
            processed[processed_id] = 1;
        }
        temp.push({
            'level': parseInt(h.tagName[1]),
            'name': h.innerText,
            'id': processed_id
        });
    }
    return temp;
}
function makeListItem(href, text) {
    // Generate a bullet point containing a link
    var link = document.createElement('a');
    link.setAttribute('href', href);
    link.innerHTML = text;
    var temp = document.createElement('li');
    temp.appendChild(link);
    return temp;
}
function makeList(listParams) {
    if (listParams === void 0) { listParams = {
        'target': "",
        'parent': "body"
    }; }
    var headers = scanHeaders(document.querySelector(listParams.parent));
    // Keep track of where we are in the list;
    var parents = [
        document.createElement("ul")
    ];
    var currentLevel = null; // Used to determine when to indent/dedent list
    var prevBullet = null;
    for (var i in headers) {
        var currentParent = parents.slice(-1)[0];
        var tocItem = headers[i];
        var link = makeListItem("#" + tocItem.id, tocItem.name);
        // Add link text
        if (currentLevel) {
            var levelDiff = tocItem.level - currentLevel;
            if (levelDiff <= 0) {
                // Dedent (or stay the same)
                while (parents.length > 1 && levelDiff) {
                    parents.pop();
                    levelDiff++;
                }
            }
            else if (levelDiff > 0) {
                // Indent
                while (levelDiff) {
                    var nextParent = document.createElement("ul");
                    prevBullet.appendChild(nextParent);
                    currentParent.appendChild(prevBullet);
                    currentParent = nextParent;
                    parents.push(nextParent);
                    levelDiff--;
                }
            }
            // Update parent
            currentParent = parents.slice(-1)[0];
        }
        // Update
        currentLevel = tocItem.level;
        currentParent.appendChild(link);
        prevBullet = link;
    }
    var target = document.querySelector(listParams.target);
    target.appendChild(parents[0]);
}
//# sourceMappingURL=toc.js.map