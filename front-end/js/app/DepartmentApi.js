import { ApiClient } from "./ApiClient.js";

/** API client for department endpoints. Provides CRUD operations for departments. */
export class DepartmentApi extends ApiClient {
  /** Fetches all departments (no pagination). @returns {Promise<object[]>} */
  async getAll() {
    return this.get("/api/departments");
  }

  /** Fetches a single department by ID. @param {number} id @returns {Promise<object>} */
  async getById(id) {
    return this.get(`/api/departments/${id}`);
  }

  /** Creates a new department. @param {object} data @returns {Promise<object>} */
  async create(data) {
    return this.post("/api/departments", data);
  }

  /** Updates an existing department. @param {number} id @param {object} data @returns {Promise<object>} */
  async update(id, data) {
    return this.put(`/api/departments/${id}`, data);
  }

  /** Deletes a department. @param {number} id @returns {Promise<object>} */
  async delete(id) {
    return this.delete(`/api/departments/${id}`);
  }
}
