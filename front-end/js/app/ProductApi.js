import { ApiClient } from "./ApiClient.js";

/** API client for product endpoints. Extends ApiClient with domain-specific methods. */
export class ProductApi extends ApiClient {
  /**
   * Fetches paginated, filterable product list.
   * @param {object} opts - Query parameters.
   * @param {string} [opts.search] - Text search filter.
   * @param {string} [opts.sort="date"] - Sort field.
   * @param {string} [opts.order="desc"] - Sort direction.
   * @param {number} [opts.page=1] - Page number.
   * @param {number} [opts.limit=10] - Items per page.
   * @returns {Promise<{data: object[], total: number, page: number, limit: number}>}
   */
  async getAll({
    search,
    sort = "date",
    order = "desc",
    page = 1,
    limit = 10,
  } = {}) {
    let url = `/api/products?page=${page}&limit=${limit}&sort=${sort}&order=${order}`;
    if (search) url += `&search=${encodeURIComponent(search)}`;
    return this.get(url);
  }

  /** Fetches a single product by ID. @param {number} id @returns {Promise<object>} */
  async getById(id) {
    return this.get(`/api/products/${id}`);
  }

  /** Creates a new product. @param {object} data - Product payload. @returns {Promise<object>} */
  async create(data) {
    return this.post("/api/products", data);
  }

  /** Updates an existing product. @param {number} id @param {object} data @returns {Promise<object>} */
  async update(id, data) {
    return this.put(`/api/products/${id}`, data);
  }

  /** Soft-deletes a product. @param {number} id @returns {Promise<object>} */
  async delete(id) {
    return this.delete(`/api/products/${id}`);
  }

  /** Toggles the favorite flag on a product. @param {number} id @returns {Promise<object>} */
  async toggleFavorite(id) {
    return this.post(`/api/products/${id}/favorite`);
  }

  /** Fetches all favorited products. @returns {Promise<object[]>} */
  async getFavorites() {
    return this.get("/api/products/favorites");
  }

  /** Generates a product description via AI (POST). @param {string} name @returns {Promise<object>} */
  async generateDescription(name) {
    return this.post("/api/products/generate-description", { name });
  }

  /** Returns the SSE streaming URL for product description generation. @param {string} name @returns {string} */
  descriptionStreamUrl(name) {
    return `/api/products/generate-description-stream?name=${encodeURIComponent(name)}`;
  }

  /** Corrects a product name via AI. @param {string} text @returns {Promise<object>} */
  async correctName(text) {
    return this.post("/api/products/correct-name", { text });
  }

  /** Classifies a text into an existing category via AI. @param {string} text @returns {Promise<object>} */
  async classify(text) {
    return this.post("/api/products/classify", { text });
  }

  /** Suggests product names for a given category via AI. @param {string} categoryName @returns {Promise<{suggestions: string[]}>} */
  async suggest(categoryName) {
    return this.post("/api/products/suggest", { categoryName });
  }
}
