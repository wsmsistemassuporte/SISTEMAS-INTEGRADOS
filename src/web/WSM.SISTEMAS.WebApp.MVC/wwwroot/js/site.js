// Interações do layout da loja: menu móvel e dropdown do usuário.
// Sem dependências além do DOM padrão.

(function () {
    "use strict";

    // Menu móvel (botão hambúrguer no cabeçalho)
    var menuToggle = document.getElementById("menu-toggle");
    var menuMobile = document.getElementById("menu-mobile");

    if (menuToggle && menuMobile) {
        menuToggle.addEventListener("click", function () {
            var aberto = menuMobile.classList.toggle("hidden") === false;
            menuToggle.setAttribute("aria-expanded", String(aberto));
        });
    }

    // Dropdown do usuário (abre no clique, fecha ao clicar fora ou com Esc)
    var userMenu = document.getElementById("user-menu");
    var userButton = document.getElementById("user-menu-button");
    var userPanel = document.getElementById("user-menu-panel");

    if (userMenu && userButton && userPanel) {
        function fecharMenu() {
            userPanel.classList.add("hidden");
            userButton.setAttribute("aria-expanded", "false");
        }

        userButton.addEventListener("click", function (evento) {
            evento.stopPropagation();
            var aberto = userPanel.classList.toggle("hidden") === false;
            userButton.setAttribute("aria-expanded", String(aberto));
        });

        document.addEventListener("click", function (evento) {
            if (!userMenu.contains(evento.target)) {
                fecharMenu();
            }
        });

        document.addEventListener("keydown", function (evento) {
            if (evento.key === "Escape") {
                fecharMenu();
            }
        });
    }
})();
