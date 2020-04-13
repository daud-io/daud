import Cookies from "js-cookie";

const changelog = document.getElementById("changelog");
let currentChangelogVersion = "0";
const lastversion = Cookies.get("changelog");
let open = false;

if (changelog) currentChangelogVersion = changelog.getAttribute("data-version");

if (lastversion != currentChangelogVersion) {
    const cookieOptions = { expires: 300 };
    Cookies.set("changelog", currentChangelogVersion, cookieOptions);

    changelog.classList.add("new");
    open = true;
}

changelog.addEventListener("click", () => {
    if (open) changelog.classList.remove("new");
    else changelog.classList.add("new");
    open = !open;
});
