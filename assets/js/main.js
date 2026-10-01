/* ============================================================
   LPS ADVOCACIA — interações
   Sem dependências. Animações discretas, menu e formulário.
   ============================================================ */
(function () {
  "use strict";

  /* --- Configuração do formulário ---------------------------
     FORM_ENDPOINT: deixe vazio ("") para enviar as mensagens
     pelo WhatsApp. Para usar um serviço de formulário
     (Formspree, Basin, Netlify Forms etc.), cole a URL aqui. */
  var FORM_ENDPOINT = "";
  var WHATSAPP_NUMBER = "5555996099851";

  var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  /* --- Header com fundo ao rolar --------------------------- */
  var header = document.querySelector("[data-header]");

  function onScroll() {
    if (!header) return;
    if (window.scrollY > 40) header.classList.add("is-scrolled");
    else header.classList.remove("is-scrolled");
  }

  onScroll();
  window.addEventListener("scroll", onScroll, { passive: true });

  /* --- Menu mobile ----------------------------------------- */
  var toggle = document.querySelector("[data-menu-toggle]");
  var menu = document.querySelector("[data-mobile-menu]");

  function setMenu(open) {
    if (!toggle || !menu) return;
    toggle.setAttribute("aria-expanded", String(open));
    toggle.setAttribute("aria-label", open ? "Fechar menu" : "Abrir menu");
    menu.hidden = !open;
    document.body.style.overflow = open ? "hidden" : "";
    if (header) header.classList.toggle("is-open", open);
  }

  if (toggle && menu) {
    toggle.addEventListener("click", function () {
      setMenu(toggle.getAttribute("aria-expanded") !== "true");
    });

    menu.addEventListener("click", function (e) {
      if (e.target.closest("a")) setMenu(false);
    });

    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape") setMenu(false);
    });

    window.addEventListener("resize", function () {
      if (window.innerWidth > 960) setMenu(false);
    });
  }

  /* --- Animação de entrada (IntersectionObserver) ---------- */
  var reveals = document.querySelectorAll(".reveal");

  if (reduceMotion || !("IntersectionObserver" in window)) {
    reveals.forEach(function (el) {
      el.classList.add("is-visible");
    });
  } else {
    var io = new IntersectionObserver(
      function (entries) {
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
            io.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.12, rootMargin: "0px 0px -40px 0px" }
    );
    reveals.forEach(function (el) {
      io.observe(el);
    });
  }

  /* --- Ano do rodapé --------------------------------------- */
  document.querySelectorAll("[data-year]").forEach(function (el) {
    el.textContent = String(new Date().getFullYear());
  });

  /* --- Formulário de contato ------------------------------- */
  var form = document.querySelector("[data-contact-form]");

  if (form) {
    var status = form.querySelector("[data-form-status]");

    function setStatus(msg) {
      if (status) {
        status.textContent = msg;
        status.scrollIntoView({ block: "nearest", behavior: reduceMotion ? "auto" : "smooth" });
      }
    }

    function fieldError(input, msg) {
      var box = input.closest(".field") || input.closest(".consent");
      var slot = box ? box.querySelector(".error") : null;
      if (slot) slot.textContent = msg || "";
      input.setAttribute("aria-invalid", msg ? "true" : "false");
      return !msg;
    }

    function validate() {
      var ok = true;
      var nome = form.querySelector("#nome");
      var whats = form.querySelector("#whatsapp");
      var email = form.querySelector("#email");
      var assunto = form.querySelector("#assunto");
      var mensagem = form.querySelector("#mensagem");
      var consent = form.querySelector("#consentimento");

      if (!nome.value.trim()) ok = fieldError(nome, "Informe seu nome.") && ok;
      else fieldError(nome, "");

      var tel = whats.value.replace(/[^\d]/g, "");
      if (tel.length < 10 || tel.length > 13) {
        ok = fieldError(whats, "Informe um telefone com DDD.") && ok;
      } else fieldError(whats, "");

      if (email.value && !/^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(email.value.trim())) {
        ok = fieldError(email, "Informe um e-mail válido.") && ok;
      } else fieldError(email, "");

      if (!assunto.value) ok = fieldError(assunto, "Selecione o assunto.") && ok;
      else fieldError(assunto, "");

      if (mensagem.value.trim().length < 10) {
        ok = fieldError(mensagem, "Escreva uma mensagem com ao menos 10 caracteres.") && ok;
      } else fieldError(mensagem, "");

      if (!consent.checked) {
        ok = fieldError(consent, "É necessário concordar com o aviso de privacidade.") && ok;
      } else fieldError(consent, "");

      return ok;
    }

    form.addEventListener("submit", function (e) {
      e.preventDefault();
      if (!validate()) {
        setStatus("Revise os campos destacados para continuar.");
        return;
      }

      var data = {
        nome: form.querySelector("#nome").value.trim(),
        whatsapp: form.querySelector("#whatsapp").value.trim(),
        email: form.querySelector("#email").value.trim(),
        assunto: form.querySelector("#assunto").value,
        mensagem: form.querySelector("#mensagem").value.trim()
      };

      if (FORM_ENDPOINT) {
        setStatus("Enviando sua mensagem…");
        fetch(FORM_ENDPOINT, {
          method: "POST",
          headers: { "Content-Type": "application/json", Accept: "application/json" },
          body: JSON.stringify(data)
        })
          .then(function (res) {
            if (!res.ok) throw new Error("falha");
            form.reset();
            setStatus("Mensagem enviada. Retornaremos o contato em breve.");
          })
          .catch(function () {
            setStatus(
              "Não foi possível enviar agora. Entre em contato pelo WhatsApp " +
                "(55) 9 9609-9851."
            );
          });
        return;
      }

      // Sem endpoint configurado: envio pelo WhatsApp.
      var texto =
        "Olá, LPS Advocacia! Meu nome é " + data.nome + ".\n" +
        "Assunto: " + data.assunto + "\n" +
        "Telefone: " + data.whatsapp + "\n" +
        (data.email ? "E-mail: " + data.email + "\n" : "") +
        "\n" + data.mensagem;

      window.open(
        "https://wa.me/" + WHATSAPP_NUMBER + "?text=" + encodeURIComponent(texto),
        "_blank",
        "noopener"
      );

      form.reset();
      setStatus(
        "Abrimos o WhatsApp com sua mensagem. Se a janela não aparecer, " +
          "fale conosco pelo número (55) 9 9609-9851."
      );
    });
  }
})();
