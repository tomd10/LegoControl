function addCustomListener(elementId, html) {
    document.getElementById(elementId).addEventListener("dragstart", function (e) {
        let crt = this.cloneNode(true);
        crt.style.position = "absolute"; crt.style.top = "auto"; crt.style.left = "-10000px";
        crt.innerHTML = html;
        document.body.appendChild(crt);
        e.dataTransfer.setDragImage(crt, 0, 0);
    }, false);
}