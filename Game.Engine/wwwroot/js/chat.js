const chatToggleKey = "t";
const chat = document.getElementById("chat");
const messages = document.getElementById("chatMessages");
const textbox = document.getElementById("chatTextbox");

window.onkeypress = function(event) {
	if (document.getElementById("nick") !== document.activeElement) {
		if (event.key.toLowerCase() === chatToggleKey.toLowerCase()) {
			if (chat.style.display === "none") {
				chat.style.display = "block";
				textbox.focus();
				return false;
			} else if (textbox !== document.activeElement) {
				chat.style.display = "none";
			}
		}
	}
}

textbox.onkeypress = function(event) {
	if (event.key === "Enter") {
		var chat = new Chat();
		chat.send();
		return false;
	}
}

class Chat {
	
	send() {
		const txt = textbox.value;
		if (txt !== "") {
			textbox.value = "";
			// some code to send message to server
			update();
		}
	}

	update() {
		// some code for refresching the chat
	}
	
}