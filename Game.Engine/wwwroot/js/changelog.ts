import Cookies from "js-cookie";

var changelog = document.getElementById("changelog");
var currentChangelogVersion = "0";
var lastversion = Cookies.get("changelog");
var open = false;

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
