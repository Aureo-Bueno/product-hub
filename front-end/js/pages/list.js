import { CategoryApi } from "../app/CategoryApi.js";
import { ProductApi } from "../app/ProductApi.js";
import { DepartmentApi } from "../app/DepartmentApi.js";
import { UiHelper } from "../app/UiHelper.js";

/** Controller for the list/listagem page: tabs for categories, departments, and products with CRUD. */
class ListPage {
  constructor() {
    this.categoryApi = new CategoryApi();
    this.productApi = new ProductApi();
    this.departmentApi = new DepartmentApi();

    /** @type {number} Current page for category pagination. */
    this.catPage = 1;
    /** @type {number} Total pages for category pagination. */
    this.catTotalPages = 1;
    /** @type {boolean} Whether only favorite categories should be shown. */
    this.catFavOnly = false;

    /** @type {number} Current page for product pagination. */
    this.prodPage = 1;
    /** @type {number} Total pages for product pagination. */
    this.prodTotalPages = 1;
    /** @type {boolean} Whether only favorite products should be shown. */
    this.prodFavOnly = false;
  }

  /** Entry point: reads URL params, activates the requested tab, loads initial data. */
  init() {
    const params = new URLSearchParams(window.location.search);
    const tab = params.get("tab") || "categorias";
    this.catFavOnly = params.get("filter") === "favorites";

    const tabMap = { categorias: 0, departments: 1, products: 2 };
    const idx = tabMap[tab] || 0;
    const tabs = document.querySelectorAll("#listTab .nav-link");
    if (tabs[idx]) new bootstrap.Tab(tabs[idx]).show();

    document
      .getElementById("listTab")
      ?.addEventListener("shown.bs.tab", (e) => {
        if (e.target.id === "dept-tab") this._loadDepartments();
      });

    this._bindEvents();
    this._loadCategories();
    this._loadDepartments();
    this._loadProducts();
  }

  /** Wires up search, sort, and favorite toggle events for all tabs. */
  _bindEvents() {
    document.getElementById("catSearch")?.addEventListener("keyup", (e) => {
      if (e.key && e.key !== "Enter") return;
      this.catPage = 1;
      this._loadCategories();
    });
    document.getElementById("catSearchBtn")?.addEventListener("click", () => {
      this.catPage = 1;
      this._loadCategories();
    });
    document.getElementById("catSort")?.addEventListener("change", () => this._loadCategories());

    document.getElementById("prodSearch")?.addEventListener("keyup", (e) => {
      if (e.key && e.key !== "Enter") return;
      this.prodPage = 1;
      this._loadProducts();
    });
    document.getElementById("prodSearchBtn")?.addEventListener("click", () => {
      this.prodPage = 1;
      this._loadProducts();
    });
    document.getElementById("prodSort")?.addEventListener("change", () => this._loadProducts());
    document.getElementById("prodFavBtn")?.addEventListener("click", () => {
      this.prodFavOnly = !this.prodFavOnly;
      this.prodPage = 1;
      this._loadProducts();
    });
  }

  /**
   * Reads the sort field and direction from a sort <select> (value format: "field|order").
   * @param {string} prefix - Select element ID prefix (e.g. "cat" or "prod").
   * @returns {string[]} [sortField, sortOrder]
   */
  _getSortValues(prefix) {
    const val = document.getElementById(`${prefix}Sort`).value;
    return val.split("|");
  }

  /**
   * Loads categories (paginated or favorites) and renders the table.
   * @param {number} [page] - Target page number (updates internal state).
   */
  async _loadCategories(page) {
    if (page) this.catPage = page;
    const search = document.getElementById("catSearch").value.trim();
    const [sort, order] = this._getSortValues("cat");

    const tbody = document.getElementById("catTableBody");
    UiHelper.showLoading(tbody);
    tbody.innerHTML =
      '<tr><td colspan="7" class="text-center py-4"><div class="spinner-border"></div></td></tr>';

    try {
      if (this.catFavOnly) {
        const data = await this.categoryApi.getFavorites();
        this._renderCatTable(data, {
          total: data.length,
          totalPages: 1,
          page: 1,
        });
      } else {
        const res = await this.categoryApi.getAll({
          search,
          sort,
          order,
          page: this.catPage,
          limit: 10,
        });
        this._renderCatTable(res.data, res);
      }
    } catch {
      tbody.innerHTML =
        '<tr><td colspan="7" class="text-center text-danger">Erro ao carregar</td></tr>';
    }
  }

  /**
   * Renders category rows into the table and attaches favorite/delete click handlers.
   * @param {object[]} categories - Category list.
   * @param {{page: number, totalPages: number}} pagination - Pagination metadata.
   */
  _renderCatTable(categories, pagination) {
    const tbody = document.getElementById("catTableBody");
    const pag = document.getElementById("catPagination");
    tbody.innerHTML = "";

    if (!categories?.length) {
      tbody.innerHTML =
        '<tr><td colspan="7" class="text-center text-muted py-4">Nenhuma categoria encontrada.</td></tr>';
      pag.innerHTML = "";
      return;
    }

    categories.forEach((c) => {
      const tr = document.createElement("tr");
      tr.innerHTML =
        `<td class="fw-semibold">${c.name}</td>` +
        `<td><span class="badge bg-info text-dark">${c.departmentName || "-"}</span></td>` +
        `<td><small class="text-muted">${c.description ? c.description.substring(0, 50) + (c.description.length > 50 ? "..." : "") : "-"}</small></td>` +
        `<td>${UiHelper.tagsHtml(c.tags)}</td>` +
        `<td><small>${UiHelper.formatDate(c.dateCreate)}</small></td>` +
        `<td class="text-center"><span style="cursor:pointer" class="cat-fav" data-id="${c.id}">${UiHelper.favIcon(c.isFavorite)}</span></td>` +
        `<td class="text-center"><button class="btn btn-danger btn-sm cat-delete" data-id="${c.id}">Excluir</button></td>`;
      tbody.appendChild(tr);
    });

    tbody.querySelectorAll(".cat-fav").forEach((el) => {
      el.addEventListener("click", () =>
        this._toggleCatFav(parseInt(el.dataset.id)),
      );
    });
    tbody.querySelectorAll(".cat-delete").forEach((el) => {
      el.addEventListener("click", () =>
        this._deleteCat(parseInt(el.dataset.id)),
      );
    });

    pag.innerHTML = "";
    if (pagination?.totalPages > 1) {
      this.catTotalPages = pagination.totalPages;
      pag.appendChild(
        UiHelper.paginationHtml(pagination, (p) => this._loadCategories(p)),
      );
    }
  }

  /** Toggles the favorite flag on a category and reloads the table. @param {number} id */
  async _toggleCatFav(id) {
    try {
      await this.categoryApi.toggleFavorite(id);
      this._loadCategories(this.catPage);
    } catch {
      Swal.fire({ icon: "error", title: "Erro ao alternar favorito" });
    }
  }

  /** Soft-deletes a category after confirmation and reloads. @param {number} id */
  async _deleteCat(id) {
    const ok = await UiHelper.confirmDelete(
      "Tem certeza? A categoria será desativada (soft delete).",
    );
    if (!ok) return;
    try {
      await this.categoryApi.delete(id);
      UiHelper.toast("success", "Excluída!", 1500);
      this._loadCategories(this.catPage);
    } catch {
      Swal.fire({ icon: "error", title: "Erro ao excluir" });
    }
  }

  /** Loads departments and renders the table. */
  async _loadDepartments() {
    const tbody = document.getElementById("deptTableBody");
    UiHelper.showLoading(tbody);

    try {
      const depts = await this.departmentApi.getAll();
      tbody.innerHTML = "";
      if (!depts?.length) {
        tbody.innerHTML =
          '<tr><td colspan="3" class="text-center text-muted py-4">Nenhum departamento encontrado.</td></tr>';
        return;
      }
      depts.forEach((d) => {
        const tr = document.createElement("tr");
        tr.innerHTML =
          `<td>${d.id}</td><td class="fw-semibold">${d.name}</td>` +
          `<td class="text-center"><button class="btn btn-danger btn-sm dept-delete" data-id="${d.id}">Excluir</button></td>`;
        tbody.appendChild(tr);
      });
      tbody.querySelectorAll(".dept-delete").forEach((el) => {
        el.addEventListener("click", () =>
          this._deleteDept(parseInt(el.dataset.id)),
        );
      });
    } catch {
      tbody.innerHTML =
        '<tr><td colspan="3" class="text-center text-danger">Erro ao carregar</td></tr>';
    }
  }

  /** Deletes a department after confirmation and reloads the table. @param {number} id */
  async _deleteDept(id) {
    const ok = await UiHelper.confirmDelete(
      "Departamentos com categorias vinculadas não podem ser excluídos.",
    );
    if (!ok) return;
    try {
      await this.departmentApi.delete(id);
      UiHelper.toast("success", "Excluído!", 1500);
      this._loadDepartments();
    } catch (err) {
      Swal.fire({ icon: "error", title: "Erro", text: err.message });
    }
  }

  /**
   * Loads products (paginated or favorites) and renders the table.
   * @param {number} [page] - Target page number (updates internal state).
   */
  async _loadProducts(page) {
    if (page) this.prodPage = page;
    const search = document.getElementById("prodSearch").value.trim();
    const [sort, order] = this._getSortValues("prod");

    const tbody = document.getElementById("prodTableBody");
    tbody.innerHTML =
      '<tr><td colspan="8" class="text-center py-4"><div class="spinner-border"></div></td></tr>';

    try {
      if (this.prodFavOnly) {
        const data = await this.productApi.getFavorites();
        this._renderProdTable(data, {
          total: data.length,
          totalPages: 1,
          page: 1,
        });
      } else {
        const res = await this.productApi.getAll({
          search,
          sort,
          order,
          page: this.prodPage,
          limit: 10,
        });
        this._renderProdTable(res.data, res);
      }
    } catch {
      tbody.innerHTML =
        '<tr><td colspan="8" class="text-center text-danger">Erro ao carregar</td></tr>';
    }
  }

  /**
   * Renders product rows into the table and attaches favorite/delete click handlers.
   * @param {object[]} products - Product list.
   * @param {{page: number, totalPages: number}} pagination - Pagination metadata.
   */
  _renderProdTable(products, pagination) {
    const tbody = document.getElementById("prodTableBody");
    const pag = document.getElementById("prodPagination");
    tbody.innerHTML = "";

    if (!products?.length) {
      tbody.innerHTML =
        '<tr><td colspan="8" class="text-center text-muted py-4">Nenhum produto encontrado.</td></tr>';
      pag.innerHTML = "";
      return;
    }

    products.forEach((p) => {
      const tr = document.createElement("tr");
      tr.innerHTML =
        `<td class="fw-semibold">${p.name}</td>` +
        `<td><strong>${UiHelper.formatPrice(p.price)}</strong></td>` +
        `<td><span class="badge bg-info text-dark">${p.categoryName || "-"}</span></td>` +
        `<td><small class="text-muted">${p.departmentName || "-"}</small></td>` +
        `<td>${UiHelper.tagsHtml(p.tags)}</td>` +
        `<td><small>${UiHelper.formatDate(p.dateCreate)}</small></td>` +
        `<td class="text-center"><span style="cursor:pointer" class="prod-fav" data-id="${p.id}">${UiHelper.favIcon(p.isFavorite)}</span></td>` +
        `<td class="text-center"><button class="btn btn-danger btn-sm prod-delete" data-id="${p.id}">Excluir</button></td>`;
      tbody.appendChild(tr);
    });

    tbody.querySelectorAll(".prod-fav").forEach((el) => {
      el.addEventListener("click", () =>
        this._toggleProdFav(parseInt(el.dataset.id)),
      );
    });
    tbody.querySelectorAll(".prod-delete").forEach((el) => {
      el.addEventListener("click", () =>
        this._deleteProd(parseInt(el.dataset.id)),
      );
    });

    pag.innerHTML = "";
    if (pagination?.totalPages > 1) {
      this.prodTotalPages = pagination.totalPages;
      pag.appendChild(
        UiHelper.paginationHtml(pagination, (p) => this._loadProducts(p)),
      );
    }
  }

  /** Toggles the favorite flag on a product and reloads. @param {number} id */
  async _toggleProdFav(id) {
    try {
      await this.productApi.toggleFavorite(id);
      this._loadProducts(this.prodPage);
    } catch {
      Swal.fire({ icon: "error", title: "Erro ao alternar favorito" });
    }
  }

  /** Soft-deletes a product after confirmation and reloads. @param {number} id */
  async _deleteProd(id) {
    const ok = await UiHelper.confirmDelete(
      "Tem certeza? O produto será desativado (soft delete).",
    );
    if (!ok) return;
    try {
      await this.productApi.delete(id);
      UiHelper.toast("success", "Excluído!", 1500);
      this._loadProducts(this.prodPage);
    } catch {
      Swal.fire({ icon: "error", title: "Erro ao excluir" });
    }
  }
}

document.addEventListener("DOMContentLoaded", () => new ListPage().init());
