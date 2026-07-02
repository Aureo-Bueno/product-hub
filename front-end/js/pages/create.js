import { CategoryApi } from "../app/CategoryApi.js";
import { ProductApi } from "../app/ProductApi.js";
import { DepartmentApi } from "../app/DepartmentApi.js";
import { UiHelper } from "../app/UiHelper.js";

/** Controller for the create/cadastro page: forms for category, department, and product creation. */
class CreatePage {
  constructor() {
    this.categoryApi = new CategoryApi();
    this.productApi = new ProductApi();
    this.departmentApi = new DepartmentApi();
    /** @type {EventSource|null} Active SSE connection for category description generation. */
    this.catEventSource = null;
    /** @type {EventSource|null} Active SSE connection for product description generation. */
    this.prodEventSource = null;
  }

  /** Entry point: loads dropdown options and wires up all form/button event listeners. */
  init() {
    this._loadDepartments();
    this._loadParentCategories();
    this._loadProductCategories();

    document.getElementById("categoryForm")?.addEventListener("submit", (e) => {
      e.preventDefault();
      this._createCategory();
    });
    document
      .getElementById("departmentForm")
      ?.addEventListener("submit", (e) => {
        e.preventDefault();
        this._createDepartment();
      });
    document.getElementById("productForm")?.addEventListener("submit", (e) => {
      e.preventDefault();
      this._createProduct();
    });

    document.getElementById("btnGenCatDesc")?.addEventListener("click", (e) => { e.preventDefault(); this._generateCatDescription(); });
    document.getElementById("btnGenProdDesc")?.addEventListener("click", (e) => { e.preventDefault(); this._generateProdDescription(); });
    document.getElementById("btnCorrectCatName")?.addEventListener("click", (e) => { e.preventDefault(); this._correctCatName(); });
    document.getElementById("btnCorrectProdName")?.addEventListener("click", (e) => { e.preventDefault(); this._correctProdName(); });
    document.getElementById("btnCheckDuplicate")?.addEventListener("click", (e) => { e.preventDefault(); this._checkDuplicate(); });
  }

  /** Populates the category department dropdown from the API. */
  async _loadDepartments() {
    try {
      const depts = await this.departmentApi.getAll();
      const sel = document.getElementById("catDepartmentId");
      sel.innerHTML = '<option value="">Selecione...</option>';
      depts.forEach((d) => {
        sel.appendChild(new Option(d.name, d.id));
      });
    } catch {
      /* ignore */
    }
  }

  /** Populates the parent category dropdown from the API. */
  async _loadParentCategories() {
    try {
      const res = await this.categoryApi.getAll({
        limit: 200,
        sort: "name",
        order: "asc",
      });
      const select = document.getElementById("catParentId");
      const categories = res.data || res;
      categories.forEach((c) => {
        select.appendChild(new Option(c.name, c.id));
      });
    } catch {
      /* ignore */
    }
  }

  /** Populates the product category dropdown from the API. */
  async _loadProductCategories() {
    try {
      const res = await this.categoryApi.getAll({
        limit: 200,
        sort: "name",
        order: "asc",
      });
      const select = document.getElementById("prodCategoryId");
      const categories = res.data || res;
      categories.forEach((c) => {
        select.appendChild(new Option(c.name, c.id));
      });
    } catch {
      /* ignore */
    }
  }

  /**
   * Validates the category form fields.
   * @returns {string[]} Array of error messages. Empty if valid.
   */
  _validateCategory() {
    const name = document.getElementById("catName").value.trim();
    const dateCreate = document.getElementById("catDateCreate").value;
    const departmentId = document.getElementById("catDepartmentId").value;
    const acept = document.getElementById("catAcept").checked;
    const errors = [];

    if (!name || name.length < 5)
      errors.push("Nome deve ter pelo menos 5 caracteres.");
    if (!dateCreate) errors.push("Selecione a data!");
    else {
      const [y, m, d] = dateCreate.split("-").map(Number);
      const now = new Date();
      if (y !== now.getFullYear()) errors.push("O ano deve ser atual!");
      if (new Date(y, m - 1, d) > now) errors.push("Data não pode ser futura!");
    }
    if (!departmentId) errors.push("Selecione um departamento!");
    if (!acept) errors.push("Aceite os termos!");

    return errors;
  }

  /** Collects form data, validates, and creates a category via the API. */
  async _createCategory() {
    const errors = this._validateCategory();
    if (errors.length) {
      Swal.fire({
        icon: "error",
        title: "Erro de validação",
        html: errors.join("<br>"),
      });
      return;
    }

    const name = document.getElementById("catName").value.trim();
    const description =
      document.getElementById("catDescription").value.trim() || null;
    const dateCreate = document.getElementById("catDateCreate").value;
    const departmentId = parseInt(
      document.getElementById("catDepartmentId").value,
    );
    const parentId = document.getElementById("catParentId").value;
    const tags = document
      .getElementById("catTags")
      .value.split(",")
      .map((t) => t.trim())
      .filter(Boolean);

    const data = {
      name,
      description,
      dateCreate,
      departmentId,
      tags: tags.length ? tags : null,
    };
    if (parentId) data.parentId = parseInt(parentId);

    try {
      await this.categoryApi.create(data);
      UiHelper.toast("success", "Categoria cadastrada!");
      document.getElementById("categoryForm").reset();
      document.getElementById("duplicateResult").innerHTML = "";
    } catch (err) {
      Swal.fire({ icon: "error", title: "Erro", text: err.message });
    }
  }

  /** Collects form data and creates a department via the API, then refreshes the dropdown. */
  async _createDepartment() {
    const name = document.getElementById("deptName").value.trim();
    if (!name)
      return Swal.fire({
        icon: "warning",
        title: "Digite o nome do departamento",
      });

    try {
      await this.departmentApi.create({ name });
      UiHelper.toast("success", "Departamento criado!");
      document.getElementById("departmentForm").reset();
      this._loadDepartments();
    } catch (err) {
      Swal.fire({ icon: "error", title: "Erro", text: err.message });
    }
  }

  /**
   * Validates the product form fields.
   * @returns {string[]} Array of error messages. Empty if valid.
   */
  _validateProduct() {
    const name = document.getElementById("prodName").value.trim();
    const categoryId = document.getElementById("prodCategoryId").value;
    const dateCreate = document.getElementById("prodDateCreate").value;
    const errors = [];

    if (!name || name.length < 3)
      errors.push("Nome deve ter pelo menos 3 caracteres.");
    if (!categoryId) errors.push("Selecione uma categoria!");
    if (!dateCreate) errors.push("Selecione a data!");
    else {
      const [y] = dateCreate.split("-").map(Number);
      if (y !== new Date().getFullYear()) errors.push("O ano deve ser atual!");
    }
    return errors;
  }

  /** Collects form data, validates, and creates a product via the API. */
  async _createProduct() {
    const errors = this._validateProduct();
    if (errors.length) {
      Swal.fire({
        icon: "error",
        title: "Erro de validação",
        html: errors.join("<br>"),
      });
      return;
    }

    const name = document.getElementById("prodName").value.trim();
    const price = parseFloat(document.getElementById("prodPrice").value) || 0;
    const description =
      document.getElementById("prodDescription").value.trim() || null;
    const categoryId = parseInt(
      document.getElementById("prodCategoryId").value,
    );
    const dateCreate = document.getElementById("prodDateCreate").value;
    const tags = document
      .getElementById("prodTags")
      .value.split(",")
      .map((t) => t.trim())
      .filter(Boolean);

    try {
      await this.productApi.create({
        name,
        price,
        description,
        categoryId,
        dateCreate,
        tags: tags.length ? tags : null,
      });
      UiHelper.toast("success", "Produto cadastrado!");
      document.getElementById("productForm").reset();
    } catch (err) {
      Swal.fire({ icon: "error", title: "Erro", text: err.message });
    }
  }

  /**
   * Opens an SSE connection to stream an AI-generated description for the category name.
   * Appends each text chunk to the description textarea. Closes on [DONE] or error.
   */
  _generateCatDescription() {
    const name = document.getElementById("catName").value.trim();
    if (!name)
      return Swal.fire({ icon: "warning", title: "Digite o nome primeiro" });

    if (this.catEventSource) this.catEventSource.close();

    const textarea = document.getElementById("catDescription");
    textarea.value = "";
    textarea.placeholder = "Gerando descrição...";

    this.catEventSource = new EventSource(
      this.categoryApi.descriptionStreamUrl(name),
    );
    this.catEventSource.onmessage = (event) => {
      if (event.data === "[DONE]") {
        this.catEventSource.close();
        this.catEventSource = null;
        textarea.placeholder = "Descrição da categoria...";
        UiHelper.toast("success", "Descrição gerada!", 1500);
        return;
      }
      try {
        const data = JSON.parse(event.data);
        if (data.text) textarea.value += data.text;
      } catch {
        /* ignore */
      }
    };
    this.catEventSource.onerror = () => {
      this.catEventSource.close();
      this.catEventSource = null;
      textarea.placeholder = "Descrição da categoria...";
      if (!textarea.value)
        Swal.fire({ icon: "error", title: "Erro ao gerar descrição" });
    };
  }

  /**
   * Opens an SSE connection to stream an AI-generated description for the product name.
   * Appends each text chunk to the description textarea. Closes on [DONE] or error.
   */
  _generateProdDescription() {
    const name = document.getElementById("prodName").value.trim();
    if (!name)
      return Swal.fire({ icon: "warning", title: "Digite o nome primeiro" });

    if (this.prodEventSource) this.prodEventSource.close();

    const textarea = document.getElementById("prodDescription");
    textarea.value = "";
    textarea.placeholder = "Gerando descrição...";

    this.prodEventSource = new EventSource(
      this.productApi.descriptionStreamUrl(name),
    );
    this.prodEventSource.onmessage = (event) => {
      if (event.data === "[DONE]") {
        this.prodEventSource.close();
        this.prodEventSource = null;
        textarea.placeholder = "Descrição do produto...";
        UiHelper.toast("success", "Descrição gerada!", 1500);
        return;
      }
      try {
        const data = JSON.parse(event.data);
        if (data.text) textarea.value += data.text;
      } catch {
        /* ignore */
      }
    };
    this.prodEventSource.onerror = () => {
      this.prodEventSource.close();
      this.prodEventSource = null;
      textarea.placeholder = "Descrição do produto...";
      if (!textarea.value)
        Swal.fire({ icon: "error", title: "Erro ao gerar descrição" });
    };
  }

  /** Sends the category name to the AI correct endpoint and updates the input field. */
  async _correctCatName() {
    const name = document.getElementById("catName").value.trim();
    if (!name) return;
    try {
      const data = await this.categoryApi.correctName(name);
      if (data.corrected)
        document.getElementById("catName").value = data.corrected;
    } catch {
      /* ignore */
    }
  }

  /** Sends the product name to the AI correct endpoint and updates the input field. */
  async _correctProdName() {
    const name = document.getElementById("prodName").value.trim();
    if (!name) return;
    try {
      const data = await this.productApi.correctName(name);
      if (data.corrected)
        document.getElementById("prodName").value = data.corrected;
    } catch {
      /* ignore */
    }
  }

  /** Checks if the category name already exists via the API and shows the result below the input. */
  async _checkDuplicate() {
    const name = document.getElementById("catName").value.trim();
    const result = document.getElementById("duplicateResult");
    if (!name) {
      result.innerHTML = '<span class="text-warning">Digite o nome</span>';
      return;
    }

    try {
      const data = await this.categoryApi.checkDuplicate(name);
      result.innerHTML = data.isDuplicate
        ? `<span class="text-danger">Já existe: ${data.category.name}</span>`
        : '<span class="text-success">Nome disponível</span>';
    } catch {
      result.innerHTML = '<span class="text-warning">Erro ao verificar</span>';
    }
  }
}

document.addEventListener("DOMContentLoaded", () => new CreatePage().init());
