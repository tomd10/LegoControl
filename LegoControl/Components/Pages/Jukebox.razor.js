export function SetVolume() {
    DotNet.invokeMethodAsync('LegoControl', 'SetVolume', 1*document.getElementById("slider").value);
}

export function addHandlers() {
    const btn = document.getElementById("slider");
    btn.addEventListener("input", SetVolume);
}
