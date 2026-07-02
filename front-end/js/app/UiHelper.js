/** Static UI utilities for common interactions: modals, toasts, rendering helpers. */
export class UiHelper {
  /**
   * Shows a SweetAlert2 confirmation dialog.
   * @param {string} [message="Tem certeza?"] - Confirmation message.
   * @returns {Promise<boolean>} True if user confirmed.
   */
  static async confirmDelete(message = "Tem certeza?") {
    const { value: ok } = await Swal.fire({
      icon: "warning",
      title: "Confirmar exclusão",
      text: message,
      showCancelButton: true,
      confirmButtonColor: "#d33",
      confirmButtonText: "Sim, excluir!",
      cancelButtonText: "Cancelar",
    });
    return ok;
  }

  /**
   * Shows a brief SweetAlert2 toast notification.
   * @param {"success"|"error"|"warning"|"info"} icon - Toast icon type.
   * @param {string} title - Toast title/message.
   * @param {number} [timer=2000] - Auto-dismiss time in ms.
   */
  static toast(icon, title, timer = 2000) {
    Swal.fire({ icon, title, timer, showConfirmButton: false });
  }

  /** Shows an error alert with a given message. @param {string} msg */
  static error(msg) {
    Swal.fire({ icon: "error", title: "Erro", text: msg });
  }

  /** Replaces element content with a centered spinner. @param {HTMLElement} target */
  static showLoading(target) {
    target.innerHTML =
      '<div class="text-center py-3"><div class="spinner-border" role="status"></div></div>';
  }

  /**
   * Recursively renders a category tree as nested <ul>/<li>.
   * @param {object[]} nodes - Tree nodes with name, departmentName, and optional children.
   * @param {number} [depth=0] - Current recursion depth (used for indentation).
   * @returns {string} HTML string of the tree.
   */
  static renderTree(nodes, depth = 0) {
    let html = `<ul class="list-unstyled ms-${depth * 3}">`;
    for (const n of nodes) {
      html += `<li class="py-1"><span class="badge bg-secondary me-2">${n.departmentName || n.department || "-"}</span> ${n.name}`;
      if (n.children?.length) html += this.renderTree(n.children, depth + 1);
      html += "</li>";
    }
    html += "</ul>";
    return html;
  }

  /**
   * Builds a Bootstrap pagination nav with click handlers.
   * @param {{page: number, totalPages: number}} pagination - Current page state.
   * @param {function} loadFn - Callback receiving the target page number.
   * @returns {HTMLElement} A <nav> element containing the pagination <ul>.
   */
  static paginationHtml({ page, totalPages }, loadFn) {
    let html = `<li class="page-item ${page <= 1 ? "disabled" : ""}"><button class="page-link" data-page="${page - 1}">Anterior</button></li>`;
    for (let i = 1; i <= totalPages; i++) {
      html += `<li class="page-item ${i === page ? "active" : ""}"><button class="page-link" data-page="${i}">${i}</button></li>`;
    }
    html += `<li class="page-item ${page >= totalPages ? "disabled" : ""}"><button class="page-link" data-page="${page + 1}">Próximo</button></li>`;

    const nav = document.createElement("nav");
    nav.innerHTML = `<ul class="pagination justify-content-center">${html}</ul>`;
    nav.querySelectorAll("[data-page]").forEach((btn) => {
      btn.addEventListener("click", () => loadFn(parseInt(btn.dataset.page)));
    });
    return nav;
  }

  /** Formats an ISO date string to Brazilian locale. @param {string} dateStr @returns {string} */
  static formatDate(dateStr) {
    return new Date(dateStr).toLocaleDateString("pt-BR");
  }

  /** Formats a numeric price as BRL currency string. @param {number} value @returns {string} */
  static formatPrice(value) {
    return `R$ ${value.toFixed(2).replace(".", ",")}`;
  }

  /**
   * Renders an array of tags as Bootstrap badges.
   * @param {string[]} tags - Array of tag strings.
   * @returns {string} HTML string of badges or a placeholder.
   */
  static tagsHtml(tags) {
    if (!tags?.length) return '<small class="text-muted">-</small>';
    return tags
      .map((t) => `<span class="badge bg-secondary me-1">${t}</span>`)
      .join("");
  }

  /** Returns a heart emoji based on favorite state. @param {boolean} isFavorite @returns {string} */
  static favIcon(isFavorite) {
    return isFavorite ? "\u2764\uFE0F" : "\uD83E\uDD0D";
  }
}
