const progress = document.getElementById("cooldown");
export class Cooldown {
    setCooldown(data) {
        progress.value = data;
    }
}
