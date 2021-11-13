var menG;
var btnG;
var menB;
var btnB;
window.onload = function () {
    menG = document.getElementById('alertG');
    btnG = document.getElementById('btnG');
    menB = document.getElementById('alertB');
    btnB = document.getElementById('btnB');
    if (btnG != null) {
        btnG.addEventListener("click", AlertaG);
    }
    if (btnB != null) {
        btnB.addEventListener("click", AlertaB);
    }
}

function AlertaG() {
    menG.style.display = 'none';
    btnG.removeEventListener("click", AlertaG)
}

function AlertaB() {
    menB.style.display = 'none';
    btnB.removeEventListener("click", AlertaB)
}