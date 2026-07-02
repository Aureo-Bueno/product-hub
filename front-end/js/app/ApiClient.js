/** Base HTTP client for REST API calls. Handles JSON serialization and error extraction. */
export class ApiClient {
  /**
   * Sends an HTTP request with JSON content-type.
   * @param {string} endpoint - The request URL.
   * @param {object} [options={}] - Fetch options (method, body, headers overrides).
   * @returns {Promise<object>} The parsed JSON response.
   * @throws {Error} With server error message or HTTP status text.
   */
  async request(endpoint, options = {}) {
    const res = await fetch(endpoint, {
      headers: { "Content-Type": "application/json" },
      ...options,
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.title || err.message || "Erro");
    }
    return res.json();
  }

  /** GET request. @param {string} endpoint @returns {Promise<object>} */
  get(endpoint) {
    return this.request(endpoint);
  }

  /** POST request with JSON body. @param {string} endpoint @param {object} body @returns {Promise<object>} */
  post(endpoint, body) {
    return this.request(endpoint, {
      method: "POST",
      body: JSON.stringify(body),
    });
  }

  /** PUT request with JSON body. @param {string} endpoint @param {object} body @returns {Promise<object>} */
  put(endpoint, body) {
    return this.request(endpoint, {
      method: "PUT",
      body: JSON.stringify(body),
    });
  }

  /** DELETE request. @param {string} endpoint @returns {Promise<object>} */
  delete(endpoint) {
    return this.request(endpoint, { method: "DELETE" });
  }
}
