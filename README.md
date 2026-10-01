# lps-advogados

Site institucional da **LPS Advocacia** — Santiago/RS.
HTML, CSS e JavaScript puro, sem etapa de build, responsivo (mobile-first), acessível e otimizado para SEO.

> **Direção:** azul-marinho + dourado discreto + fotografia da equipe como elemento protagonista.
> **Comunicação:** informativa e sóbria, conforme o Provimento 205/2021 da OAB — sem promessas de resultado, depoimentos, rankings ou comparações.

---

## 1. Estrutura

```
advogados/
├── index.html                      ← Homepage
├── o-escritorio/index.html         ← O Escritório
├── equipe/index.html               ← Equipe (cards + perfis completos)
├── areas-de-atuacao/index.html     ← Áreas de atuação
│   ├── familia/index.html          ← Direito de Família
│   ├── trabalhista/index.html      ← Direito Trabalhista
│   ├── civil/index.html            ← Direito Civil
│   ├── imobiliario/index.html      ← Regularização de Imóveis
│   └── previdenciario/index.html   ← Direito Previdenciário
├── contato/index.html              ← Contato (formulário)
├── politica-de-privacidade/index.html
├── assets/
│   ├── css/style.css               ← Design system completo
│   ├── js/main.js                  ← Menu, animações e formulário
│   └── img/                        ← Logo, fotos, favicon, OG image
├── sitemap.xml
├── robots.txt
└── tools/                          ← Scripts de desenvolvimento (não publicar)
    ├── build-images.ps1            ← Regenera cortes/miniaturas das fotos
    ├── server.ps1                  ← Servidor local de preview (porta 8086)
    ├── validate.ps1                ← Checa links, headings e classes
    ├── ImgTool.cs                  ← Utilitário de imagem (C#)
    └── responsive.html             ← Harness de QA responsivo
```

---

## 2. Como visualizar localmente

```powershell
powershell -ExecutionPolicy Bypass -File tools\server.ps1
```

Acesse `http://localhost:8086/`.
(Também é possível abrir o `index.html` direto no navegador, mas o servidor local
reproduz melhor o comportamento das URLs limpas.)

Para validar o projeto após qualquer edição:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validate.ps1
```

---

## 3. Checklist — antes de publicar

### 3.1. Dados dos profissionais (marcados com `[...]` e comentários `<!-- EDITAR -->`)

| Onde | O que preencher |
|---|---|
| `index.html` → seção Equipe | Nome, cargo, OAB, tags de áreas, mini biografia |
| `equipe/index.html` → seção Perfis | Nome, OAB, apresentação, formação, atuação |

> Use apenas informações verificáveis: nome completo, número real da OAB,
> formações comprovadas. Evite adjetivos e qualquer promessa de resultado.

### 3.2. Domínio

Hoje o domínio usado em `canonical`, `og:url`, JSON-LD, `sitemap.xml` e `robots.txt`
é o placeholder **`https://www.lpsadvocacia.com.br`**.
Substitua por todos os arquivos HTML, `sitemap.xml` e `robots.txt` (busca global: `Ctrl+H` no editor).

### 3.3. Formulário de contato

O envio funciona **pelo WhatsApp** por padrão (sem backend).
Para usar um serviço de formulário (Formspree, Basin, Netlify Forms…), edite em `assets/js/main.js`:

```js
var FORM_ENDPOINT = ""; // cole a URL do serviço entre as aspas
```

### 3.4. Política de Privacidade

Revisar e aprovar com o responsável jurídico antes da publicação
(itens marcados com `<!-- EDITAR -->`: data, e-mail oficial, cookies de terceiros).

### 3.5. Fotografia

As fotos atuais são provisórias (cortes da arte enviada).
O PRD prevê um **ensaio fotográfico institucional** — substituir em `assets/img/`:

| Arquivo | Uso | Proporção sugerida |
|---|---|---|
| `hero-equipe.jpg` | Hero da homepage | 1:1 (mín. 1200×1200) |
| `retrato-1.jpg`, `retrato-2.jpg` | Cards e perfis da equipe | 4:5 (mín. 800×1000) |
| `escritorio.jpg` | Seção O Escritório | ~16:11 (mín. 1200×825) |

Depois de trocar, rode `tools/build-images.ps1` para regerar favicon e OG image a partir do logo.

---

## 4. Onde editar cada conteúdo

| Conteúdo | Arquivo |
|---|---|
| Textos da home (hero, manifesto, escritório, equipe, áreas, etapas, atendimento, CTA) | `index.html` |
| Texto/valores do O Escritório | `o-escritorio/index.html` |
| Cards e perfis da equipe | `equipe/index.html` |
| Cards das áreas | `areas-de-atuacao/index.html` |
| Conteúdo de cada área | `areas-de-atuacao/<slug>/index.html` |
| Dados de contato + formulário | `contato/index.html` e `index.html` (seção Contato) |
| Política de privacidade | `politica-de-privacidade/index.html` |
| Cores, tipografia, espaçamentos, breakpoints | `assets/css/style.css` (bloco “1. Tokens”) |
| WhatsApp, endpoint do formulário | `assets/js/main.js` (topo do arquivo) |
| Telefone, endereço, Instagram | Todos os HTMLs (busca global) |

**Números usados no site**

- WhatsApp/telefone: `(55) 9 9609-9851` → links `tel:+5555996099851` e `wa.me/5555996099851`
- Endereço: Marechal Deodoro, nº 1012, Centro, Santiago/RS
- Instagram: `@lpsadvocacia_` → `https://www.instagram.com/lpsadvocacia_/`

---

## 5. Diretrizes de conteúdo (não violar)

- ❌ Depoimentos, avaliações, “o que nossos clientes dizem”
- ❌ Resultados, casos de sucesso, causas ganhas, percentuais, rankings
- ❌ Promessas de vitória ou comparação com outros escritórios
- ❌ “Especialidades” → usar **“Áreas de atuação”**
- ✅ Linguagem clara, acolhedora e verificável (Provimento 205/2021)

`tools/validate.ps1` já bloqueia as principais violações caso reapareçam.

---

## 6. Publicação

1. Envie o conteúdo da pasta (exceto `tools/`) para o hospedeiro via FTP/gerenciador de arquivos.
2. Como cada página tem seu `index.html` dentro da pasta, as URLs ficam limpas automaticamente:
   `/equipe/`, `/areas-de-atuacao/familia/` etc.
3. Após publicar: envie `sitemap.xml` ao Google Search Console e confirme o HTTPS.

---

## 7. Acessibilidade e performance

- Lighthouse: **Acessibilidade 100 · Melhores Práticas 100 · SEO 100**
- Navegação por teclado, foco visível, skip-link, labels nos campos, `alt` descritivo
- Contraste AA verificado; animações respeitam `prefers-reduced-motion`
- Imagens em WebP/JPEG otimizados + `loading="lazy"` (exceto hero)
- Fontes Manrope/Inter com `preconnect` e `display=swap`
