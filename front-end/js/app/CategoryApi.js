import { ApiClient } from "./ApiClient.js";

/** API client for category endpoints. Extends ApiClient with domain-specific methods. */
export class CategoryApi extends ApiClient {
  /**
   * Fetches paginated, filterable category list.
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
    let url = `/api/categories?page=${page}&limit=${limit}&sort=${sort}&order=${order}`;
    if (search) url += `&search=${encodeURIComponent(search)}`;
    return this.get(url);
  }

  /** Fetches a single category by ID. @param {number} id @returns {Promise<object>} */
  async getById(id) {
    return this.get(`/api/categories/${id}`);
  }

  /** Creates a new category. @param {object} data - Category payload. @returns {Promise<object>} */
  async create(data) {
    return this.post("/api/categories", data);
  }

  /** Updates an existing category. @param {number} id @param {object} data @returns {Promise<object>} */
  async update(id, data) {
    return this.put(`/api/categories/${id}`, data);
  }

  /** Soft-deletes a category. @param {number} id @returns {Promise<object>} */
  async delete(id) {
    return this.delete(`/api/categories/${id}`);
  }

  /** Toggles the favorite flag on a category. @param {number} id @returns {Promise<object>} */
  async toggleFavorite(id) {
    return this.post(`/api/categories/${id}/favorite`);
  }

  /** Fetches all favorited categories. @returns {Promise<object[]>} */
  async getFavorites() {
    return this.get("/api/categories/favorites");
  }

  /** Fetches the category hierarchy as a tree. @returns {Promise<object[]>} */
  async getTree() {
    return this.get("/api/categories/tree");
  }

  /** Fetches category statistics (total, created this month, updated today, favorites). @returns {Promise<object>} */
  async getStats() {
    return this.get("/api/categories/stats");
  }

  /** Returns the CSV export URL (triggers file download via browser). @returns {string} */
  exportCsvUrl() {
    return "/api/categories/export";
  }

  /** Generates a category description via AI (POST). @param {string} name @returns {Promise<object>} */
  async generateDescription(name) {
    return this.post("/api/categories/generate-description", { name });
  }

  /** Returns the SSE streaming URL for category description generation. @param {string} name @returns {string} */
  descriptionStreamUrl(name) {
    return `/api/categories/generate-description-stream?name=${encodeURIComponent(name)}`;
  }

  /** Suggests category names for a given topic via AI. @param {string} topic @returns {Promise<{suggestions: string[]}>} */
  async suggest(topic) {
    return this.post("/api/categories/suggest", { prompt: topic });
  }

  /** Corrects a category name via AI. @param {string} name @returns {Promise<{corrected: string}>} */
  async correctName(name) {
    return this.post("/api/categories/correct-name", { prompt: name });
  }

  /** Checks if a category name already exists (duplicate detection). @param {string} name @returns {Promise<{isDuplicate: boolean, category?: object}>} */
  async checkDuplicate(name) {
    return this.post("/api/categories/check-duplicate", { name });
  }

  /** Classifies a text into an existing category via AI. @param {string} text @returns {Promise<{category?: string, message?: string}>} */
  async classify(text) {
    return this.post("/api/categories/classify", { text });
  }
}
