import { CategoryApi } from "../app/CategoryApi.js";
import { ProductApi } from "../app/ProductApi.js";
import { LogApi } from "../app/LogApi.js";
import { UiHelper } from "../app/UiHelper.js";

/** Controller for the home/index page: activity feed, stats, tree, AI suggestions, CSV export. */
class HomePage {
  constructor() {
    this.categoryApi = new CategoryApi();
    this.productApi = new ProductApi();
    this.logApi = new LogApi();
  }

  /** Entry point: binds modal events, SSE activity feed, and action buttons. */
  init() {
    this._bindModals();
    this._bindActivityFeed();
    this._bindButtons();
  }

  /** Wires up the suggestion, classification, and CSV export buttons. */
  _bindButtons() {
    document.getElementById('btnExportCsv')?.addEventListener('click', () => this._exportCsv());
    document.getElementById('btnSuggestCategories')?.addEventListener('click', () => this._suggestCategories());
    document.getElementById('btnClassifyText')?.addEventListener('click', () => this._classifyText());
    document.getElementById('btnSuggestProducts')?.addEventListener('click', () => this._suggestProducts());
  }

  /** Loads stats / category tree when their modals open (lazy load via show.bs.modal). */
  _bindModals() {
    document
      .getElementById("statsModal")
      ?.addEventListener("show.bs.modal", () => this._loadStats());
    document
      .getElementById("treeModal")
      ?.addEventListener("show.bs.modal", () => this._loadTree());
  }

  /**
   * Opens an EventSource to /api/logs/stream and appends incoming activity
   * entries to the #activityFeed container. Limits the feed to 20 items.
   * Updates #activityStatus badge on connection error.
   */
  _bindActivityFeed() {
    const feed = document.getElementById("activityFeed");
    const status = document.getElementById("activityStatus");
    if (!feed) return;

    const source = new EventSource(this.logApi.streamUrl());

    source.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        const time = new Date(data.createdAt).toLocaleTimeString("pt-BR");
        const dotClass =
          data.action === "created"
            ? "bg-success"
            : data.action === "deleted"
              ? "bg-danger"
              : "bg-warning";

        const item = document.createElement("div");
        item.className =
          "d-flex align-items-center gap-2 py-1 border-bottom border-light";
        item.innerHTML =
          `<span class="badge ${dotClass} rounded-pill" style="width:8px;height:8px;padding:0"></span>` +
          `<small><strong>[${time}]</strong> ${data.message}</small>`;

        feed.insertBefore(item, feed.firstChild);
        if (feed.children.length > 20) feed.lastChild.remove();
      } catch (e) {
        /* ignore parse errors */
      }
    };

    source.onerror = () => {
      if (status) {
        status.className = "badge bg-danger";
        status.textContent = "Desconectado";
      }
    };
  }

  /** Fetches category statistics and renders them into #statsBody. */
  async _loadStats() {
    const body = document.getElementById("statsBody");
    UiHelper.showLoading(body);
    try {
      const s = await this.categoryApi.getStats();
      body.innerHTML =
        '<div class="row text-center g-3">' +
        `<div class="col-6"><div class="p-3 bg-light rounded"><h3>${s.total}</h3><small class="text-muted">Total</small></div></div>` +
        `<div class="col-6"><div class="p-3 bg-light rounded"><h3>${s.createdThisMonth}</h3><small class="text-muted">Criadas este mês</small></div></div>` +
        `<div class="col-6"><div class="p-3 bg-light rounded"><h3>${s.updatedToday}</h3><small class="text-muted">Atualizadas hoje</small></div></div>` +
        `<div class="col-6"><div class="p-3 bg-light rounded"><h3>${s.favorites}</h3><small class="text-muted">Favoritas</small></div></div>` +
        "</div>";
    } catch {
      body.innerHTML =
        '<div class="alert alert-danger">Erro ao carregar estatísticas.</div>';
    }
  }

  /** Fetches the category tree and renders it via UiHelper.renderTree into #treeBody. */
  async _loadTree() {
    const body = document.getElementById("treeBody");
    UiHelper.showLoading(body);
    try {
      const tree = await this.categoryApi.getTree();
      if (!tree?.length) {
        body.innerHTML =
          '<p class="text-muted text-center">Nenhuma categoria raiz encontrada.</p>';
        return;
      }
      body.innerHTML = UiHelper.renderTree(tree);
    } catch {
      body.innerHTML =
        '<div class="alert alert-danger">Erro ao carregar árvore.</div>';
    }
  }

  /** Sends a topic to the AI suggest endpoint and displays returned category names as badges. */
  async _suggestCategories() {
    const input = document.getElementById("suggestInput");
    const result = document.getElementById("suggestResult");
    const topic = input.value.trim();
    if (!topic) {
      result.innerHTML =
        '<div class="alert alert-warning">Digite um tema.</div>';
      return;
    }

    result.innerHTML =
      '<div class="text-center"><div class="spinner-border spinner-border-sm"></div> Consultando IA...</div>';
    try {
      const data = await this.categoryApi.suggest(topic);
      const list = data.suggestions || [];
      if (!list.length) {
        result.innerHTML =
          '<div class="alert alert-info">Nenhuma sugestão retornada (IA pode não estar configurada).</div>';
        return;
      }
      result.innerHTML =
        '<div class="d-flex flex-wrap gap-2">' +
        list
          .map((s) => `<span class="badge bg-primary fs-6">${s}</span>`)
          .join("") +
        "</div>";
    } catch {
      result.innerHTML =
        '<div class="alert alert-danger">Erro ao consultar IA.</div>';
    }
  }

  /** Sends a category name to the product suggest endpoint and displays returned product names. */
  async _suggestProducts() {
    const input = document.getElementById("suggestProductInput");
    const result = document.getElementById("suggestProductResult");
    const category = input.value.trim();
    if (!category) {
      result.innerHTML =
        '<div class="alert alert-warning">Digite uma categoria.</div>';
      return;
    }

    result.innerHTML =
      '<div class="text-center"><div class="spinner-border spinner-border-sm"></div> Consultando IA...</div>';
    try {
      const data = await this.productApi.suggest(category);
      const list = data.suggestions || [];
      if (!list.length) {
        result.innerHTML =
          '<div class="alert alert-info">Nenhuma sugestão retornada (IA pode não estar configurada).</div>';
        return;
      }
      result.innerHTML =
        '<div class="d-flex flex-wrap gap-2">' +
        list
          .map((s) => `<span class="badge bg-primary fs-6">${s}</span>`)
          .join("") +
        "</div>";
    } catch {
      result.innerHTML =
        '<div class="alert alert-danger">Erro ao consultar IA.</div>';
    }
  }

  /** Sends a text to the classify endpoint and displays the suggested category name. */
  async _classifyText() {
    const input = document.getElementById("classifyInput");
    const result = document.getElementById("classifyResult");
    const text = input.value.trim();
    if (!text) {
      result.innerHTML =
        '<div class="alert alert-warning">Digite um texto.</div>';
      return;
    }

    result.innerHTML =
      '<div class="text-center"><div class="spinner-border spinner-border-sm"></div> Classificando...</div>';
    try {
      const data = await this.categoryApi.classify(text);
      if (data.category) {
        result.innerHTML = `<div class="alert alert-success">Categoria sugerida: <strong>${data.category}</strong></div>`;
      } else {
        result.innerHTML = `<div class="alert alert-info">${data.message || "Não foi possível classificar."}</div>`;
      }
    } catch {
      result.innerHTML =
        '<div class="alert alert-danger">Erro ao classificar.</div>';
    }
  }

  /** Redirects the browser to the CSV export endpoint, triggering a download. */
  _exportCsv() {
    window.location.href = this.categoryApi.exportCsvUrl();
  }
}

document.addEventListener("DOMContentLoaded", () => new HomePage().init());
